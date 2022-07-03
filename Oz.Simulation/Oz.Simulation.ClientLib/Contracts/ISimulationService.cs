using System.Threading.Tasks;

namespace Oz.Simulation.ClientLib.Contracts;

public interface ISimulationService
{
    Task PrepareSimulationAsync();
}