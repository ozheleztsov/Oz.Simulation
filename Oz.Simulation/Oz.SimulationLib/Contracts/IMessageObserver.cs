namespace Oz.SimulationLib.Contracts;

public interface IMessageObserver<in TMessage> : IMessageObserver where TMessage : class
{
    Task IMessageObserver.ReceiveAsync(object message) => ReceiveAsync(message as TMessage);
    Task ReceiveAsync(TMessage? message);
}

public interface IMessageObserver
{
    Task ReceiveAsync(object message);
}