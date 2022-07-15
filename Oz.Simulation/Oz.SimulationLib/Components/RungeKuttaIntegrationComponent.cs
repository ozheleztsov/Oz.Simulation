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
    private bool _isPrepared;
    private bool _markedToReloadObjects = false;

    public RungeKuttaIntegrationComponent(ISimContext context, ISimObject owner, ILogger logger, Guid? id = null,
        string? name = null) : base(context, owner, logger, id, name)
    {
    }

    public ConcurrentDictionary<Guid, Vector3> OutputPositions { get; } = new();
    public double DeltaTime { get; set; } = 1.0 / 365; //1 day

    public double DistanceSoftValue { get; set; } = 1E-5; //AU;

    public async Task PrepareAsync()
    {
        await ReloadSimObjectsAsync().ConfigureAwait(false);
        _isPrepared = true;
    }

    private async Task ReloadSimObjectsAsync()
    {
        OutputPositions.Clear();
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
    }

    public void SetMarkToReloadObjects() => _markedToReloadObjects = true;

    public override async Task OnUpdateAsync()
    {
        if (!_isPrepared)
        {
            return;
        }

        if (_markedToReloadObjects)
        {
            await ReloadSimObjectsAsync().ConfigureAwait(false);
            _markedToReloadObjects = false;
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

    private static void AdvanceIntegrationObjectsPositions(List<IntegrationObject> integrationObjects,
        IReadOnlyList<(Vector3 Velocity, Vector3 Position)> newVelocitiesPositions)
    {
        for (var i = 0; i < integrationObjects.Count; i++)
        {
            integrationObjects[i].Transform.Position = newVelocitiesPositions[i].Position;
            integrationObjects[i].Velocity.Velocity = newVelocitiesPositions[i].Velocity;
        }
    }

    private List<(Vector3 Velocity, Vector3 Position)> GetUpdatedPositionVelocities(
        IReadOnlyCollection<IntegrationObject> integrationObjects, double deltaTime)
    {
        var result = new List<(Vector3 Velocity, Vector3 Position)>(integrationObjects.Count);
        var nextPositionVelocitiesMasses = GetUpdatedPositionVelocitiesInternal(integrationObjects, deltaTime);
        result.AddRange(nextPositionVelocitiesMasses.Select(nextPositionVelocityMass => (nextPositionVelocityMass.Velocity, nextPositionVelocityMass.Position)));
        return result;
    }

    private IEnumerable<PositionVelocityMass> GetUpdatedPositionVelocitiesInternal(IEnumerable<IntegrationObject> previousObjects,
        double deltaTime)
    {
        var sourcePositionVelocityMass = previousObjects.Select(io => new PositionVelocityMass(
            io.Transform.Position, io.Velocity.Velocity, io.Mass.Mass)).ToArray();

        var k1Values = GetKValues(sourcePositionVelocityMass, deltaTime);

        var k1AdvancedPvm = new PositionVelocityMass[sourcePositionVelocityMass.Length];
        for (var i = 0; i < sourcePositionVelocityMass.Length; i++)
        {
            k1AdvancedPvm[i] = new PositionVelocityMass(
                sourcePositionVelocityMass[i].Position + (0.5 * k1Values[i].Position),
                sourcePositionVelocityMass[i].Velocity + (0.5 * k1Values[i].Velocity),
                sourcePositionVelocityMass[i].Mass);
        }

        var k2Values = GetKValues(k1AdvancedPvm, deltaTime);
        var k2AdvancedPvm = new PositionVelocityMass[sourcePositionVelocityMass.Length];
        for (var i = 0; i < sourcePositionVelocityMass.Length; i++)
        {
            k2AdvancedPvm[i] = new PositionVelocityMass(
                sourcePositionVelocityMass[i].Position + (0.5 * k2Values[i].Position),
                sourcePositionVelocityMass[i].Velocity + (0.5 * k2Values[i].Velocity),
                sourcePositionVelocityMass[i].Mass);
        }

        var k3Values = GetKValues(k2AdvancedPvm, deltaTime);
        var k3AdvancedPvm = new PositionVelocityMass[sourcePositionVelocityMass.Length];
        for (var i = 0; i < sourcePositionVelocityMass.Length; i++)
        {
            k3AdvancedPvm[i] = new PositionVelocityMass(
                sourcePositionVelocityMass[i].Position + k3Values[i].Position,
                sourcePositionVelocityMass[i].Velocity + k3Values[i].Velocity,
                sourcePositionVelocityMass[i].Mass);
        }

        var k4Values = GetKValues(k3AdvancedPvm, deltaTime);

        var result = new PositionVelocityMass[sourcePositionVelocityMass.Length];
        for (var i = 0; i < sourcePositionVelocityMass.Length; i++)
        {
            var newPosition = sourcePositionVelocityMass[i].Position + ((k1Values[i].Position +
                                                                         (2.0 * k2Values[i].Position) +
                                                                         (2.0 * k3Values[i].Position) +
                                                                         k4Values[i].Position) * (1.0 / 6.0));
            var newVelocity = sourcePositionVelocityMass[i].Velocity +
                              ((k1Values[i].Velocity +
                                (2.0 * k2Values[i].Velocity) +
                                (2.0 * k3Values[i].Velocity) +
                                k4Values[i].Velocity) * (1.0 / 6.0));
            var newMass = sourcePositionVelocityMass[i].Mass;
            result[i] = new PositionVelocityMass(newPosition, newVelocity, newMass);
        }

        return result;
    }

    private PositionVelocityMass[] GetKValues(IReadOnlyList<PositionVelocityMass> sourceValues, double deltaTime)
    {
        var result = new PositionVelocityMass[sourceValues.Count];

        for (var i = 0; i < sourceValues.Count; i++)
        {
            var k1Velocity = deltaTime * GetAcceleration(sourceValues, i);
            var k1Position = deltaTime * GetVelocity(sourceValues, i);
            result[i] = new PositionVelocityMass(k1Position, k1Velocity, sourceValues[i].Mass);
        }

        return result;
    }


    private Vector3 GetAcceleration(IReadOnlyList<PositionVelocityMass> previousObjects, int targetObjectIndex)
    {
        var result = new Vector3();
        var rTarget = previousObjects[targetObjectIndex].Position;

        for (var i = 0; i < previousObjects.Count; i++)
        {
            if (i == targetObjectIndex)
            {
                continue;
            }

            var mj = previousObjects[i].Mass;
            var rj = previousObjects[i].Position;
            var diff = rTarget - rj;

            var distance = diff.Magnitude < DistanceSoftValue ? DistanceSoftValue : diff.Magnitude;
            var diffMag3 = Math.Pow(distance, 3);
            result += Constants.G * mj / diffMag3 * diff;
        }

        return -result;
    }

    private static Vector3 GetVelocity(IReadOnlyList<PositionVelocityMass> previousObjects, int targetObjectIndex) =>
        previousObjects[targetObjectIndex].Velocity;
}

public record IntegrationObject(SimObject3D SimObject, TransformComponent Transform, VelocityComponent Velocity,
    MassComponent Mass);

public record PositionVelocityMass(Vector3 Position, Vector3 Velocity, double Mass);