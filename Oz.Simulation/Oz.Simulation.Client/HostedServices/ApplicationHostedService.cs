using Microsoft.Extensions.Hosting;
using Oz.Simulation.Client.Contracts.Services;
using Oz.Simulation.Client.Contracts.Windows;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Oz.Simulation.Client.HostedServices;

public class ApplicationHostedService : IHostedService
{
    private readonly IAsyncService _asyncService;

    public ApplicationHostedService(IAsyncService asyncService) =>
        _asyncService = asyncService;

    public async Task StartAsync(CancellationToken cancellationToken) =>
        await _asyncService.ExecuteOnUiThreadAsync(() =>
        {
            var mainWindow = ((App)Application.Current).GetService<IMainWindow>();
            mainWindow?.Show();
        }).ConfigureAwait(false);

    public Task StopAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;
}