using Oz.SimulationLib.Core;

namespace Oz.Simulation.ClientLib.Models;

public sealed record PositionVelocityMass(Vector3 Position, Vector3 Velocity, double Mass);