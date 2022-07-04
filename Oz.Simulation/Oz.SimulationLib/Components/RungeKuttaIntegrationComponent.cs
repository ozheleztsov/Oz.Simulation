using Microsoft.Extensions.Logging;
using Oz.SimulationLib.Contracts;
using Oz.SimulationLib.Core;
using Oz.SimulationLib.Default;
using Oz.SimulationLib.Exceptions;
using System.Collections.Concurrent;

namespace Oz.SimulationLib.Components;

public class RungeKuttaIntegrationComponent : SimComponent
{
    private readonly List<IntegrationObject> _integrationObjects = new();
    public ConcurrentDictionary<Guid, Vector3> OutputPositions { get; } = new();
    
    private bool _isPrepared;

    public double DeltaTime { get; set; } = 1.0 / 365;
    

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
        IntegrateStep(_integrationObjects, DeltaTime);
        await Task.CompletedTask;
    }

    private void IntegrateStep(List<IntegrationObject> integrationObjects, double deltaTime)
    {
        var updatedVelocitiesPositions = GetUpdatedPositionVelocities(integrationObjects, deltaTime);
        AdvanceIntegrationObjectsPositions(integrationObjects, updatedVelocitiesPositions);
        foreach (var integrationObj in _integrationObjects)
        {
            OutputPositions[integrationObj.SimObject.Id] = integrationObj.Transform.Position;
        }
    }

    private void AdvanceIntegrationObjectsPositions(List<IntegrationObject> integrationObjects, List<(Vector3 Velocity, Vector3 Position)> newVelocitiesPositions)
    {
        for (var i = 0; i < integrationObjects.Count; i++)
        {
            integrationObjects[i].Transform.Position = newVelocitiesPositions[i].Position;
            integrationObjects[i].Velocity.Velocity = newVelocitiesPositions[i].Velocity;
        }
    }

    private List<(Vector3 Velocity, Vector3 Position)> GetUpdatedPositionVelocities(List<IntegrationObject> integrationObjects, double deltaTime)
    {
        var result = new List<(Vector3 Velocity, Vector3 Position)>(integrationObjects.Count());
        for (var i = 0; i < integrationObjects.Count(); i++)
        {
            result.Add(GetUpdatedPositionVelocities(integrationObjects, i, deltaTime));
        }

        return result;
    }

    private (Vector3 Velocity, Vector3 Position) GetUpdatedPositionVelocities(List<IntegrationObject> previousObjects, int targetObjectIndex, double deltaTime)
    {
        var targetObject = previousObjects[targetObjectIndex];
        var velocity = targetObject.Velocity.Velocity + (deltaTime * GetAcceleration(previousObjects, targetObjectIndex));
        var position = targetObject.Transform.Position + (deltaTime * targetObject.Velocity.Velocity);
        return (velocity, position);
    }

    private Vector3 GetAcceleration(IReadOnlyList<IntegrationObject> previousObjects, int targetObjectIndex)
    {
        var result = new Vector3();
        var rTarget = previousObjects[targetObjectIndex].Transform.Position;

        for (var i = 0; i < previousObjects.Count; i++)
        {
            if (i == targetObjectIndex)
            {
                continue;
            }

            var mj = _integrationObjects[i].Mass.Mass;
            var rj = _integrationObjects[i].Transform.Position;
            var diff = rTarget - rj;
            var diffMag3 = Math.Pow(diff.Magnitude, 3);
            result += Constants.G * mj / diffMag3 * diff;
        }

        return -result;
    }
}

public record IntegrationObject(SimObject3D SimObject, TransformComponent Transform, VelocityComponent Velocity,
    MassComponent Mass);