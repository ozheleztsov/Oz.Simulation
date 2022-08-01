using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.Sources.Clear();
        config.AddJsonFile("appsettings.json");
        config.AddCommandLine(args);
    })
    .ConfigureServices((context, services) => { })
    .Build();
    
    await RunAsync(host.Services);


static async Task RunAsync(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateAsyncScope();
    var program = scope.ServiceProvider.GetService<Program>();
}