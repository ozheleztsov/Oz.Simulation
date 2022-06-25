using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Oz.SimulationLib.Contracts;
using Oz.SimulationLib.Default;
using Oz.SimulationLib.Tests.Stubs;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Oz.SimulationLib.Tests;

public class SimObjectTests
{
    private readonly Mock<ISimContext> _simContextMock = new();
    private readonly SimObject _sut;

    public SimObjectTests() =>
        _sut = new SimObject(_simContextMock.Object, Guid.NewGuid(), "TestObject", new NullLoggerFactory());

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

    [Fact]
    public async Task Should_Add_Component_Correctly()
    {
        var component = await _sut.AddComponentAsync<TestComponent>("OneComp");

        component.Should().NotBeNull();
        component.Name.Should().Be("OneComp");

        //should not call component initialization before obj initializes
        component.InitializeCalled.Should().Be(0);

        await _sut.InitializeAsync();
        var component2 = await _sut.AddComponentAsync<TestComponent>("TwoComp");

        component2.Should().NotBeNull();
        component2.Name.Should().Be("TwoComp");
        component2.InitializeCalled.Should().Be(1);
        component.InitializeCalled.Should().Be(1);

        var componentWithoutName = await _sut.AddComponentAsync<TestComponent>();
        componentWithoutName.Should().NotBeNull();
        componentWithoutName.Name.Length.Should().BePositive();
    }

    [Fact]
    public async Task Should_Add_Component_Second_Approach()
    {
        var component1 = new TestComponent(_simContextMock.Object, _sut, new NullLogger<TestComponent>(), Guid.NewGuid(), "OneComp");
        await _sut.AddComponentAsync(component1);
        component1.InitializeCalled.Should().Be(0);
        await _sut.InitializeAsync();
        component1.InitializeCalled.Should().Be(1);

        var component2 = new TestComponent(_simContextMock.Object, null!, new NullLogger<TestComponent>(),Guid.NewGuid(), "TwoComp");
        await Assert.ThrowsAsync<ArgumentException>(async () => await _sut.AddComponentAsync(component2));

        var otherObj = new SimObject(_simContextMock.Object, Guid.NewGuid(), "OtherObj", new NullLoggerFactory());
        var component3 = new TestComponent(_simContextMock.Object, otherObj,new NullLogger<TestComponent>(), Guid.NewGuid(), "ThreeObj");
        await Assert.ThrowsAsync<ArgumentException>(async () => await _sut.AddComponentAsync(component3));

        //should not add the same reference twice
        await Assert.ThrowsAsync<ArgumentException>(async () => await _sut.AddComponentAsync(component1));
    }

    [Fact]
    public async Task Should_Get_All_Components()
    {
        await _sut.InitializeAsync();
        for (var i = 0; i < 10; i++)
        {
            await _sut.AddComponentAsync<TestComponent>();
        }

        var components = _sut.GetComponents().ToImmutableArray();
        components.Count().Should().Be(10);
        components.All(c => ((TestComponent)c).InitializeCalled == 1).Should().BeTrue();
    }

    [Fact]
    public async Task Should_Get_Components_Of_Correct_Type()
    {
        await _sut.InitializeAsync();
        for (var i = 0; i < 5; i++)
        {
            await _sut.AddComponentAsync<TestComponent>();
            await _sut.AddComponentAsync<TestComponent2>();
        }

        var result = _sut.GetComponents<TestComponent>().ToImmutableArray();
        result.Count().Should().Be(5);
        result.All(c => c != null).Should().Be(true);
    }

    [Fact]
    public async Task Should_Get_Components_Correctly()
    {
        var nullComp = _sut.GetComponent<TestComponent>();
        nullComp.Should().BeNull();

        await _sut.AddComponentAsync<TestComponent2>();

        nullComp = _sut.GetComponent<TestComponent>();
        nullComp.Should().BeNull();

        var nonNullComp = _sut.GetComponent<TestComponent2>();
        nonNullComp.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_Get_Component_By_Id()
    {
        var comp = await _sut.AddComponentAsync<TestComponent>();

        _sut.GetComponent(Guid.NewGuid()).Should().BeNull();
        var comp2 = _sut.GetComponent(comp.Id);

        object.ReferenceEquals(comp, comp2).Should().BeTrue();
    }
}