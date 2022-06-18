using Oz.SimulationLib.Contracts;

namespace Oz.SimulationLib.Default;

public sealed class MessageRegistration<TMessage> : IAsyncDisposable where TMessage: class
{
    private readonly IMessageChannel _messageChannel;
    private readonly IMessageObserver<TMessage> _observer;
    private bool _disposed;

    public MessageRegistration(IMessageChannel messageChannel, IMessageObserver<TMessage> observer)
    {
        _messageChannel = messageChannel;
        _observer = observer;
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await _messageChannel.UnregisterAsync(_observer).ConfigureAwait(false);
        _disposed = true;
    }
}