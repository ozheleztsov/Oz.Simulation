namespace Oz.SimulationLib.Contracts;

public interface ISimContext
{
    ISimWorld? World { get; }
    ISimLevel? Level { get; }
    ITime Time { get; }
}