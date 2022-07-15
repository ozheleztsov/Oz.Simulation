using Microsoft.Toolkit.Mvvm.ComponentModel;
using Oz.Simulation.Client.Models;
using Oz.Simulation.ClientLib.Models;
using Oz.SimulationLib.Core;

namespace Oz.Simulation.Client.ViewModels;

public class Vector3UserControlViewModel : ObservableObject
{
    private string _x = "0.0";
    private string _y = "0.0";
    private string _z = "0.0";
    
    public string X
    {
        get => _x;
        set => SetProperty(ref _x, value);
    }

    public string Y
    {
        get => _y;
        set => SetProperty(ref _y, value);
    }

    public string Z
    {
        get => _z;
        set => SetProperty(ref _z, value);
    }

    public OperationResult<Vector3> GetVector()
    {
        OperationResult<Vector3> operationResult = new();

        if (!double.TryParse(X, out var dX))
        {
            operationResult.AddError(nameof(X), $"Can't parse to double value: {X}");
            dX = 0.0;
        }

        if (!double.TryParse(Y, out var dY))
        {
            operationResult.AddError(nameof(Y), $"Can't parse to double value: {Y}");
            dY = 0.0;
        }

        if (!double.TryParse(Z, out var dZ))
        {
            operationResult.AddError(nameof(Z), $"Can't parse to double value: {Z}");
            dZ = 0.0;
        }

        operationResult.SetResult(new Vector3(dX, dY, dZ));
        return operationResult;
    }
}
