using Oz.SimulationLib.Contracts;

namespace Oz.SimulationLib.Default;

public class SimContext : ISimContext
{
    public SimContext(ITime time) =>
        Time = time;

    public ISimWorld? World { get; private set; }
    public ISimLevel? Level { get; private set; }
    public ITime Time { get; }

    public void Prepare(ISimWorld? world, ISimLevel? level) =>
        (World, Level) = (world, level);
}