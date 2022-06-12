namespace Oz.SimulationLib.Contracts;

public interface ISimComponent : ISimEntity
{
    ISimObject Owner { get; }
    
}