namespace Oz.SimulationLib.Contracts;

public interface ISimulator
{
    Task PrepareSimulationAsync();
    Task<Task> StartSimulationAsync();
    Task FinishSimulationAsync();
    ISimContext Context { get; }
    ISimWorld World { get; }
    bool IsPrepared { get; }
}