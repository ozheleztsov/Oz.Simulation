using Microsoft.Extensions.Logging;
using Oz.SimulationLib.Contracts;
using Oz.SimulationLib.Core;
using Oz.SimulationLib.Default;

namespace Oz.SimulationLib.Components;

public class TransformComponent : SimComponent
{
    public Vector3 Position { get; set; } = new();

    public TransformComponent(ISimContext context, ISimObject owner, ILogger logger, Guid? id = null, string? name = null) : base(context, owner, logger, id, name)
    {
    }
}