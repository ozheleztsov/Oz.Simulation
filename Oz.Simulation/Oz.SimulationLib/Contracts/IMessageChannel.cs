namespace Oz.SimulationLib.Contracts;

public interface IMessageChannel
{
    Task<IAsyncDisposable> RegisterAsync<TMessage>(IMessageObserver<TMessage> observer);
    Task UnregisterAsync<TMessage>(IMessageObserver<TMessage> observer);
}

public interface IMessageObserver<in TMessage>
{
    Task ReceiveAsync(TMessage message);
}

public class MessageRegistration<TMessage> : IAsyncDisposable
{
    private readonly IMessageChannel _messageChannel;
    private readonly IMessageObserver<TMessage> _observer;
    private bool _disposed = false;

    public MessageRegistration(IMessageChannel messageChannel, IMessageObserver<TMessage> observer)
    {
        _messageChannel = messageChannel;
        _observer = observer;
    }

#pragma warning disable CA1816
    public async ValueTask DisposeAsync()
#pragma warning restore CA1816
    {
        if (_disposed)
        {
            return;
        }

        await _messageChannel.UnregisterAsync(_observer).ConfigureAwait(false);
        _disposed = true;
    }
}