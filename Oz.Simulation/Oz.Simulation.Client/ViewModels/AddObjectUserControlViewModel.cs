using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Oz.Simulation.Client.Contracts.Services;
using Oz.Simulation.ClientLib.Contracts;
using Oz.Simulation.ClientLib.Models;
using System;

namespace Oz.Simulation.Client.ViewModels;

public class AddObjectUserControlViewModel : ObservableObject
{
    private readonly ISimulationService _simulationService;
    private readonly IDialogService _dialogService;
    private string _mass = "1.0";
    private bool _isVisible = false;
    private Vector3UserControlViewModel _position = new();
    private Vector3UserControlViewModel _velocity = new();
    private RelayCommand? _cancelCommand;
    private AsyncRelayCommand? _applyCommand;
    private RelayCommand? _generateRandomCommand;
    private AddObjectByRandomParametersUserControlViewModel _randomParameters;

    public AddObjectUserControlViewModel(ISimulationService simulationService, IDialogService dialogService)
    {
        _simulationService = simulationService;
        _dialogService = dialogService;
        _randomParameters = new AddObjectByRandomParametersUserControlViewModel(simulationService, dialogService);
    }

    public RelayCommand CancelCommand =>
        _cancelCommand ??= new RelayCommand(() =>
        {
            IsVisible = false;
        });

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
                return;
            }
        });

    public RelayCommand GenerateRandomCommand =>
        _generateRandomCommand ??= new RelayCommand(() =>
        {

        });

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

    public double GetMass()
    {
        if (!double.TryParse(Mass, out var dMass))
        {
            dMass = 0.0;
        }

        return dMass;
    }

    public AddObjectByRandomParametersUserControlViewModel RandomParameters
    {
        get => _randomParameters;
        set => SetProperty(ref _randomParameters, value);
    }
}