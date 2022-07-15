using Microsoft.Toolkit.Mvvm.ComponentModel;
using Oz.Simulation.ClientLib.Models;

namespace Oz.Simulation.Client.ViewModels;

public class DoubleRangeUserControlViewModel : ObservableObject
{
    private string _minValue = string.Empty;
    private string _maxValue = string.Empty;

    public string MinValue
    {
        get => _minValue;
        set => SetProperty(ref _minValue, value);
    }

    public string MaxValue
    {
        get => _maxValue;
        set => SetProperty(ref _maxValue, value);
    }

    public OperationResult<MinMax<double>> GetMinMax()
    {
        OperationResult<MinMax<double>> operationResult = new();
        if (!double.TryParse(MinValue, out var min))
        {
            operationResult.Errors.Add( new FieldError(nameof(MinValue), "Couldn't convert to double"));
        }

        if (!double.TryParse(MaxValue, out var max))
        {
            operationResult.Errors.Add(new FieldError(nameof(MaxValue), "Couldn't convert to double"));
        }

        if (!operationResult.HasErrors)
        {
            operationResult.SetResult(new MinMax<double>(min, max));
        }

        return operationResult;
    }
}

public sealed record MinMax<T>(T Min, T Max);