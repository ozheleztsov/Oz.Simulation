namespace Oz.SimulationLib.Contracts;

public interface ISimWorld : ISimEntity
{
    ISimLevel? ActiveLevel { get; }

    IEnumerable<ISimLevel> Levels { get; }

    Task AddLevelAsync(ISimLevel simLevel);

    Task<ISimLevel> AddLevelAsync(string name);

    void MakeActive(Guid levelId);
}