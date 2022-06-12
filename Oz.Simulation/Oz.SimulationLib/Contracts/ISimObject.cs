using System.Collections.Concurrent;

namespace Oz.SimulationLib.Contracts;

public interface ISimObject : ISimEntity
{
    void SetProperty(string key, object? value);
    
    T? GetProperty<T>(string key);

    IEnumerable<KeyValuePair<string, object>> Properties { get; }

    T AddComponent<T>(string? name = null) where T : ISimComponent;

    void AddComponent<T>(T instance) where T : ISimComponent;

    IEnumerable<ISimComponent> GetComponents();

    IEnumerable<T> GetComponents<T>() where T : ISimComponent;

    T? GetComponent<T>() where T : ISimComponent;

    ISimComponent? GetComponent(Guid id);

    IEnumerable<ISimComponent> GetComponents(string name);

    IEnumerable<T> GetComponents<T>(Func<T, bool> predicate);

    IEnumerable<ISimComponent> GetComponents(Func<ISimComponent, bool> predicate);
}