namespace Oz.SimulationLib.Contracts;

public interface ISimulator
{
    Task PrepareAsync(ISimContext simContext);
    Task SimulateStepAsync(ISimContext simContext);
    Task FinishAsync(ISimContext simContext);
}