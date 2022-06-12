using System.Collections.Concurrent;

namespace Oz.SimulationLib.Contracts;

public interface ISimObject : ISimEntity
{
    ConcurrentDictionary<string, object> Properties { get; }
}