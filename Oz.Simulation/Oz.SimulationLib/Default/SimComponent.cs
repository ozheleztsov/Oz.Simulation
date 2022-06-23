using Oz.SimulationLib.Contracts;
using Oz.SimulationLib.Exceptions;

namespace Oz.SimulationLib.Default;

public abstract class SimComponent : ISimComponent
{
    private readonly ISimContext _context;
    private bool _destroyed;
    private bool _initialized;

    internal SimComponent(ISimContext context, ISimObject owner, Guid? id = null, string? name = null)
    {
        _context = context;
        Id = id ?? Guid.NewGuid();
        Name = name ?? ConstructDefaultName(owner);
        Owner = owner;
    }

    public Guid Id { get; }
    public virtual string Name { get; }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            throw new SimulationException($"Component {this} already initialized");
        }

        if (_destroyed)
        {
            throw new SimulationException($"Component {this} already destroyed");
        }

        await OnInitializeAsync().ConfigureAwait(false);
        _initialized = true;
    }

    public async Task UpdateAsync()
    {
        if (!_initialized)
        {
            throw new SimulationException($"Component {this} is not initialized");
        }

        await OnUpdateAsync().ConfigureAwait(false);
    }

    public async Task DestroyAsync()
    {
        if (_destroyed)
        {
            throw new SimulationException($"Component {this} already destroyed");
        }

        await OnDestroyAsync();
        _destroyed = true;
    }

    public ISimObject Owner { get; }

    public async Task TryInitializeAsync()
    {
        if (!_initialized && !_destroyed)
        {
            await InitializeAsync().ConfigureAwait(false);
        }
    }

    public virtual async Task OnInitializeAsync() =>
        await Task.CompletedTask;

    public virtual async Task OnUpdateAsync() =>
        await Task.CompletedTask;

    public virtual async Task OnDestroyAsync() =>
        await Task.CompletedTask;

    private string ConstructDefaultName(ISimObject owner) =>
        $"{owner.Name}_{GetType().Name}";

    public override string ToString() =>
        $"{Id}:{Name}";
}