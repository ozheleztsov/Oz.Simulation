using Microsoft.Extensions.Logging;
using Oz.SimulationLib.Contracts;
using Oz.SimulationLib.Default.Messages;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Oz.SimulationLib.Default;

public sealed class SimLevel : ISimLevel
{
    private readonly ISimContext _context;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ConcurrentDictionary<Guid, ISimObject> _simObjects = new();
    private bool _destroyed, _initialized;
    private readonly ILogger<SimLevel> _logger;

    public SimLevel(ISimContext context, Guid id, ILoggerFactory loggerFactory, string? name = null)
    {
        _context = context;
        _loggerFactory = loggerFactory;
        Id = id;
        Name = name ?? GetDefaultName();
        _logger = loggerFactory.CreateLogger<SimLevel>();
    }

    public Guid Id { get; }
    public string Name { get; }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            throw new InvalidOperationException($"SimLevel {Name} already initialized.");
        }

        if (_destroyed)
        {
            throw new InvalidOperationException($"Impossible to initialize destroyed SimLevel {Name}");
        }

        var simObjects = _simObjects.Values.ToImmutableArray();
        foreach (var simObject in simObjects)
        {
            await simObject.InitializeAsync().ConfigureAwait(false);
        }

        _logger.LogInformation("SimLevel {Name} is initialized", Name);
        _initialized = true;
    }

    public async Task UpdateAsync()
    {
        if (!_initialized)
        {
            _logger.LogWarning("SimLevel {Name} is not initialized, update skipped", Name);
            return;
        }

        if (_destroyed)
        {
            _logger.LogWarning("SimLevel {Name} is destroyed, update skipped", Name);
            return;
        }

        foreach (var (_, simObject) in _simObjects)
        {
            await simObject.UpdateAsync().ConfigureAwait(false);
        }

        //_logger.LogInformation("SimLevel {Name} is updated", Name);
    }

    public async Task DestroyAsync()
    {
        if (_destroyed)
        {
            return;
        }

        _destroyed = true;
        var simObjects = _simObjects.Values.ToImmutableArray();
        foreach (var simObject in simObjects)
        {
            await simObject.DestroyAsync().ConfigureAwait(false);
        }
        _simObjects.Clear();
        _logger.LogInformation("SimLevel {Name} is destroyed", Name);
    }

    public async Task<SimObject> AddObjectAsync(string name)
    {
        var simObject = new SimObject(_context, Guid.NewGuid(), name, _loggerFactory);
        await AddObjectAsync(simObject).ConfigureAwait(false);
        return simObject;
    }

    public async Task AddObjectAsync(ISimObject simObject)
    {
        if (_simObjects.ContainsKey(simObject.Id))
        {
            throw new ArgumentException(
                $"Unable to add object {simObject} to level. Object with such id already exists");
        }

        var success = _simObjects.TryAdd(simObject.Id, simObject);
        if (_initialized && !_destroyed)
        {
            await simObject.TryInitializeAsync().ConfigureAwait(false);
        }

        if (success)
        {
            await _context.MessageChannel.SendMessageAsync(new ObjectAddedMessage(simObject, this));
        }
    }

    public async Task<ISimObject?> RemoveObjectAsync(Guid id)
    {
        if (_simObjects.TryRemove(id, out var obj))
        {
            await _context.MessageChannel.SendMessageAsync(new ObjectRemovedMessage(obj, this)).ConfigureAwait(false);
            return obj;
        }

        return null;
    }

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

    private static string GetDefaultName() => $"{nameof(SimLevel)}";

    public override string ToString() => $"[Lvl: {Id}:{Name}]";
}