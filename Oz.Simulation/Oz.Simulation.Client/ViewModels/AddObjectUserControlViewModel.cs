using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Oz.Simulation.Client.Contracts.Services;
using Oz.Simulation.ClientLib.Contracts;
using Oz.Simulation.ClientLib.Models;
using System;

namespace Oz.Simulation.Client.ViewModels;

public class AddObjectUserControlViewModel : ObservableRecipient
{
    private readonly IDialogService _dialogService;
    private readonly ISimulationService _simulationService;
    private AsyncRelayCommand? _applyCommand;
    private RelayCommand? _cancelCommand;
    private RelayCommand? _generateRandomCommand;
    private bool _isVisible;
    private string _mass = "1.0";
    private Vector3UserControlViewModel _position = new();
    private AddObjectByRandomParametersUserControlViewModel _randomParameters;
    private Vector3UserControlViewModel _velocity = new();

    public AddObjectUserControlViewModel(ISimulationService simulationService, IDialogService dialogService,
        IAsyncService asyncService, ILoggerFactory loggerFactory)
    {
        _simulationService = simulationService;
        _dialogService = dialogService;
        _randomParameters = new AddObjectByRandomParametersUserControlViewModel(simulationService, dialogService);
        SystemStatistics = new SystemStatisticsUserControlViewModel(asyncService, simulationService, loggerFactory);
    }

    public SystemStatisticsUserControlViewModel SystemStatistics { get; }

    public RelayCommand CancelCommand =>
        _cancelCommand ??= new RelayCommand(() => { IsVisible = false; });

    public AsyncRelayCommand ApplyCommand =>
        _applyCommand ??= new AsyncRelayCommand(async () =>
        {
            var mass = GetMass();
            if (mass == 0.0)
            {
                _dialogService.ShowErrorDialog("Mass can't be zero");
                return;
            }

            var positionResult = Position.GetVector();
            if (positionResult.HasErrors)
            {
                _dialogService.ShowErrorDialog(positionResult.GetErrorsString());
                return;
            }

            var velocityResult = Velocity.GetVector();
            if (velocityResult.HasErrors)
            {
                _dialogService.ShowErrorDialog(velocityResult.GetErrorsString());
                return;
            }

            var objectModel = new ObjectModel(Guid.NewGuid().ToString(), mass, (double[])positionResult.Result,
                (double[])velocityResult.Result);
            var result = await _simulationService.AddObjectToSimulationAsync(objectModel).ConfigureAwait(false);
            if (result.HasErrors)
            {
                _dialogService.ShowErrorDialog(result.GetErrorsString());
            }
        });

    public RelayCommand GenerateRandomCommand =>
        _generateRandomCommand ??= new RelayCommand(() => { });

    public string Mass
    {
        get => _mass;
        set => SetProperty(ref _mass, value);
    }

    public Vector3UserControlViewModel Position
    {
        get => _position;
        set => SetProperty(ref _position, value);
    }

    public Vector3UserControlViewModel Velocity
    {
        get => _velocity;
        set => SetProperty(ref _velocity, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    public AddObjectByRandomParametersUserControlViewModel RandomParameters
    {
        get => _randomParameters;
        set => SetProperty(ref _randomParameters, value);
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        SystemStatistics.IsActive = true;
    }

    protected override void OnDeactivated()
    {
        SystemStatistics.IsActive = false;
        base.OnDeactivated();
    }

    public double GetMass()
    {
        if (!double.TryParse(Mass, out var dMass))
        {
            dMass = 0.0;
        }

        return dMass;
    }
}