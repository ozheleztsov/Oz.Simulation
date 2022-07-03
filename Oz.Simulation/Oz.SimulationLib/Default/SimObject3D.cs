using Microsoft.Extensions.Logging;
using Oz.SimulationLib.Components;
using Oz.SimulationLib.Contracts;

namespace Oz.SimulationLib.Default;

public class SimObject3D : SimObject, ISimObject3D
{
    private TransformComponent? _transformComponent;
    private VelocityComponent? _velocityComponent;
    private MassComponent? _massComponent;
    
    public SimObject3D(ISimContext context, Guid id, string? name, ILoggerFactory loggerFactory) 
        : base(context, id, name, loggerFactory)
    {
    }

    public async Task<TransformComponent> GetTransformAsync()
    {
        if (_transformComponent is not null)
        {
            return _transformComponent;
        }

        _transformComponent = GetComponent<TransformComponent>() ?? 
                              await AddComponentAsync<TransformComponent>($"{Name}_{nameof(TransformComponent)}")
                                  .ConfigureAwait(false);
        return _transformComponent;
    }

    public async Task<VelocityComponent> GetVelocityAsync()
    {
        if (_velocityComponent is not null)
        {
            return _velocityComponent;
        }

        _velocityComponent = GetComponent<VelocityComponent>() ??
                             await AddComponentAsync<VelocityComponent>($"{Name}_{nameof(VelocityComponent)}")
                                 .ConfigureAwait(false);
        return _velocityComponent;
    }

    public async Task<MassComponent> GetMassAsync()
    {
        if (_massComponent is not null)
        {
            return _massComponent;
        }

        _massComponent = GetComponent<MassComponent>() ??
                         await AddComponentAsync<MassComponent>($"{Name}_{nameof(MassComponent)}")
                             .ConfigureAwait(false);
        return _massComponent;
    }
}