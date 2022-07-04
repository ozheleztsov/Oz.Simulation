using Microsoft.Extensions.Logging;
using Oz.Simulation.ClientLib.Contracts;
using Oz.SimulationLib.Components;
using Oz.SimulationLib.Contracts;
using Oz.SimulationLib.Core;
using Oz.SimulationLib.Default;
using Oz.SimulationLib.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Oz.Simulation.ClientLib.Services;

public class SimulationService : ISimulationService
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ISimulator _simulator;
    private RungeKuttaIntegrationComponent? _integrationComponent;

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
        firstTransform.Position = new Vector3(-1,0,0);
        var firstVelocity = await firstStar.GetVelocityAsync().ConfigureAwait(false);
        firstVelocity.Velocity = new Vector3(0, 0, 1.1* Math.PI);

        var secondStar = new SimObject3D(_simulator.Context ?? throw new SimulationException("Context is not set"), Guid.NewGuid(), "Second", _loggerFactory);
        var secondMass = await secondStar.GetMassAsync().ConfigureAwait(false);
        secondMass.SetMass(1);
        var secondTransform = await secondStar.GetTransformAsync().ConfigureAwait(false);
        secondTransform.Position = new Vector3(1, 0, 0);
        var secondVelocity = await secondStar.GetVelocityAsync().ConfigureAwait(false);
        secondVelocity.Velocity = new Vector3(0, 0,  -1.1 * Math.PI  );


        var thirdStar = new SimObject3D(_simulator.Context ?? throw new SimulationException("Context is not set"),
            Guid.NewGuid(), "Third", _loggerFactory);
        var thirdMass = await thirdStar.GetMassAsync().ConfigureAwait(false);
        thirdMass.SetMass(0.01);
        var thirdTransform = await thirdStar.GetTransformAsync().ConfigureAwait(false);
        thirdTransform.Position = new Vector3(0, 0, -5);
        var thirdVelocity = await thirdStar.GetVelocityAsync().ConfigureAwait(false);
        thirdVelocity.Velocity = new Vector3(-Math.PI * 1.3, 0, 0);
        
        var simulationManager = new SimObject(_simulator.Context, Guid.NewGuid(), "Simulation Manager", _loggerFactory);
        var integrator = await simulationManager.AddComponentAsync<RungeKuttaIntegrationComponent>().ConfigureAwait(false);

        await simulationLevel.AddObjectAsync(firstStar).ConfigureAwait(false);
        await simulationLevel.AddObjectAsync(secondStar).ConfigureAwait(false);
        await simulationLevel.AddObjectAsync(thirdStar).ConfigureAwait(false);
        await simulationLevel.AddObjectAsync(simulationManager).ConfigureAwait(false);

        await _simulator.StartSimulationAsync().ConfigureAwait(false);
        await integrator.PrepareAsync().ConfigureAwait(false);
        _integrationComponent = integrator;
    }

    public async Task<Dictionary<Guid, Vector3>> GetPlanetPositionsAsync()
    {
        var world = _simulator.World;
        if (world is null)
        {
            return new Dictionary<Guid, Vector3>();
        }

        var level = world.ActiveLevel;
        if (level is null)
        {
            return new Dictionary<Guid, Vector3>();
        }

        var integrators = await level.FindComponentsAsync<RungeKuttaIntegrationComponent>();
        if (!integrators.Any())
        {
            return new Dictionary<Guid, Vector3>();
        }

        var integrator = integrators.Single();
        
        // if (_integrationComponent is null)
        // {
        //     return new Dictionary<Guid, Vector3>();
        // }

        Dictionary<Guid, Vector3> positions = new();
        foreach (var (key, pos) in integrator.OutputPositions)
        {
            positions[key] = pos;
        }

        return positions;
    }
}