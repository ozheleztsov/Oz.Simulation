using System.Threading.Tasks;

namespace Oz.Simulation.ClientLib.Contracts;

public interface ISimulationViewportService
{
    Task RenderAsync();
}