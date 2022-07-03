using Oz.SimulationLib.Components;

namespace Oz.SimulationLib.Contracts;

public interface ISimObject3D : ISimObject
{
    Task<TransformComponent> GetTransformAsync();
    Task<VelocityComponent> GetVelocityAsync();
    Task<MassComponent> GetMassAsync();
}