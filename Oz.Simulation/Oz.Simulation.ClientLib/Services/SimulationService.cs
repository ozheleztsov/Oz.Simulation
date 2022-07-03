using Microsoft.Extensions.Logging;
using Oz.Simulation.ClientLib.Contracts;
using Oz.SimulationLib.Components;
using Oz.SimulationLib.Contracts;
using Oz.SimulationLib.Core;
using Oz.SimulationLib.Default;
using Oz.SimulationLib.Exceptions;
using System;
using System.Threading.Tasks;

namespace Oz.Simulation.ClientLib.Services;

public class SimulationService : ISimulationService
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ISimulator _simulator;

    public SimulationService(ISimulator simulator, ILoggerFactory loggerFactory)
    {
        _simulator = simulator;
        _loggerFactory = loggerFactory;
    }

    public async Task PrepareSimulationAsync()
    {
        await _simulator.PrepareSimulationAsync().ConfigureAwait(false);

        var simulationLevel = await (_simulator.World ?? throw new SimulationException("World is null")).AddLevelAsync("Star System").ConfigureAwait(false);

        var firstStar = new SimObject3D(_simulator.Context ?? throw new SimulationException("Context is not set"), Guid.NewGuid(), "First", _loggerFactory);
        var firstMass = await firstStar.GetMassAsync().ConfigureAwait(false);
        firstMass.SetMass(1);
        var firstTransform = await firstStar.GetTransformAsync().ConfigureAwait(false);
        firstTransform.Position = new Vector3();
        var firstVelocity = await firstStar.GetVelocityAsync().ConfigureAwait(false);
        firstVelocity.Velocity = new Vector3(0, 0, 2 * Math.PI / 365);

        var secondStar = new SimObject3D(_simulator.Context ?? throw new SimulationException("Context is not set"), Guid.NewGuid(), "Second", _loggerFactory);
        var secondMass = await secondStar.GetMassAsync().ConfigureAwait(false);
        secondMass.SetMass(1);
        var secondTransform = await secondStar.GetTransformAsync().ConfigureAwait(false);
        secondTransform.Position = new Vector3(1, 0, 0);
        var secondVelocity = await secondStar.GetVelocityAsync().ConfigureAwait(false);
        secondVelocity.Velocity = new Vector3(0, 0, -2 * Math.PI / 365);

        var simulationManager = new SimObject(_simulator.Context, Guid.NewGuid(), "Simulation Manager", _loggerFactory);
        var integrator = await simulationManager.AddComponentAsync<RungeKuttaIntegrationComponent>().ConfigureAwait(false);

        await simulationLevel.AddObjectAsync(firstStar).ConfigureAwait(false);
        await simulationLevel.AddObjectAsync(secondStar).ConfigureAwait(false);
        await simulationLevel.AddObjectAsync(simulationManager).ConfigureAwait(false);

        await _simulator.StartSimulationAsync().ConfigureAwait(false);
        await integrator.PrepareAsync().ConfigureAwait(false);
    }
}