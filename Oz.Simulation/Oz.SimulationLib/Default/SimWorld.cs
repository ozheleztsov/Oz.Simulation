using Microsoft.Extensions.Logging;
using Oz.SimulationLib.Contracts;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Oz.SimulationLib.Default;

public sealed class SimWorld : ISimWorld
{
    private readonly ISimContext _context;
    private readonly ILogger<SimWorld> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ConcurrentDictionary<Guid, ISimLevel> _simLevels = new();
    private bool _destroyed;
    private bool _initialized;
    

    public SimWorld(ISimContext context, Guid id, string name, ILoggerFactory loggerFactory)
    {
        Id = id;
        Name = name;
        _context = context;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<SimWorld>();
    }

    public Guid Id { get; }
    public string Name { get; }

    public async Task InitializeAsync()
    {
        if (_destroyed)
        {
            throw new InvalidOperationException($"World {Name} was destroyed, initialization fails");
        }

        if (_initialized)
        {
            throw new InvalidOperationException($"World {Name} already initialized, initialization fails");
        }

        foreach (var (_, simLevel) in _simLevels)
        {
            _context.Prepare(this, simLevel);
            await simLevel.InitializeAsync().ConfigureAwait(false);
        }

        _initialized = true;
    }

    public async Task UpdateAsync()
    {
        if (!_initialized)
        {
            _logger.LogWarning("Cannot update uninitialized world {Name}", Name);
            return;
        }

        if (_destroyed)
        {
            _logger.LogWarning("Cannot update destroyed world {Name}", Name);
            return;
        }

        var activeLevel = ActiveLevel;
        if (activeLevel != null)
        {
            _context.Prepare(this, activeLevel);
            await activeLevel.UpdateAsync().ConfigureAwait(false);
        }
    }

    public async Task DestroyAsync()
    {
        if (_destroyed)
        {
            _logger.LogWarning("World {Name} already destroyed. Destroy fails", Name);
            return;
        }

        _destroyed = true;
        await DestroyAllLevelsAsync().ConfigureAwait(false);
    }

    public async Task DestroyAllLevelsAsync()
    {
        foreach (var (_, simLevel) in _simLevels)
        {
            _context.Prepare(this, simLevel);
            await simLevel.DestroyAsync().ConfigureAwait(false);
        }

        ActiveLevel = null;
        _simLevels.Clear();        
    }
    
    

    public ISimLevel? ActiveLevel { get; private set; }

    public IEnumerable<ISimLevel> Levels =>
        _simLevels.Values.ToImmutableArray();

    public async Task AddLevelAsync(ISimLevel simLevel)
    {
        if (_simLevels.ContainsKey(simLevel.Id))
        {
            throw new ArgumentException($"Unable to add level {simLevel} to the world. Level already added");
        }

        if (_simLevels.TryAdd(simLevel.Id, simLevel))
        {
            if (_initialized && !_destroyed)
            {
                _context.Prepare(this, simLevel);
                await simLevel.InitializeAsync().ConfigureAwait(false);
            }

            if (_simLevels.Count == 1 && ActiveLevel is null)
            {
                MakeActive(simLevel.Id);
            }
        }
    }

    public void MakeActive(Guid levelId)
    {
        if (!_simLevels.ContainsKey(levelId))
        {
            throw new ArgumentException($"Not found level {levelId}");
        }

        if (_simLevels.TryGetValue(levelId, out var newLevel))
        {
            ActiveLevel = newLevel;
        }
    }

    public async Task<ISimLevel> AddLevelAsync(string name)
    {
        var simLevel = new SimLevel(_context, Guid.NewGuid(), _loggerFactory, name);
        await AddLevelAsync(simLevel);
        return simLevel;
    }
}