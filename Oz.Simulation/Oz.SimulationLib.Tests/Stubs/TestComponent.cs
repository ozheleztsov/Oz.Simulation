using Microsoft.Extensions.Logging;
using Oz.SimulationLib.Contracts;
using Oz.SimulationLib.Default;
using System;
using System.Threading.Tasks;

namespace Oz.SimulationLib.Tests.Stubs;

public class TestComponent : SimComponent
{
    public TestComponent(ISimContext context, ISimObject owner, ILogger logger, Guid? id = null, string? name = null) : base(context, owner, logger, id, name)
    {
    }

    public int InitializeCalled { get; private set; }

    public int UpdateCalled { get; private set; }

    public int DestroyCalled { get; private set; }

    public override Task OnInitializeAsync()
    {
        InitializeCalled++;
        return base.OnInitializeAsync();
    }

    public override Task OnUpdateAsync()
    {
        UpdateCalled++;
        return base.OnUpdateAsync();
    }

    public override Task OnDestroyAsync()
    {
        DestroyCalled++;
        return base.OnDestroyAsync();
    }
}