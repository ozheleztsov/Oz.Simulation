using Oz.SimulationLib.Contracts;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Oz.SimulationLib.Default;

public sealed class SimObject : ISimObject
{
    private readonly ConcurrentDictionary<Type, List<ISimComponent>> _components = new();
    private readonly ConcurrentDictionary<string, object> _properties = new();
    private bool _destoyed;

    public SimObject(Guid id, string? name)
    {
        Id = id;
        Name = name ?? GetDefaultName();
    }

    public Guid Id { get; }
    public string Name { get; }

    public async Task InitializeAsync(ISimContext simContext)
    {
        if (_destoyed)
        {
            return;
        }
        
        foreach (var (_, typedComponents) in _components)
        {
            foreach (var component in typedComponents)
            {
                await component.InitializeAsync(simContext).ConfigureAwait(false);
            }
        }
    }

    public async Task UpdateAsync(ISimContext simContext)
    {
        if (_destoyed)
        {
            return;
        }
        
        foreach (var (_, typedComponents) in _components)
        {
            foreach (var component in typedComponents)
            {
                await component.UpdateAsync(simContext).ConfigureAwait(false);
            }
        }
    }

    public async Task DestroyAsync(ISimContext simContext)
    {
        if (_destoyed)
        {
            return;
        }

        _destoyed = true;
        
        foreach (var (_, typedComponents) in _components)
        {
            foreach (var component in typedComponents)
            {
                await component.DestroyAsync(simContext).ConfigureAwait(false);
            }
        }
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

    public T AddComponent<T>(string? name = null) where T : ISimComponent
    {
        var component = (T?)Activator.CreateInstance(typeof(T), this, Guid.NewGuid(), name);
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
            var components = new List<ISimComponent> {component};
            _components.TryAdd(typeof(T), components);
        }

        return component;
    }

    public void AddComponent<T>(T instance) where T : ISimComponent
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
            _components.TryAdd(typeof(T), new List<ISimComponent> {instance});
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

    public bool RemoveComponent(Guid id)
    {
        foreach (var (_, typedComponents) in _components)
        {
            var targetComponent = typedComponents.FirstOrDefault(c => c.Id == id);
            var success = false;
            if (targetComponent != null)
            {
                success = typedComponents.Remove(targetComponent);
            }

            if (success && typedComponents.Count == 0 && targetComponent != null)
            {
                _components.TryRemove(targetComponent.GetType(), out _);
            }

            if (success)
            {
                return success;
            }
        }

        return false;
    }

    public void RemoveComponents<T>() where T : ISimComponent =>
        _components.TryRemove(typeof(T), out _);

    private static string GetDefaultName() => nameof(SimObject);

    public override string ToString() => $"[Obj: {Id}:{Name}]";
}