using FluentAssertions;
using Moq;
using Oz.SimulationLib.Contracts;
using Oz.SimulationLib.Default;
using System;
using System.Linq;
using Xunit;

namespace Oz.SimulationLib.Tests;

public class SimObjectTests
{
    private readonly SimObject _sut;
    private readonly Mock<ISimContext> _simContextMock = new Mock<ISimContext>();

    public SimObjectTests()
    {
        _sut = new SimObject(_simContextMock.Object, Guid.NewGuid(), "TestObject");
    }

    [Fact]
    public void Should_Correct_Set_Properties()
    {
        _sut.Properties.Count().Should().Be(0);
        _sut.SetProperty("first", 1);
        _sut.SetProperty("second", "str");
        _sut.Properties.Count().Should().Be(2);
        _sut.GetProperty<string>("second").Should().Be("str");
        _sut.GetProperty<int>("first").Should().Be(1);
    }
}