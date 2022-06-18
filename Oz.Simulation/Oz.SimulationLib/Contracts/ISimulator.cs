namespace Oz.SimulationLib.Contracts;

public interface ISimulator
{
    Task PrepareSimulationAsync();
    Task StartSimulationAsync();
    Task FinishSimulationAsync();
    ISimContext? Context { get; }
    ISimWorld? World { get; }
}