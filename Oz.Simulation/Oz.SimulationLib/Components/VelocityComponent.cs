using Microsoft.Extensions.Logging;
using Oz.SimulationLib.Contracts;
using Oz.SimulationLib.Core;
using Oz.SimulationLib.Default;

namespace Oz.SimulationLib.Components;

public class VelocityComponent : SimComponent
{
    public Vector3 Velocity { get; set; }
    
    public VelocityComponent(ISimContext context, ISimObject owner, ILogger logger, Guid? id = null, string? name = null) : base(context, owner, logger, id, name)
    {
    }
}