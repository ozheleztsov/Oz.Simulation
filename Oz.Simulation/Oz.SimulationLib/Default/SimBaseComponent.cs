using Oz.SimulationLib.Contracts;

namespace Oz.SimulationLib.Default;

public abstract class SimComponent : ISimComponent
{
    internal SimComponent(ISimObject owner, Guid? id = null, string? name = null)
    {
        Id = id ?? Guid.NewGuid();
        Name = name ?? ConstructDefaultName(owner);
        Owner = owner;
    }

    public Guid Id { get; }
    public virtual string Name { get; }

    public virtual Task InitializeAsync(ISimContext simContext) =>
        Task.CompletedTask;

    public virtual Task UpdateAsync(ISimContext simContext) =>
        Task.CompletedTask;

    public virtual Task DestroyAsync(ISimContext simContext) =>
        Task.CompletedTask;

    public ISimObject Owner { get; }

    private string ConstructDefaultName(ISimObject owner) =>
        $"{owner.Name}_{GetType().Name}";
}