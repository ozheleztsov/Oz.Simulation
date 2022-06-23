using Oz.SimulationLib.Contracts;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Oz.SimulationLib.Default;

public sealed class SimWorld : ISimWorld
{
    private readonly ISimContext _context;
    private readonly ConcurrentDictionary<Guid, ISimLevel> _simLevels = new();
    
    public SimWorld(ISimContext context, Guid id, string name)
    {
        Id = id;
        Name = name;
        _context = context;
    }

    public Guid Id { get; }
    public string Name { get; }

    public async Task InitializeAsync()
    {
        foreach (var (_, simLevel) in _simLevels)
        {
            _context.Prepare(this, simLevel);
            await simLevel.InitializeAsync().ConfigureAwait(false);
        }
    }
    
    public async Task UpdateAsync()
    {
        var activeLevel = ActiveLevel;
        if (activeLevel != null)
        {
            _context.Prepare(this, activeLevel);
            await activeLevel.UpdateAsync().ConfigureAwait(false);
        }
    }

    public async Task DestroyAsync()
    {
        foreach (var (_, simLevel) in _simLevels)
        {
            _context.Prepare(this, simLevel);
            await simLevel.DestroyAsync().ConfigureAwait(false);
        }
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
            _context.Prepare(this, simLevel);
            await simLevel.InitializeAsync().ConfigureAwait(false);
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
}