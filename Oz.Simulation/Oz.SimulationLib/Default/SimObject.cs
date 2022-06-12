using Oz.SimulationLib.Contracts;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Oz.SimulationLib.Default;

public sealed class SimObject : ISimObject
{
    private readonly ConcurrentDictionary<Type, List<ISimComponent>> _components = new();
    private readonly ConcurrentDictionary<string, object> _properties = new();

    public SimObject(Guid id, string? name)
    {
        Id = id;
        Name = name ?? GetDefaultName();
    }

    public Guid Id { get; }
    public string Name { get; }

    public async Task InitializeAsync(ISimContext simContext)
    {
        foreach (var (type, typedComponents) in _components)
        {
            foreach (var component in typedComponents)
            {
                await component.InitializeAsync(simContext).ConfigureAwait(false);
            }
        }
    }

    public async Task UpdateAsync(ISimContext simContext)
    {
        foreach (var (type, typedComponents) in _components)
        {
            foreach (var component in typedComponents)
            {
                await component.UpdateAsync(simContext).ConfigureAwait(false);
            }
        }
    }

    public async Task DestroyAsync(ISimContext simContext)
    {
        foreach (var (type, typedComponents) in _components)
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
            var components = new List<ISimComponent>() {component};
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
            _components.TryAdd(typeof(T), new List<ISimComponent>() {instance});
        }
    }

    public IEnumerable<ISimComponent> GetComponents()
    {
        List<ISimComponent> allComponents = new();
        foreach (var (type, components) in _components)
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

    public T? GetComponent<T>() where T : ISimComponent =>
        throw new NotImplementedException();

    public ISimComponent? GetComponent(Guid id) =>
        throw new NotImplementedException();

    public IEnumerable<ISimComponent> GetComponents(string name) =>
        throw new NotImplementedException();

    public IEnumerable<T> GetComponents<T>(Func<T, bool> predicate) =>
        throw new NotImplementedException();

    public IEnumerable<ISimComponent> GetComponents(Func<ISimComponent, bool> predicate) =>
        throw new NotImplementedException();

    private string GetDefaultName() => nameof(SimObject);
}