using Microsoft.Extensions.Logging;
using Oz.SimulationLib.Contracts;
using Oz.SimulationLib.Default;

namespace Oz.SimulationLib.Console.Components;

public class FpsCounter : SimComponent
{
    public FpsCounter(ISimContext context, ISimObject owner, ILogger logger, Guid? id = null, string? name = null) : base(context, owner, logger, id, name)
    {
    }

    private int _counter;
    private double _intervalSeconds;
    
    public override Task OnUpdateAsync()
    {
        
        _intervalSeconds += Context.Time.DeltaTime;
        _counter++;
        if (_intervalSeconds >= 1.0)
        {
            _intervalSeconds -= 1.0;
            Logger.LogInformation("Component updated with fps: {Fps}", _counter);
            _counter = 0;
        }
        return base.OnUpdateAsync();
    }
}