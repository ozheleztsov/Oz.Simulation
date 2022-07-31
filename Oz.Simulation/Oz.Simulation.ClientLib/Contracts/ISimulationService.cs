using Oz.Simulation.ClientLib.Messages;
using Oz.Simulation.ClientLib.Models;
using Oz.SimulationLib.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oz.Simulation.ClientLib.Contracts;

public interface ISimulationService : ILifetimeService
{
    Task PrepareSimulationAsync();

    Task<Dictionary<Guid, Vector3>> GetPlanetPositionsAsync();

    Task<OperationResult> AddObjectToSimulationAsync(ObjectModel objectModel);
    
    StatsMessage? Stats { get; }
}

public interface ILifetimeService
{
    Task InitializeAsync();
    Task ShutdownAsync();
}