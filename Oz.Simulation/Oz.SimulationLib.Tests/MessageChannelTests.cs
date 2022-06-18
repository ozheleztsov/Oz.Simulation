using FluentAssertions;
using Oz.SimulationLib.Contracts;
using Oz.SimulationLib.Default;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Oz.SimulationLib.Tests;

public class MessageChannelTests
{
    private readonly MessageChannel _messageChannel = new();

    [Fact]
    public async Task Should_Unregister_Correctly()
    {
        var counter = 0;

        void OnObserve() =>
            counter++;

        var testObserver1 = new TestObserver(OnObserve);
        var testObserver2 = new TestObserver(OnObserve);

        var reg1 = await _messageChannel.RegisterAsync(testObserver1);
        var reg2 = await _messageChannel.RegisterAsync(testObserver2);

        await _messageChannel.SendMessageAsync(new TestMessage());

        counter.Should().Be(2);

        await reg1.DisposeAsync();

        await _messageChannel.SendMessageAsync(new TestMessage());
        counter.Should().Be(3);

        await reg2.DisposeAsync();

        await _messageChannel.SendMessageAsync(new TestMessage());
        counter.Should().Be(3);

        var reg3 = await _messageChannel.RegisterAsync(new TestObserver(OnObserve));
        await _messageChannel.SendMessageAsync(new TestMessage());
        counter.Should().Be(4);
        await reg3.DisposeAsync();
    }

    private class TestObserver : IMessageObserver<TestMessage>
    {
        private readonly Action _messageReceivedAction;

        public TestObserver(Action messageReceivedAction) =>
            _messageReceivedAction = messageReceivedAction;

        public Task ReceiveAsync(TestMessage? message)
        {
            _messageReceivedAction();
            return Task.CompletedTask;
        }
    }

    private sealed record TestMessage;
}