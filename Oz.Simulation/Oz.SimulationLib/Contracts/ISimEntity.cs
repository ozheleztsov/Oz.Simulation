namespace Oz.SimulationLib.Contracts;

public interface ISimEntity
{
    Guid Id { get; }
    string Name { get; }
    Task InitializeAsync();
    Task UpdateAsync();
    Task DestroyAsync();
}