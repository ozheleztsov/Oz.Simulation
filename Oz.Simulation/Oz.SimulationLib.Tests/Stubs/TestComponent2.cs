using Oz.SimulationLib.Contracts;
using System;

namespace Oz.SimulationLib.Tests.Stubs;

public class TestComponent2 : TestComponent
{
    public TestComponent2(ISimContext context, ISimObject owner, Guid? id = null, string? name = null) : base(context, owner, id, name)
    {
    }
}