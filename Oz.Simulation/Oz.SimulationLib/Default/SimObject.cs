using Microsoft.Extensions.Logging;
using Oz.SimulationLib.Contracts;
using Oz.SimulationLib.Exceptions;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Oz.SimulationLib.Default;

public sealed class SimObject : ISimObject
{
    private readonly ISimContext _context;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<SimObject> _logger;
    private readonly ConcurrentDictionary<Type, ConcurrentBag<ISimComponent>> _components = new();
    private readonly ConcurrentDictionary<string, object> _properties = new();
    private bool _destroyed;
    private bool _initialized;

    public SimObject(ISimContext context, Guid id, string? name, ILoggerFactory loggerFactory)
    {
        _context = context;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<SimObject>();
        Id = id;
        Name = name ?? GetDefaultName();
    }

    public Guid Id { get; }
    public string Name { get; }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            throw new SimulationException($"{this} already initialized");
        }
        if (_destroyed)
        {
            throw new SimulationException($"{this} already destroyed");
        }
        foreach (var (_, typedComponents) in _components)
        {
            foreach (var component in typedComponents)
            {
                await component.TryInitializeAsync().ConfigureAwait(false);
            }
        }
        _logger.LogInformation("SimObject {Name} is initialized", ToString());
        _initialized = true;
    }

    public async Task UpdateAsync()
    {
        if (!_initialized)
        {
            _logger.LogWarning("SimObject {Name} is not initialized, no updates", Name);
            return;
        }
        if (_destroyed)
        {
            _logger.LogWarning("SimObject {Name} is destroyed, no updates", Name);
            return;
        }
        
        foreach (var (_, typedComponents) in _components)
        {
            
            foreach (var component in typedComponents)
            {
                await component.UpdateAsync().ConfigureAwait(false);
            }
        }
        //_logger.LogInformation("SimObject {Name} is updated", ToString());
    }

    public async Task DestroyAsync()
    {
        if (_destroyed)
        {
            return;
        }

        _destroyed = true;
        
        foreach (var (_, typedComponents) in _components)
        {
            foreach (var component in typedComponents)
            {
                await component.DestroyAsync().ConfigureAwait(false);
            }
        }
        _logger.LogInformation("SimObject {Name} is destroyed", ToString());
    }

    public void SetProperty(string key, object? value)
    {
        if (value is null)
        {
            _properties.TryRemove(key, out _);
        }
        else
        {
            _properties[key] = value;
        }
    }

    public T? GetProperty<T>(string key)
    {
        if (_properties.TryGetValue(key, out var value))
        {
            return (T)value;
        }

        return default;
    }

    public IEnumerable<KeyValuePair<string, object>> Properties =>
        _properties.Select(kvp => kvp).ToImmutableList();

    public async Task<T> AddComponentAsync<T>(string? name = null) where T : ISimComponent
    {
        var component = (T?)Activator.CreateInstance(typeof(T),  _context, this, _loggerFactory.CreateLogger<T>(), Guid.NewGuid(), name);
        if (component is null)
        {
            throw new InvalidOperationException($"Impossible to create component of type {typeof(T).Name}");
        }

        if (_components.TryGetValue(typeof(T), out var typedComponents))
        {
            typedComponents.Add(component);
        }
        else
        {
            var components = new ConcurrentBag<ISimComponent>() {component};
            _components.TryAdd(typeof(T), components);
        }

        if (_initialized)
        {
            await component.TryInitializeAsync().ConfigureAwait(false);
        }

        return component;
    }

    public async Task AddComponentAsync<T>(T instance) where T : ISimComponent
    {
        if (instance.Owner != null && instance.Owner != this)
        {
            throw new ArgumentException(
                $"Adding component failure. Component {instance.Id}:{instance.Name} already attached to object {instance.Owner.Id}:{instance.Owner.Name}");
        }

        if (instance.Owner is null)
        {
            throw new ArgumentException(
                $"Adding component failure. Component {instance.Id}:{instance.Name} doesn't have owner");
        }

        if (_components.TryGetValue(typeof(T), out var existingComponents))
        {
            if (existingComponents.Any(existingComponent => ReferenceEquals(existingComponent, instance)))
            {
                throw new ArgumentException(
                    $"Adding component failure. Component {instance.Id}:{instance.Name} already attached to object {instance.Owner.Id}:{instance.Owner.Name}");
            }

            existingComponents.Add(instance);
        }
        else
        {
            _components.TryAdd(typeof(T), new ConcurrentBag<ISimComponent>() {instance});
        }

        if (_initialized)
        {
            await instance.TryInitializeAsync().ConfigureAwait(false);
        }
    }

    public IEnumerable<ISimComponent> GetComponents()
    {
        List<ISimComponent> allComponents = new();
        foreach (var (_, components) in _components)
        {
            allComponents.AddRange(components);
        }

        return allComponents.ToImmutableArray();
    }

    public IEnumerable<T> GetComponents<T>() where T : ISimComponent
    {
        if (!_components.ContainsKey(typeof(T)))
        {
            return Array.Empty<T>();
        }

        var components = _components[typeof(T)];
        return components.Cast<T>().ToImmutableArray();
    }

    public T? GetComponent<T>() where T : ISimComponent
    {
        if (!_components.ContainsKey(typeof(T)))
        {
            return default;
        }

        var typedComponents = _components[typeof(T)];
        if (!typedComponents.Any())
        {
            return default;
        }

        return (T)typedComponents.First();
    }

    public ISimComponent? GetComponent(Guid id)
    {
        foreach (var (_, typedComponents) in _components)
        {
            foreach (var component in typedComponents.Where(component => component.Id == id))
            {
                return component;
            }
        }

        return default;
    }

    public IEnumerable<ISimComponent> GetComponents(string name)
    {
        List<ISimComponent> resultComponents = new();
        foreach (var (_, typedComponents) in _components)
        {
            resultComponents.AddRange(typedComponents.Where(component => component.Name == name));
        }

        return resultComponents.ToImmutableArray();
    }

    public IEnumerable<T> GetComponents<T>(Func<T, bool> predicate)
    {
        if (!_components.ContainsKey(typeof(T)))
        {
            return Array.Empty<T>();
        }

        var typedComponents = _components[typeof(T)];
        if (!typedComponents.Any())
        {
            return Array.Empty<T>();
        }

        var resultComponents = typedComponents.Where(component => predicate((T)component)).Cast<T>().ToList();
        return resultComponents.ToImmutableArray();
    }

    public IEnumerable<ISimComponent> GetComponents(Func<ISimComponent, bool> predicate)
    {
        List<ISimComponent> resultComponents = new();
        foreach (var (_, typedComponents) in _components)
        {
            resultComponents.AddRange(typedComponents.Where(predicate));
        }

        return resultComponents.ToImmutableArray();
    }

    public async Task<bool> RemoveComponentAsync(Guid id)
    {
        foreach (var (_, typedComponents) in _components)
        {
            var targetComponent = typedComponents.FirstOrDefault(c => c.Id == id);
            var success = false;
            if (targetComponent != null)
            {
                var arr = typedComponents.ToImmutableArray();
                typedComponents.Clear();
                foreach (var c in arr)
                {
                    if (!ReferenceEquals(c, targetComponent))
                    {
                        typedComponents.Add(c);
                    }
                }

                success = true;
            }

            if (success && typedComponents.Count == 0 && targetComponent != null)
            {
                _components.TryRemove(targetComponent.GetType(), out _);
            }

            if (success)
            {
                if (targetComponent != null)
                {
                    await targetComponent.DestroyAsync().ConfigureAwait(false);
                }

                return success;
            }
        }

        return false;
    }

    public async Task RemoveComponentsAsync<T>() where T : ISimComponent
    {
        if (_components.TryRemove(typeof(T), out var removedComponents))
        {
            foreach (var component in removedComponents)
            {
                await component.DestroyAsync().ConfigureAwait(false);
            }
        }
    }

    private static string GetDefaultName() => nameof(SimObject);

    public override string ToString() => $"[Obj: {Id}:{Name}]";
}