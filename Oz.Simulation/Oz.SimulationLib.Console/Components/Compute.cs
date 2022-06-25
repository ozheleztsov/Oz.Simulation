using Microsoft.Extensions.Logging;
using Oz.SimulationLib.Contracts;
using Oz.SimulationLib.Default;

namespace Oz.SimulationLib.Console.Components;

public class Compute : SimComponent
{
    public Compute(ISimContext context, ISimObject owner, ILogger logger, Guid? id = null, string? name = null) : base(context, owner, logger, id, name)
    {
    }

    public override Task OnUpdateAsync()
    {
        var arr = new int[100];
        for (var i = 0; i < 100; i++)
        {
            arr[i] = 1;
        }

        for (var i = 1; i < arr.Length; i++)
        {
            arr[i] += arr[i - 1];
        }

        return base.OnUpdateAsync();
    }
}