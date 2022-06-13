namespace Oz.SimulationLib.Contracts;

public interface ISimWorld : ISimEntity
{
    ISimLevel? ActiveLevel { get; }

    IEnumerable<ISimLevel> Levels { get; }

    Task AddLevelAsync(ISimLevel simLevel);

    void MakeActive(Guid levelId);
}