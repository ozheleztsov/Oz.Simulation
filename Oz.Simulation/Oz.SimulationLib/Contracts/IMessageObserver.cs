namespace Oz.SimulationLib.Contracts;

public interface IMessageObserver<in TMessage> : IMessageObserver where TMessage : class
{
    Task ReceiveAsync(TMessage? message);

    Task IMessageObserver.ReceiveAsync(object message) => ReceiveAsync(message as TMessage);
}

public interface IMessageObserver
{
    Task ReceiveAsync(object message);
}