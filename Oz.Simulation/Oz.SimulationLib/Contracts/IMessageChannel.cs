namespace Oz.SimulationLib.Contracts;

/// <summary>
///     Represent channel for sending messages to recipients
/// </summary>
public interface IMessageChannel
{
    /// <summary>
    ///     Register observer at the channel
    /// </summary>
    /// <param name="observer">Observer that receives messages</param>
    /// <typeparam name="TMessage">Message type</typeparam>
    /// <returns>Registration object</returns>
    Task<IAsyncDisposable> RegisterAsync<TMessage>(IMessageObserver<TMessage> observer) where TMessage : class;

    /// <summary>
    ///     Unregister observer from the channel
    /// </summary>
    /// <param name="observer">Observer that was registered previously</param>
    /// <typeparam name="TMessage">Message type</typeparam>
    /// <returns>Operation</returns>
    Task UnregisterAsync<TMessage>(IMessageObserver<TMessage> observer) where TMessage : class;

    /// <summary>
    ///     Send message to the channel
    /// </summary>
    /// <param name="message">Message object</param>
    /// <typeparam name="TMessage">Message type</typeparam>
    /// <returns>Operation</returns>
    Task SendMessageAsync<TMessage>(TMessage message) where TMessage : class;
}