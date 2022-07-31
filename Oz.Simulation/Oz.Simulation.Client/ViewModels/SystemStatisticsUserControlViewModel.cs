using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Oz.Simulation.Client.Contracts.Services;
using Oz.Simulation.ClientLib.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Oz.Simulation.Client.ViewModels;

public class SystemStatisticsUserControlViewModel : ObservableRecipient
{
    private readonly IAsyncService _asyncService;
    private readonly ISimulationService _simulationService;
    private readonly ILogger<SystemStatisticsUserControlViewModel> _logger;

    private string _initialTotalEnergy = string.Empty;
    private string _currentTotalEnergy = string.Empty;
    private string _initialMomentum = string.Empty;
    private string _currentMomentum = string.Empty;
    
    public string InitialTotalEnergy
    {
        get => _initialTotalEnergy;
        set => SetProperty(ref _initialTotalEnergy, value);
    }

    public string CurrentTotalEnergy
    {
        get => _currentTotalEnergy;
        set => SetProperty(ref _currentTotalEnergy, value);
    }

    public string InitialMomentum
    {
        get => _initialMomentum;
        set => SetProperty(ref _initialMomentum, value);
    }

    public string CurrentMomentum
    {
        get => _currentMomentum;
        set => SetProperty(ref _currentMomentum, value);
    }

    public SystemStatisticsUserControlViewModel(IAsyncService asyncService, 
        ISimulationService simulationService, 
        ILoggerFactory loggerFactory)
    {
        _asyncService = asyncService;
        _simulationService = simulationService;
        _logger = loggerFactory.CreateLogger<SystemStatisticsUserControlViewModel>();
    }
    
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _updateTask;

    protected override void OnActivated()
    {
        base.OnActivated();
        _cancellationTokenSource = new CancellationTokenSource();
        _updateTask = Task.Run(async () => await  OnUpdateStatsAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
    }

    protected override void OnDeactivated()
    {
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = null;
            _updateTask = null;
        }
        base.OnDeactivated();
    }

    private async Task OnUpdateStatsAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                var stats = _simulationService.Stats;
                if (stats != null)
                {
                    await _asyncService.ExecuteOnUiThreadAsync(() =>
                    {
                        InitialTotalEnergy = $"{stats.InitialTotalEnergy:F2}";
                        InitialMomentum = $"{stats.InitialMomentum.X:F2}, {stats.InitialMomentum.Y:F2}, {stats.InitialMomentum.Z:F2}";
                        CurrentTotalEnergy = $"{stats.TotalEnergy:F2}";
                        CurrentMomentum = $"{stats.Momentum.X:F2}, {stats.Momentum.Y:F2}, {stats.Momentum.Z:F2}";
                    });
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Some exception");
        }
    }
}