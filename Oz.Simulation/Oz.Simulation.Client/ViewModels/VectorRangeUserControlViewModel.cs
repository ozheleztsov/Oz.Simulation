using Microsoft.Toolkit.Mvvm.ComponentModel;
using Oz.Simulation.ClientLib.Models;
using Oz.SimulationLib.Core;
using System.Collections.Generic;

namespace Oz.Simulation.Client.ViewModels;

public class VectorRangeUserControlViewModel : ObservableObject
{
    private Vector3UserControlViewModel _maxVector = new();

    private Vector3UserControlViewModel _minVector = new();

    public Vector3UserControlViewModel MinVector
    {
        get => _minVector;
        set => SetProperty(ref _minVector, value);
    }

    public Vector3UserControlViewModel MaxVector
    {
        get => _maxVector;
        set => SetProperty(ref _maxVector, value);
    }

    public OperationResult<MinMaxVector> GetMinMaxVector()
    {
        var minOperationResult = MinVector.GetVector();
        var maxOperationResult = MaxVector.GetVector();
        var operationResult =
            OperationResult.CombineOperationResults<Vector3, MinMaxVector>(
                new List<OperationResult<Vector3>> {minOperationResult, maxOperationResult}, "MinMaxVector");
        if (!operationResult.HasErrors)
        {
            operationResult.SetResult(new MinMaxVector(minOperationResult.Result, maxOperationResult.Result));
        }

        return operationResult;
    }
}

public sealed record MinMaxVector(Vector3 MinVector, Vector3 MaxVector);