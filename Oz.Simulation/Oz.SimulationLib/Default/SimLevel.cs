using Oz.SimulationLib.Contracts;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Oz.SimulationLib.Default;

public sealed class SimLevel : ISimLevel
{
    private readonly ConcurrentDictionary<Guid, ISimObject> _simObjects = new();
    private bool _destroyed;
    public SimLevel(Guid id, string? name = null)
    {
        Id = id;
        Name = name ?? GetDefaultName();
    }

    public Guid Id { get; }
    public string Name { get; }

    public async Task InitializeAsync(ISimContext simContext)
    {
        if (_destroyed)
        {
            return;
        }
        
        var simObjects = _simObjects.Values.ToImmutableArray();
        foreach (var simObject in simObjects)
        {
            await simObject.InitializeAsync(simContext).ConfigureAwait(false);
        }
    }

    public async Task UpdateAsync(ISimContext simContext)
    {
        if (_destroyed)
        {
            return;
        }
        foreach (var (_, simObject) in _simObjects)
        {
            await simObject.UpdateAsync(simContext).ConfigureAwait(false);
        }
    }

    public async Task DestroyAsync(ISimContext simContext)
    {
        if (_destroyed)
        {
            return;
        }

        _destroyed = true;
        var simObjects = _simObjects.Values.ToImmutableArray();
        foreach (var simObject in simObjects)
        {
            await simObject.DestroyAsync(simContext).ConfigureAwait(false);
        }
    }

    public void AddObject(ISimObject simObject)
    {
        if (_simObjects.ContainsKey(simObject.Id))
        {
            throw new ArgumentException(
                $"Unable to add object {simObject} to level. Object with such id already exists");
        }

        _simObjects.TryAdd(simObject.Id, simObject);
    }

    public void RemoveObject(Guid id) =>
        _simObjects.TryRemove(id, out _);

    public async Task<ISimObject?> FindAsync(Guid id) =>
        await Task.FromResult(!_simObjects.TryGetValue(id, out var simObject) ? simObject : null);

    public async Task<IEnumerable<ISimObject>> FindAsync(string name) =>
        await Task.FromResult<IEnumerable<ISimObject>>(_simObjects.Values.Where(so => so.Name == name)
            .ToImmutableArray());

    public async Task<IEnumerable<ISimObject>> FindAsync(Func<ISimObject, bool> predicate) =>
        await Task.FromResult<IEnumerable<ISimObject>>(_simObjects.Values.Where(predicate).ToImmutableArray());

    public async Task<IEnumerable<T>> FindComponentsAsync<T>() where T : ISimComponent
    {
        var resultComponents = new ConcurrentBag<T>();

        ParallelOptions parallelOptions = new()
        {
            MaxDegreeOfParallelism = _simObjects.Count == 0 ? 10 : _simObjects.Count
        };

        await Parallel.ForEachAsync(_simObjects.Values, parallelOptions, async (so, _) =>
        {
            var components = so.GetComponents<T>();
            foreach (var foundComponent in components)
            {
                resultComponents.Add(foundComponent);
            }

            await ValueTask.CompletedTask;
        });

        return resultComponents.ToImmutableArray();
    }

    public async Task<IEnumerable<T>> FindComponentsAsync<T>(Func<T, bool> predicate) where T : ISimComponent
    {
        var resultComponents = new ConcurrentBag<T>();

        ParallelOptions parallelOptions = new()
        {
            MaxDegreeOfParallelism = _simObjects.Count == 0 ? 10 : _simObjects.Count
        };

        await Parallel.ForEachAsync(_simObjects.Values, parallelOptions, async (so, _) =>
        {
            var components = so.GetComponents(predicate);
            foreach (var foundComponent in components)
            {
                resultComponents.Add(foundComponent);
            }

            await ValueTask.CompletedTask;
        });

        return resultComponents.ToImmutableArray();
    }

    public async Task<IEnumerable<T>> FindComponentsAsync<T>(Guid objectId) where T : ISimComponent
    {
        var resultComponents = new ConcurrentBag<T>();

        ParallelOptions parallelOptions = new()
        {
            MaxDegreeOfParallelism = _simObjects.Count == 0 ? 10 : _simObjects.Count
        };

        await Parallel.ForEachAsync(_simObjects.Values, parallelOptions, async (so, _) =>
        {
            var components = so.GetComponents<T>(c => c.Id == objectId);
            foreach (var foundComponent in components)
            {
                resultComponents.Add(foundComponent);
            }

            await ValueTask.CompletedTask;
        });

        return resultComponents.ToImmutableArray();
    }

    public async Task<T?> FindComponentAsync<T>(Guid objectId) where T : ISimComponent
    {
        if (_simObjects.TryGetValue(objectId, out var simObject))
        {
            return await Task.FromResult(simObject.GetComponent<T>());
        }

        return default;
    }

    private string GetDefaultName() => $"{nameof(SimLevel)}";

    public override string ToString() => $"[Lvl: {Id}:{Name}]";
}