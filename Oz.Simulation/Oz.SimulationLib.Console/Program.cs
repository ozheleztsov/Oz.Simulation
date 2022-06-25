using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oz.SimulationLib.Console.Components;
using Oz.SimulationLib.Contracts;
using Oz.SimulationLib.Default;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
    {
        config.Sources.Clear();
        config.AddJsonFile("appsettings.json", false, false);
        config.AddEnvironmentVariables();
        config.AddCommandLine(args);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<ITime, TimeManager>();
        services.AddSingleton<IMessageChannel, MessageChannel>();
        services.AddSingleton<ISimulator, Simulator>();
    })
    .Build();
await RunAsync(host.Services);


static async Task RunAsync(IServiceProvider serviceProvider)
{
    var simulator = serviceProvider.GetRequiredService<ISimulator>();
    await simulator.PrepareSimulationAsync();
    await simulator.StartSimulationAsync();
    var level = await simulator.World!.AddLevelAsync("Sample level");
    ISimObject simObject = await level.AddObjectAsync("Sample Object");
    
    Console.WriteLine(simObject.ToString());
    simObject = (await simulator.World!.ActiveLevel!.FindAsync("Sample Object")).Single();
    await simObject.AddComponentAsync<FpsCounter>("FpsCounter");
    await AddComputeAsync(simulator.World.ActiveLevel);
    
    Console.ReadLine();

    await simulator.FinishSimulationAsync();
    Console.WriteLine("Done");
}

static async Task AddComputeAsync(ISimLevel simLevel)
{
    for (int i = 0; i < 1000; i++)
    {
        var simObject = await simLevel.AddObjectAsync($"TestObj_{i}");

        for (int j = 0; j < 10; j++)
        {
            await simObject.AddComponentAsync<Compute>($"TestObj_{i}_Compute_{j}");
        }
    }
}