using Oz.Simulation.ClientLib.Models;
using System.Threading.Tasks;

namespace Oz.Simulation.ClientLib.Contracts;

public interface IObjectModelReader
{
    Task<ObjectModelCollection> LoadAsync();
}