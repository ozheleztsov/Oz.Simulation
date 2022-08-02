using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oz.Snake.Client.Contracts;
using Oz.Snake.Client.Services;
using Oz.Snake.Client.Settings;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
    {
        config.Sources.Clear();
        config.AddJsonFile("appsettings.json");
        config.AddCommandLine(args);
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<SnakeSettings>(context.Configuration.GetSection(SnakeSettings.SnakeSettingsName));
        services.AddSingleton<ICommunicationService, CommunicationService>();
        services.AddSingleton<IKeyboardService, KeyboardService>();
        services.AddSingleton<ISnakeBoardService, SnakeBoardService>();
        services.AddSingleton<IOutputService, ConsoleOutputService>();

    })
    .Build();
    
    await RunAsync(host.Services);


static async Task RunAsync(IServiceProvider serviceProvider)
{
    CancellationTokenSource cancellationTokenSource = new();
    await using var scope = serviceProvider.CreateAsyncScope();
    var snakeService = scope.ServiceProvider.GetRequiredService<ICommunicationService>();
    var keyboardService = scope.ServiceProvider.GetRequiredService<IKeyboardService>();
    var outputService = scope.ServiceProvider.GetRequiredService<IOutputService>();
    
    await snakeService.JoinGame(cancellationTokenSource.Token);
    await keyboardService.StartListening(() =>
    {
        outputService.DrawMessage("Closing");
        cancellationTokenSource.Cancel();
    }, cancellationTokenSource.Token);
    outputService.DrawMessage("Done.");
}