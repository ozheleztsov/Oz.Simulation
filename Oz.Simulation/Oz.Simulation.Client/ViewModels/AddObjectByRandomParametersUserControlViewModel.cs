using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Oz.Simulation.Client.Contracts.Services;
using Oz.Simulation.ClientLib.Contracts;
using Oz.Simulation.ClientLib.Models;
using Oz.SimulationLib.Core;
using System;

namespace Oz.Simulation.Client.ViewModels;

public class AddObjectByRandomParametersUserControlViewModel : ObservableObject
{
    private readonly ISimulationService _simulationService;
    private readonly IDialogService _dialogService;
    private int _countOfObjects;
    private DoubleRangeUserControlViewModel _massRange = new();
    private VectorRangeUserControlViewModel _positionRange = new();
    private VectorRangeUserControlViewModel _velocityRange = new();
    private AsyncRelayCommand? _generateCommand;

    public AddObjectByRandomParametersUserControlViewModel(ISimulationService simulationService,
        IDialogService dialogService)
    {
        _simulationService = simulationService;
        _dialogService = dialogService;
    }
    
    public AsyncRelayCommand GenerateCommand =>
        _generateCommand ??= new AsyncRelayCommand(async () =>
        {
            var count = CountOfObjects;
            if (count <= 0)
            {
                _dialogService.ShowErrorDialog($"Count of objects should be greater than zero. Provided value: {count}");
                return;
            }

            var massOperationResult = MassRange.GetMinMax();
            if (massOperationResult.HasErrors)
            {
                _dialogService.ShowErrorDialog($"Mass range error: {massOperationResult.GetErrorsString()}");
                return;
            }

            var positionOperationResult = PositionRange.GetMinMaxVector();
            if (positionOperationResult.HasErrors)
            {
                _dialogService.ShowErrorDialog($"Position range error: {positionOperationResult.GetErrorsString()}");
                return;
            }

            var velocityOperationResult = VelocityRange.GetMinMaxVector();
            if (velocityOperationResult.HasErrors)
            {
                _dialogService.ShowErrorDialog($"Velocity range error: {velocityOperationResult.GetErrorsString()}");
                return;
            }

            if (massOperationResult.Result is null)
            {
                _dialogService.ShowErrorDialog("Mass range is null");
                return;
            }

            if (positionOperationResult.Result is null)
            {
                _dialogService.ShowErrorDialog("Position range is null");
                return;
            }

            if (velocityOperationResult.Result is null)
            {
                _dialogService.ShowErrorDialog("Velocity range is null");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                var objectModel = GenerateObjectModel(massOperationResult.Result, positionOperationResult.Result,
                    velocityOperationResult.Result);
                await _simulationService.AddObjectToSimulationAsync(objectModel).ConfigureAwait(false);
            }
            
        });

    private ObjectModel GenerateObjectModel(MinMax<double> massRange, MinMaxVector positionRange,
        MinMaxVector velocityRange)
    {
        double mass = massRange.Min + Random.Shared.NextDouble() * (massRange.Max - massRange.Min);
        Vector3 position = RandomBetween(positionRange.MinVector, positionRange.MaxVector);
        Vector3 velocity = RandomBetween(velocityRange.MinVector, velocityRange.MaxVector);
        return new ObjectModel($"Planet_{mass:F2}", mass, (double[])position, (double[])velocity);
    }

    private Vector3 RandomBetween(Vector3 min, Vector3 max)
    {
        var x = RandomBetween(min.X, max.X);
        var y = RandomBetween(min.Y, max.Y);
        var z = RandomBetween(min.Z, max.Z);
        return new Vector3(x, y, z);
    }

    private double RandomBetween(double min, double max) =>
        min + Random.Shared.NextDouble() * (max - min);
    
    public int CountOfObjects
    {
        get => _countOfObjects;
        set => SetProperty(ref _countOfObjects, value);
    }

    public DoubleRangeUserControlViewModel MassRange
    {
        get => _massRange;
        set => SetProperty(ref _massRange, value);
    }

    public VectorRangeUserControlViewModel PositionRange
    {
        get => _positionRange;
        set => SetProperty(ref _positionRange, value);
    }

    public VectorRangeUserControlViewModel VelocityRange
    {
        get => _velocityRange;
        set => SetProperty(ref _velocityRange, value);
    }
}