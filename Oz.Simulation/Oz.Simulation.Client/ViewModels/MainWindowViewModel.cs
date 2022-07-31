using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Oz.Simulation.Client.Contracts.Services;
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
    private RelayCommand? _addObjectCommand;
    private AddObjectUserControlViewModel _addObjectViewModel;

    public MainWindowViewModel(ISimulationService simulationService, IDialogService dialogService, IAsyncService asyncService, ILoggerFactory loggerFactory)
    {
        _simulationService = simulationService;
        _addObjectViewModel = new AddObjectUserControlViewModel(simulationService, dialogService, asyncService, loggerFactory);
    }

    public AsyncRelayCommand LoadedCommand =>
        _loadedCommand ??= new AsyncRelayCommand(OnLoadedAsync);

    public ICommand UnloadedCommand =>
        _unloadedCommand ??= new RelayCommand(OnUnloaded);

    public RelayCommand AddObjectCommand =>
        _addObjectCommand ??= new RelayCommand(() =>
        {
            AddObjectViewModel.IsVisible = true;
        });

    public IAsyncRelayCommand RestartSimulationCommand =>
        _restartSimulationCommand ??= new AsyncRelayCommand(async () =>
        {
            await _simulationService.PrepareSimulationAsync().ConfigureAwait(false);
        });

    public AddObjectUserControlViewModel AddObjectViewModel
    {
        get => _addObjectViewModel;
        set => SetProperty(ref _addObjectViewModel, value);
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        _addObjectViewModel.IsActive = true;
    }

    protected override void OnDeactivated()
    {
        _addObjectViewModel.IsActive = false;
        base.OnDeactivated();
    }

    private async Task OnLoadedAsync()
    {
        IsActive = true;
        
    }

    private void OnUnloaded()
    {
        IsActive = false;
    }
}