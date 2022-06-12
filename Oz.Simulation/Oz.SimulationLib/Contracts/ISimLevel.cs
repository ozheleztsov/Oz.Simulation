namespace Oz.SimulationLib.Contracts;

public interface ISimLevel : ISimEntity
{
    void AddObject(ISimObject simObject);

    Task<ISimObject> FindAsync(Guid id);

    Task<IEnumerable<ISimObject>> FindAsync(string name);

    Task<IEnumerable<ISimObject>> FindAsync(Func<ISimObject, bool> predicate);

    Task<IEnumerable<T>> FindComponentsAsync<T>() where T : ISimComponent;

    Task<IEnumerable<T>> FindComponentsAsync<T>(Func<T, bool> predicate) where T : ISimComponent;

    Task<IEnumerable<T>> FindComponentsAsync<T>(Guid objectId) where T : ISimComponent;

    Task<T?> FindComponentAsync<T>(Guid objectId) where T : ISimComponent;
    
}