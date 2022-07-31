using Oz.SimulationLib.Core;

namespace Oz.Simulation.ClientLib.Messages;

public sealed record StatsMessage(
    double InitialTotalEnergy, Vector3 InitialMomentum,
    double TotalEnergy, Vector3 Momentum);