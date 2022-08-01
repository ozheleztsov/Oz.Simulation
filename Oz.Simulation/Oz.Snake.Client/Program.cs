using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oz.Snake.Client.Contracts;
using Oz.Snake.Client.Services;
using Oz.Snake.Client.Settings;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.Sources.Clear();
        config.AddJsonFile("appsettings.json");
        config.AddCommandLine(args);
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<SnakeSettings>(context.Configuration.GetSection(SnakeSettings.SnakeSettingsName));
        services.AddSingleton<ISnakeService, SnakeService>();
        services.AddSingleton<IKeyboardService, KeyboardService>();
        
    })
    .Build();
    
    await RunAsync(host.Services);


static async Task RunAsync(IServiceProvider serviceProvider)
{
    CancellationTokenSource cancellationTokenSource = new();
    await using var scope = serviceProvider.CreateAsyncScope();
    var snakeService = scope.ServiceProvider.GetRequiredService<ISnakeService>();
    var keyboardService = scope.ServiceProvider.GetRequiredService<IKeyboardService>();
    await snakeService.JoinGame(cancellationTokenSource.Token);
    await keyboardService.StartListening(() =>
    {
        Console.WriteLine("Closing");
        cancellationTokenSource.Cancel();
    }, cancellationTokenSource.Token);
    Console.WriteLine("Done.");
    
}