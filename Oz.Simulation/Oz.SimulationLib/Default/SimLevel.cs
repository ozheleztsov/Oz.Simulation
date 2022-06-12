using Oz.SimulationLib.Contracts;

namespace Oz.SimulationLib.Default;

public sealed class SimLevel : ISimLevel
{
    public Guid Id { get; }
    public string Name { get; }
    public Task InitializeAsync(ISimContext simContext) =>
        throw new NotImplementedException();

    public Task UpdateAsync(ISimContext simContext) =>
        throw new NotImplementedException();

    public Task DestroyAsync(ISimContext simContext) =>
        throw new NotImplementedException();

    public void AddObject(ISimObject simObject) =>
        throw new NotImplementedException();

    public Task<ISimObject> FindAsync(Guid id) =>
        throw new NotImplementedException();

    public Task<IEnumerable<ISimObject>> FindAsync(string name) =>
        throw new NotImplementedException();

    public Task<IEnumerable<ISimObject>> FindAsync(Func<ISimObject, bool> predicate) =>
        throw new NotImplementedException();

    public Task<IEnumerable<T>> FindComponentsAsync<T>() where T : ISimComponent =>
        throw new NotImplementedException();

    public Task<IEnumerable<T>> FindComponentsAsync<T>(Func<T, bool> predicate) where T : ISimComponent =>
        throw new NotImplementedException();

    public Task<IEnumerable<T>> FindComponentsAsync<T>(Guid objectId) where T : ISimComponent =>
        throw new NotImplementedException();

    public Task<T?> FindComponentAsync<T>(Guid objectId) where T : ISimComponent =>
        throw new NotImplementedException();
}