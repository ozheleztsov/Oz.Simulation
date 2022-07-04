using Oz.SimulationLib.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oz.Simulation.ClientLib.Contracts;

public interface ISimulationService
{
    Task PrepareSimulationAsync();

    Task<Dictionary<Guid, Vector3>> GetPlanetPositionsAsync();
}