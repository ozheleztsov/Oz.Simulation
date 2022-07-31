using Oz.SimulationLib.Components;
using Oz.SimulationLib.Contracts;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Oz.Simulation.ClientLib.Extensions;

public static class SimObject3DExtensions
{
    public static async Task<PositionVelocityMass> ToPositionVelocityMassAsync(this ISimObject3D simObject3D)
    {
        var position = await simObject3D.GetTransformAsync().ConfigureAwait(false);
        var velocity = await simObject3D.GetVelocityAsync().ConfigureAwait(false);
        var mass = await simObject3D.GetMassAsync().ConfigureAwait(false);
        return new PositionVelocityMass(position.Position, velocity.Velocity, mass.Mass);
    }

    public static async Task<ImmutableList<PositionVelocityMass>> ToPositionVelocityMassAsync(
        this IEnumerable<ISimObject3D> simObject3Ds)
    {
        List<PositionVelocityMass> positionVelocityMasses = new();
        foreach (var obj in simObject3Ds)
        {
            positionVelocityMasses.Add(await obj.ToPositionVelocityMassAsync().ConfigureAwait(false));
        }

        return positionVelocityMasses.ToImmutableList();
    }
}