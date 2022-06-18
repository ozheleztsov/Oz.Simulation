using Oz.SimulationLib.Contracts;
using System.Text.Json;

namespace Oz.SimulationLib.Default;

public sealed class MessageChannel : IMessageChannel, IAsyncDisposable
{
    private readonly Dictionary<Type, List<IMessageObserver>> _observers = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private bool _disposed;

    public ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return ValueTask.CompletedTask;
        }

        _disposed = true;
        _semaphore.Dispose();
        return ValueTask.CompletedTask;
    }


    public async Task<IAsyncDisposable> RegisterAsync<TMessage>(IMessageObserver<TMessage> observer)
        where TMessage : class
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);

        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(MessageChannel));
        }

        try
        {
            if (_observers.ContainsKey(typeof(TMessage)))
            {
                var typedObservers = _observers[typeof(TMessage)];
                typedObservers.Add(observer);
            }
            else
            {
                _observers.TryAdd(typeof(TMessage), new List<IMessageObserver> {observer});
            }

            IAsyncDisposable registration = new MessageRegistration<TMessage>(this, observer);
            return registration;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task UnregisterAsync<TMessage>(IMessageObserver<TMessage> observer) where TMessage : class
    {
        await _semaphore.WaitAsync();

        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(MessageChannel));
        }

        try
        {
            var type = typeof(TMessage);
            if (_observers.ContainsKey(type))
            {
                var typedObservers = _observers[type];
                typedObservers.Remove(observer);
                if (typedObservers.Count == 0)
                {
                    _observers.Remove(type, out _);
                }
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SendMessageAsync<TMessage>(TMessage message) where TMessage : class
    {
        await _semaphore.WaitAsync();
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(MessageChannel));
        }

        try
        {
            var type = typeof(TMessage);
            if (_observers.ContainsKey(type))
            {
                var typedObservers = _observers[type];
                foreach (var observer in typedObservers)
                {
                    await observer.ReceiveAsync(message);
                }
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public override string ToString()
    {
        _semaphore.Wait();
        try
        {
            List<ObserverStats> statsList = new();
            foreach (var (type, observers) in _observers)
            {
                statsList.Add(new ObserverStats(type.Name, observers.Count));
            }

            return JsonSerializer.Serialize(statsList,
                new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private sealed record ObserverStats(string MessageTypeName, int ObserverCount);
}