using Microsoft.Extensions.Logging;
using Oz.SimulationLib.Contracts;
using System;

namespace Oz.SimulationLib.Tests.Stubs;

public class TestComponent2 : TestComponent
{
    public TestComponent2(ISimContext context, ISimObject owner, ILogger logger, Guid? id = null, string? name = null) : base(context, owner, logger, id, name)
    {
    }
}