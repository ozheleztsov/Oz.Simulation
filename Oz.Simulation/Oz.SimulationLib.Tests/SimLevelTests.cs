using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Oz.SimulationLib.Contracts;
using Oz.SimulationLib.Default;
using Oz.SimulationLib.Tests.Stubs;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Oz.SimulationLib.Tests;

public class SimLevelTests
{
    private readonly Mock<ISimContext> _simContextMock = new();
    private readonly SimLevel _sut;

    public SimLevelTests() =>
        _sut = new SimLevel(_simContextMock.Object, Guid.NewGuid(), new NullLoggerFactory(), "TestLevel");

    [Fact]
    public async Task Should_Initialize_Only_Once()
    {
        await _sut.InitializeAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await _sut.InitializeAsync());
    }

    [Fact]
    public async Task Should_Not_Initialize_When_Destroyed()
    {
        await _sut.DestroyAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await _sut.InitializeAsync());
    }

    [Fact]
    public async Task Should_Correctly_Add_Objects()
    {
        await _sut.InitializeAsync();
        for (var i = 0; i < 10; i++)
        {
            await _sut.AddObjectAsync("Obj");
        }

        var actual = await _sut.FindAsync(obj => true);
        actual.Count().Should().Be(10);
    }

    [Fact]
    public async Task Should_Find_Object_By_Name()
    {
        await _sut.AddObjectAsync("One");
        await _sut.AddObjectAsync("One");
        await _sut.AddObjectAsync("Two");
        await _sut.InitializeAsync();

        var actual = await _sut.FindAsync("One");
        actual.Count().Should().Be(2);
    }

    [Fact]
    public async Task Should_Remove_Object_Correctly()
    {
        await _sut.InitializeAsync();
        await _sut.AddObjectAsync("test");
        var obj = (await _sut.FindAsync("test")).Single();
        var result = _sut.RemoveObject(obj.Id);

        result.Should().NotBeNull();
        var cnt = (await _sut.FindAsync(o => true)).Count();
        cnt.Should().Be(0);
    }

    [Fact]
    public async Task Should_Find_Components()
    {
        await _sut.InitializeAsync();

        var obj1 = await _sut.AddObjectAsync("1");
        var obj2 = await _sut.AddObjectAsync("2");

        await obj1.AddComponentAsync<TestComponent>();
        await obj1.AddComponentAsync<TestComponent2>();
        await obj2.AddComponentAsync<TestComponent>();

        var comps = await _sut.FindComponentsAsync<TestComponent>();
        comps.Count().Should().Be(2);
        comps = await _sut.FindComponentsAsync<TestComponent>(c => c.Owner.Name == "2");
        comps.Count().Should().Be(1);
    }
}