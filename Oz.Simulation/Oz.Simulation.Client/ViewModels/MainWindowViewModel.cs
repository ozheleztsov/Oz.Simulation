using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Oz.Simulation.ClientLib.Contracts;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Oz.Simulation.Client.ViewModels;

public class MainWindowViewModel : ObservableRecipient
{
    private readonly ISimulationService _simulationService;
    private AsyncRelayCommand? _loadedCommand;
    private ICommand? _unloadedCommand;
    private AsyncRelayCommand? _restartSimulationCommand;

    public MainWindowViewModel(ISimulationService simulationService) =>
        _simulationService = simulationService;

    public AsyncRelayCommand LoadedCommand =>
        _loadedCommand ??= new AsyncRelayCommand(OnLoadedAsync);

    public ICommand UnloadedCommand =>
        _unloadedCommand ??= new RelayCommand(OnUnloaded);

    public IAsyncRelayCommand RestartSimulationCommand =>
        _restartSimulationCommand ??= new AsyncRelayCommand(async () =>
        {
            await _simulationService.PrepareSimulationAsync().ConfigureAwait(false);
        });

    private async Task OnLoadedAsync()
    {
        IsActive = true;
        
    }

    private void OnUnloaded()
    {
        IsActive = false;
    }
}