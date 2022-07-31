using Microsoft.Extensions.Logging;
using Oz.SimulationLib.Contracts;
using Oz.SimulationLib.Exceptions;

namespace Oz.SimulationLib.Default;

public abstract class SimComponent : ISimComponent
{
    private bool _destroyed;
    private bool _initialized;

    public SimComponent(ISimContext context, ISimObject owner, ILogger logger, Guid? id = null, string? name = null)
    {
        Context = context;
        Logger = logger;
        Id = id ?? Guid.NewGuid();
        Name = name ?? ConstructDefaultName(owner);
        Owner = owner;
    }

    protected bool Destroyed => _destroyed;

    public ISimContext Context { get; }

    protected ILogger Logger { get; }

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
        Logger.LogInformation("Component {Name} is initialized", Name);
        _initialized = true;
    }

    public async Task UpdateAsync()
    {
        if (!_initialized)
        {
            Logger.LogWarning("SimComponent {Name} is not initialize, update fails", Name);
            return;
        }

        await OnUpdateAsync().ConfigureAwait(false);
        //Logger.LogInformation("Component {Name} is updated", Name);
    }

    public async Task DestroyAsync()
    {
        if (_destroyed)
        {
            throw new SimulationException($"Component {this} already destroyed");
        }

        await OnDestroyAsync();
        _destroyed = true;
        Logger.LogInformation("Component {Name} is destroyed", Name);
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