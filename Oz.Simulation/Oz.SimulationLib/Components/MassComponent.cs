using Microsoft.Extensions.Logging;
using Oz.SimulationLib.Contracts;
using Oz.SimulationLib.Default;

namespace Oz.SimulationLib.Components;

public class MassComponent : SimComponent
{
    public double Mass { get; private set; }
    
    public MassComponent(ISimContext context, ISimObject owner, ILogger logger, Guid? id = null, string? name = null) : base(context, owner, logger, id, name)
    {
    }

    public void SetMass(double mass) =>
        Mass = mass;
}