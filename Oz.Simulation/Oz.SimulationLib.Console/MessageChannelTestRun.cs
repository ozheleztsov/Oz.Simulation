using Oz.SimulationLib.Contracts;
using Oz.SimulationLib.Default;
using System.Text.Json;

namespace Oz.SimulationLib.Console;

public class MessageChannelTestRun : IAsyncDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly IMessageChannel _messageChannel = new MessageChannel();
    private readonly List<Task> _producers = new();
    private readonly List<IAsyncDisposable> _regisrations = new();
    private bool _disposed;
    private bool _isRun;
    
    public async Task RunAsync()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(MessageChannelTestRun));
        }
        
        var message1Counter = 0;
        var message2Counter = 0;

        for (var i = 0; i < 5; i++)
        {
            _producers.Add(CreateMessageProducer(Message1Produce, TimeSpan.FromSeconds(0.1),
                _cancellationTokenSource.Token));
            _producers.Add(CreateMessageProducer(Message2Produce, TimeSpan.FromSeconds(0.2),
                _cancellationTokenSource.Token));
        }

        var counter = 0;
        for (var i = 0; i < 30; i++)
        {
            var observer = new MessageObserver<Message1>($"{++counter}");
            _regisrations.Add(await _messageChannel.RegisterAsync(observer));
        }

        var msg2Observer = new MessageObserver<Message2>("SPECIAL");
        _regisrations.Add(await _messageChannel.RegisterAsync(msg2Observer));

        Message1 Message1Produce()
        {
            return new Message1($"Message1 - {++message1Counter}");
        }

        Message2 Message2Produce()
        {
            return new Message2($"Message2 - {++message2Counter}");
        }

        _isRun = true;
    }

    private async Task StopAsync()
    {
        try
        {
            _cancellationTokenSource.Cancel();
            await Task.WhenAll(_producers);
        }
        catch (TaskCanceledException exception)
        {
            System.Console.WriteLine("Tasks were cancelled");
        }

        foreach (var reg in _regisrations)
        {
            await reg.DisposeAsync();
        }
        _regisrations.Clear();
    }


    private Task CreateMessageProducer<TMessage>(Func<TMessage> produce, TimeSpan intervalBetweenMessages,
        CancellationToken cancellationToken)
        where TMessage : class =>
        Task.Run(async () =>
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _messageChannel.SendMessageAsync(produce());
                await Task.Delay(intervalBetweenMessages, cancellationToken);
            }
        }, cancellationToken);

    private class Message1
    {
        public Message1(string data) =>
            Data = data;

        public string Data { get; }
    }

    private class Message2
    {
        public Message2(string data) =>
            Data = data;

        public string Data { get; }
    }

    private class MessageObserver<TMessage> : IMessageObserver<TMessage> where TMessage : class
    {
        private readonly string _name;

        public MessageObserver(string name) =>
            _name = name;

        public async Task ReceiveAsync(TMessage? message)
        {
            if (message is null)
            {
                return;
            }

            var messageJson = JsonSerializer.Serialize(message,
                new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});

            System.Console.WriteLine($"Observer1 {nameof(MessageObserver<TMessage>)}:{_name} received: {messageJson}");
            await Task.CompletedTask;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        if (_isRun)
        {
            _isRun = false;
            await StopAsync();
        }

        if (_messageChannel is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync();
        }
    }
}