#nullable enable

using System;
using System.Threading.Tasks;
using FluentAssertions;
using RoslynGuardAnalyzer.Events;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="EventBusExtensions"/>.
/// </summary>
public class EventBusExtensionsTests
{
    private sealed class SampleEvent : Event
    {
        public override string EventType => "SampleEvent";
    }

    private sealed class OtherEvent : Event
    {
        public override string EventType => "OtherEvent";
    }

#region PublishIfSubscribedAsync

    [Fact]
    public async Task PublishIfSubscribedAsync_WithMatchingHandler_InvokesHandler()
    {
        // Arrange
        IEventBus bus = new EventBus();
        var received = 0;
        bus.Subscribe<SampleEvent>(_ =>
        {
            received++;
            return Task.CompletedTask;
        });

        // Act
        await bus.PublishIfSubscribedAsync(new SampleEvent());

        // Assert
        received.Should().Be(1);
    }

    [Fact]
    public async Task PublishIfSubscribedAsync_WithoutMatchingHandler_DoesNotInvokeUnrelatedHandler()
    {
        // Arrange
        IEventBus bus = new EventBus();
        var otherReceived = 0;
        bus.Subscribe<OtherEvent>(_ =>
        {
            otherReceived++;
            return Task.CompletedTask;
        });

        // Act
        Func<Task> act = async () => await bus.PublishIfSubscribedAsync(new SampleEvent());

        // Assert
        await act.Should().NotThrowAsync();
        otherReceived.Should().Be(0);
    }

    [Fact]
    public async Task PublishIfSubscribedAsync_WithNoSubscribers_CompletesWithoutThrowing()
    {
        // Arrange
        IEventBus bus = new EventBus();

        // Act
        Func<Task> act = async () => await bus.PublishIfSubscribedAsync(new SampleEvent());

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishIfSubscribedAsync_AfterUnsubscribe_DoesNotInvokeHandler()
    {
        // Arrange
        IEventBus bus = new EventBus();
        var received = 0;
        Func<SampleEvent, Task> handler = _ =>
        {
            received++;
            return Task.CompletedTask;
        };
        bus.Subscribe(handler);
        bus.Unsubscribe(handler);

        // Act
        await bus.PublishIfSubscribedAsync(new SampleEvent());

        // Assert
        received.Should().Be(0);
    }

    [Fact]
    public async Task PublishIfSubscribedAsync_WithBaseTypeHandler_InvokesHandlerForDerivedEvent()
    {
        // Arrange
        IEventBus bus = new EventBus();
        var received = 0;
        bus.Subscribe<Event>(_ =>
        {
            received++;
            return Task.CompletedTask;
        });

        // Act
        await bus.PublishIfSubscribedAsync(new SampleEvent());

        // Assert
        received.Should().Be(1);
    }

    [Fact]
    public async Task PublishIfSubscribedAsync_WithNullBus_ThrowsArgumentNullException()
    {
        // Arrange
        IEventBus? bus = null;

        // Act
        Func<Task> act = async () => await bus!.PublishIfSubscribedAsync(new SampleEvent());

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task PublishIfSubscribedAsync_WithNullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        IEventBus bus = new EventBus();

        // Act
        Func<Task> act = async () => await bus.PublishIfSubscribedAsync<SampleEvent>(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

#endregion
}
