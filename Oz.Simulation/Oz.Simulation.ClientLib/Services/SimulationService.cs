using Microsoft.Extensions.Logging;
using Oz.Simulation.ClientLib.Components;
using Oz.Simulation.ClientLib.Contracts;
using Oz.Simulation.ClientLib.Messages;
using Oz.Simulation.ClientLib.Models;
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
    private readonly IObjectModelReader _objectModelReader;

    private IAsyncDisposable? _statsSubscription;
    
    public SimulationService(ISimulator simulator, IObjectModelReader objectModelReader, ILoggerFactory loggerFactory)
    {
        _simulator = simulator;
        _objectModelReader = objectModelReader;
        _loggerFactory = loggerFactory;
    }

    public async Task InitializeAsync() =>
        // register for update of the system stats
        _statsSubscription = await _simulator.Context.MessageChannel.RegisterAsync(new StatsHandler(this))
            .ConfigureAwait(false);

    public async Task ShutdownAsync()
    {
        if (_statsSubscription != null)
        {
            // unregister system stats message
            await _statsSubscription.DisposeAsync().ConfigureAwait(false);
        }
    }

    private ISimWorld World => _simulator.World;

    private ISimContext Context => _simulator.Context;
    
    public async Task<OperationResult> AddObjectToSimulationAsync(ObjectModel objectModel)
    {
        OperationResult operationResult = new();
        
        if (_simulator.Context is {Level: null})
        {
            operationResult.AddError("Level", "Context Level is null");
        }

        RungeKuttaIntegrationComponent? integrationComponent = null;

        if (_simulator.Context is {Level: { }})
        {
            var integrators = await Context.Level!.FindComponentsAsync<RungeKuttaIntegrationComponent>();
            var rungeKuttaIntegrationComponents =
                integrators as RungeKuttaIntegrationComponent[] ?? integrators.ToArray();
            if (!rungeKuttaIntegrationComponents.Any())
            {
                operationResult.AddError(nameof(RungeKuttaIntegrationComponent), "No integrators in level");
            }
            else
            {
                integrationComponent = rungeKuttaIntegrationComponents.Single();
            }
        }

        if (operationResult.HasErrors)
        {
            return operationResult;
        }

        var simObject = await CreateSimObject(Context, objectModel).ConfigureAwait(false);
        await Context.Level!.AddObjectAsync(simObject).ConfigureAwait(false);
        integrationComponent!.SetMarkToReloadObjects();
        return operationResult;
    }

    public StatsMessage? Stats { get; private set; }

    public async Task PrepareSimulationAsync()
    {
        await _simulator.FinishSimulationAsync().ConfigureAwait(false);
        await _simulator.PrepareSimulationAsync().ConfigureAwait(false);
        
        if (_simulator.World is null)
        {
            throw new InvalidOperationException("World must not be null here");
        }
        if (_simulator.World.Levels.Any())
        {
            await _simulator.World.DestroyAllLevelsAsync();
        }

        var simulationLevel = await World.AddLevelAsync("Star System").ConfigureAwait(false);

        var objectModels = await _objectModelReader.LoadAsync().ConfigureAwait(false);

        foreach (var objectModel in objectModels.Objects)
        {
            var simObject = await CreateSimObject(Context, objectModel).ConfigureAwait(false);
            await simulationLevel.AddObjectAsync(simObject).ConfigureAwait(false);
        }
        
        var simulationManager = new SimObject(Context, Guid.NewGuid(), "Simulation Manager", _loggerFactory);
        
        var integrator = await simulationManager.AddComponentAsync<RungeKuttaIntegrationComponent>().ConfigureAwait(false);
        await simulationLevel.AddObjectAsync(simulationManager).ConfigureAwait(false);
        await _simulator.StartSimulationAsync().ConfigureAwait(false);
        await integrator.PrepareAsync().ConfigureAwait(false);

        var statsComponent = await simulationManager.AddComponentAsync<ObjectSystemStatisticsComponent>()
            .ConfigureAwait(false);
        
    }

    public async Task<Dictionary<Guid, Vector3>> GetPlanetPositionsAsync()
    {
        var world = _simulator.World;
        var level = world.ActiveLevel;
        if (level is null)
        {
            return new Dictionary<Guid, Vector3>();
        }

        var integrators = await level.FindComponentsAsync<RungeKuttaIntegrationComponent>();
        var rungeKuttaIntegrationComponents = integrators as RungeKuttaIntegrationComponent[] ?? integrators.ToArray();
        if (!rungeKuttaIntegrationComponents.Any())
        {
            return new Dictionary<Guid, Vector3>();
        }

        var integrator = rungeKuttaIntegrationComponents.Single();
        Dictionary<Guid, Vector3> positions = new();
        foreach (var (key, pos) in integrator.OutputPositions)
        {
            positions[key] = pos;
        }

        return positions;
    }
    
    private async Task<ISimObject> CreateSimObject(ISimContext simContext, ObjectModel objectModel)
    {
        var obj = new SimObject3D(simContext, Guid.NewGuid(), objectModel.Name, _loggerFactory);
        var massComponent = await obj.GetMassAsync().ConfigureAwait(false);
        var transformComponent = await obj.GetTransformAsync().ConfigureAwait(false);
        var velocityComponent = await obj.GetVelocityAsync().ConfigureAwait(false);
        massComponent.SetMass(objectModel.Mass);
        transformComponent.Position = new Vector3(objectModel.Position);
        velocityComponent.Velocity = new Vector3(objectModel.Velocity);
        return obj;
    }
    
    private class StatsHandler : IMessageObserver<StatsMessage>
    {
        private readonly SimulationService _simulationService;

        public StatsHandler(SimulationService simulationService) =>
            _simulationService = simulationService;

        public async Task ReceiveAsync(StatsMessage? message)
        {
            if (message != null)
            {
                _simulationService.Stats = message;
            }
            await Task.CompletedTask;
        }
    }
}