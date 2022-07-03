using Microsoft.Extensions.Logging;
using Oz.SimulationLib.Contracts;
using Oz.SimulationLib.Default;
using Oz.SimulationLib.Exceptions;

namespace Oz.SimulationLib.Components;

public class RungeKuttaIntegrationComponent : SimComponent
{
    private readonly List<IntegrationObject> _integrationObjects = new();
    private bool _isPrepared;

    public RungeKuttaIntegrationComponent(ISimContext context, ISimObject owner, ILogger logger, Guid? id = null,
        string? name = null) : base(context, owner, logger, id, name)
    {
    }

    public async Task PrepareAsync()
    {
        _integrationObjects.Clear();
        var currentLevel = Context.Level ?? throw new SimulationException("Level is null");
        var simObjects3D = await currentLevel.FindAsync(obj => obj is SimObject3D).ConfigureAwait(false);
        foreach (var simObj in simObjects3D)
        {
            var simObj3d = (SimObject3D)simObj;
            var transform = await simObj3d.GetTransformAsync().ConfigureAwait(false);
            var velocity = await simObj3d.GetVelocityAsync().ConfigureAwait(false);
            var mass = await simObj3d.GetMassAsync().ConfigureAwait(false);
            _integrationObjects.Add(new IntegrationObject(simObj3d, transform, velocity, mass));
        }

        _isPrepared = true;
    }

    public override async Task OnUpdateAsync()
    {
        if (!_isPrepared)
        {
            return;
        }
        
    }
}

public record IntegrationObject(SimObject3D SimObject, TransformComponent Transform, VelocityComponent Velocity,
    MassComponent Mass);