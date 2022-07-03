using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Oz.SimulationLib.Contracts;
using Oz.SimulationLib.Default;

namespace Oz.SimulationLib.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterSimulator(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddSingleton<ITime, TimeManager>();
        serviceCollection.TryAddSingleton<IMessageChannel, MessageChannel>();
        serviceCollection.TryAddSingleton<ISimulator, Simulator>();
        return serviceCollection;
    }
}