using Oz.SimulationLib.Contracts;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Oz.SimulationLib.Default;

public sealed class SimWorld : ISimWorld
{
    private readonly ISimulator _simulator;
    private readonly ConcurrentDictionary<Guid, ISimLevel> _simLevels = new();
    
    public SimWorld(Guid id, string name, ISimulator simulator)
    {
        Id = id;
        Name = name;
        _simulator = simulator;
    }

    public Guid Id { get; }
    public string Name { get; }

    public async Task InitializeAsync(ISimContext simContext)
    {
        foreach (var (_, simLevel) in _simLevels)
        {
            simContext.Prepare(this, simLevel);
            await simLevel.InitializeAsync(simContext).ConfigureAwait(false);
        }
    }
    
    public async Task UpdateAsync(ISimContext simContext)
    {
        var activeLevel = ActiveLevel;
        if (activeLevel != null)
        {
            simContext.Prepare(this, activeLevel);
            await activeLevel.UpdateAsync(simContext).ConfigureAwait(false);
        }
    }

    public async Task DestroyAsync(ISimContext simContext)
    {
        foreach (var (_, simLevel) in _simLevels)
        {
            simContext.Prepare(this, simLevel);
            await simLevel.DestroyAsync(simContext).ConfigureAwait(false);
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

        var context = _simulator.Context;
        if (_simLevels.TryAdd(simLevel.Id, simLevel))
        {
            context.Prepare(this, simLevel);
            await simLevel.InitializeAsync(_simulator.Context).ConfigureAwait(false);
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