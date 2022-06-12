namespace Oz.SimulationLib.Contracts;

public interface ISimEntity
{
    Guid Id { get; }
    string Name { get; }
    Task InitializeAsync(ISimContext simContext);
    Task UpdateAsync(ISimContext simContext);
    Task DestroyAsync(ISimContext simContext);
}