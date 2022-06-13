using Oz.SimulationLib.Contracts;

namespace Oz.SimulationLib.Default;

public class SimContext : ISimContext
{
    public SimContext(ITime time, IMessageChannel messageChannel, ISimulator simulator)
    {
        Time = time;
        MessageChannel = messageChannel;
        Simulator = simulator;
    }

    public ISimWorld? World { get; private set; }
    public ISimLevel? Level { get; private set; }
    public ITime Time { get; }
    public IMessageChannel MessageChannel { get; }
    
    public ISimulator Simulator { get; }

    public void Prepare(ISimWorld? world, ISimLevel? level) =>
        (World, Level) = (world, level);
}