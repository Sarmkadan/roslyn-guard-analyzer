#nullable enable

using System;
using System.Threading.Tasks;
using FluentAssertions;
using RoslynGuardAnalyzer.Events;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

/// <summary>
/// Unit tests for <see cref="EventBus"/>.
/// </summary>
public class EventBusTests
{
    private readonly EventBus _eventBus;

    public EventBusTests()
    {
        _eventBus = new EventBus();
    }

#region Constructor and Initial State

    [Fact]
    public void Constructor_InitializesEmptySubscriptions()
    {
        // Arrange & Act
        var eventBus = new EventBus();

        // Assert
        eventBus.SubscriptionCount.Should().Be(0);
    }

#endregion

#region PublishAsync

    [Fact]
    public async Task PublishAsync_WithNullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        EventBus eventBus = new();

        // Act & Assert
        Func<Task> act = async () => await eventBus.PublishAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task PublishAsync_WithNoSubscribers_DoesNotThrow()
    {
        // Arrange
        var eventBus = new EventBus();
        var testEvent = new TestEvent();

        // Act
        Func<Task> act = async () => await eventBus.PublishAsync(testEvent);

        // Assert
        await act.Should().NotThrowAsync();
        eventBus.SubscriptionCount.Should().Be(0);
    }

    [Fact]
    public async Task PublishAsync_WithSingleSubscriber_InvokesHandler()
    {
        // Arrange
        var eventBus = new EventBus();
        var testEvent = new TestEvent();
        bool handlerCalled = false;

        Func<TestEvent, Task> handler = e =>
        {
            handlerCalled = true;
            return Task.CompletedTask;
        };

        eventBus.Subscribe(handler);

        // Act
        await eventBus.PublishAsync(testEvent);

        // Assert
        handlerCalled.Should().BeTrue();
    }

    [Fact]
    public async Task PublishAsync_WithMultipleSubscribers_InvokesAllHandlers()
    {
        // Arrange
        var eventBus = new EventBus();
        var testEvent = new TestEvent();
        int handler1CallCount = 0;
        int handler2CallCount = 0;

        Func<TestEvent, Task> handler1 = e =>
        {
            handler1CallCount++;
            return Task.CompletedTask;
        };

        Func<TestEvent, Task> handler2 = e =>
        {
            handler2CallCount++;
            return Task.CompletedTask;
        };

        eventBus.Subscribe(handler1);
        eventBus.Subscribe(handler2);

        // Act
        await eventBus.PublishAsync(testEvent);

        // Assert
        handler1CallCount.Should().Be(1);
        handler2CallCount.Should().Be(1);
    }

    [Fact]
    public async Task PublishAsync_WithDerivedEvent_InvokesBaseEventHandler()
    {
        // Arrange
        var eventBus = new EventBus();
        var derivedEvent = new DerivedTestEvent();
        bool baseHandlerCalled = false;

        Func<TestEvent, Task> baseHandler = e =>
        {
            baseHandlerCalled = true;
            return Task.CompletedTask;
        };

        eventBus.Subscribe(baseHandler);

        // Act
        await eventBus.PublishAsync(derivedEvent);

        // Assert
        baseHandlerCalled.Should().BeTrue();
    }

    [Fact]
    public async Task PublishAsync_WithMultipleHandlersForSameEventType_InvokesAll()
    {
        // Arrange
        var eventBus = new EventBus();
        var testEvent = new TestEvent();
        int callCount = 0;

        Func<TestEvent, Task> handler1 = e =>
        {
            callCount++;
            return Task.CompletedTask;
        };

        Func<TestEvent, Task> handler2 = e =>
        {
            callCount++;
            return Task.CompletedTask;
        };

        Func<TestEvent, Task> handler3 = e =>
        {
            callCount++;
            return Task.CompletedTask;
        };

        eventBus.Subscribe(handler1);
        eventBus.Subscribe(handler2);
        eventBus.Subscribe(handler3);

        // Act
        await eventBus.PublishAsync(testEvent);

        // Assert
        callCount.Should().Be(3);
    }

    [Fact]
    public async Task PublishAsync_WithAsyncHandler_ExecutesCorrectly()
    {
        // Arrange
        var eventBus = new EventBus();
        var testEvent = new TestEvent();
        bool handlerCompleted = false;

        Func<TestEvent, Task> handler = async e =>
        {
            await Task.Delay(10);
            handlerCompleted = true;
        };

        eventBus.Subscribe(handler);

        // Act
        await eventBus.PublishAsync(testEvent);

        // Assert
        handlerCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task PublishAsync_WithHandlerThrowingException_LogsErrorButContinues()
    {
        // Arrange
        var eventBus = new EventBus();
        var testEvent = new TestEvent();
        int handler1CallCount = 0;
        int handler2CallCount = 0;

        Func<TestEvent, Task> throwingHandler = e =>
        {
            handler1CallCount++;
            throw new InvalidOperationException("Test exception");
        };

        Func<TestEvent, Task> normalHandler = e =>
        {
            handler2CallCount++;
            return Task.CompletedTask;
        };

        eventBus.Subscribe(throwingHandler);
        eventBus.Subscribe(normalHandler);

        // Act - should not throw even though one handler throws
        Func<Task> act = async () => await eventBus.PublishAsync(testEvent);
        await act.Should().NotThrowAsync();

        // Assert - both handlers should have been called
        handler1CallCount.Should().Be(1);
        handler2CallCount.Should().Be(1);
    }

#endregion

#region Subscribe

    [Fact]
    public void Subscribe_WithNullHandler_ThrowsArgumentNullException()
    {
        // Arrange
        var eventBus = new EventBus();

        // Act & Assert
        Action act = () => eventBus.Subscribe<TestEvent>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Subscribe_WithValidHandler_AddsSubscription()
    {
        // Arrange
        var eventBus = new EventBus();
        Func<TestEvent, Task> handler = e => Task.CompletedTask;

        // Act
        eventBus.Subscribe(handler);

        // Assert
        eventBus.SubscriptionCount.Should().Be(1);
    }

    [Fact]
    public void Subscribe_WithMultipleHandlers_AddsAllSubscriptions()
    {
        // Arrange
        var eventBus = new EventBus();
        Func<TestEvent, Task> handler1 = e => Task.CompletedTask;
        Func<TestEvent, Task> handler2 = e => Task.CompletedTask;
        Func<TestEvent, Task> handler3 = e => Task.CompletedTask;

        // Act
        eventBus.Subscribe(handler1);
        eventBus.Subscribe(handler2);
        eventBus.Subscribe(handler3);

        // Assert
        eventBus.SubscriptionCount.Should().Be(3);
    }

    [Fact]
    public void Subscribe_WithDifferentEventTypes_AddsSeparateSubscriptions()
    {
        // Arrange
        var eventBus = new EventBus();
        Func<TestEvent, Task> testEventHandler = e => Task.CompletedTask;
        Func<AnotherTestEvent, Task> anotherEventHandler = e => Task.CompletedTask;

        // Act
        eventBus.Subscribe(testEventHandler);
        eventBus.Subscribe(anotherEventHandler);

        // Assert
        eventBus.SubscriptionCount.Should().Be(2);
    }

#endregion

#region Unsubscribe

    [Fact]
    public void Unsubscribe_WithNullHandler_ThrowsArgumentNullException()
    {
        // Arrange
        var eventBus = new EventBus();

        // Act & Assert
        Action act = () => eventBus.Unsubscribe<TestEvent>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Unsubscribe_WithNonExistentHandler_DoesNotThrow()
    {
        // Arrange
        var eventBus = new EventBus();
        Func<TestEvent, Task> handler = e => Task.CompletedTask;

        // Act
        Action act = () => eventBus.Unsubscribe(handler);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Unsubscribe_WithSubscribedHandler_RemovesSubscription()
    {
        // Arrange
        var eventBus = new EventBus();
        Func<TestEvent, Task> handler = e => Task.CompletedTask;
        eventBus.Subscribe(handler);
        eventBus.SubscriptionCount.Should().Be(1);

        // Act
        eventBus.Unsubscribe(handler);

        // Assert
        eventBus.SubscriptionCount.Should().Be(0);
    }

    [Fact]
    public void Unsubscribe_WithMultipleHandlers_RemovesOnlySpecifiedHandler()
    {
        // Arrange
        var eventBus = new EventBus();
        Func<TestEvent, Task> handler1 = e => Task.CompletedTask;
        Func<TestEvent, Task> handler2 = e => Task.CompletedTask;
        Func<TestEvent, Task> handler3 = e => Task.CompletedTask;

        eventBus.Subscribe(handler1);
        eventBus.Subscribe(handler2);
        eventBus.Subscribe(handler3);
        eventBus.SubscriptionCount.Should().Be(3);

        // Act
        eventBus.Unsubscribe(handler2);

        // Assert
        eventBus.SubscriptionCount.Should().Be(2);
    }

    [Fact]
    public void Unsubscribe_WithSameHandlerSubscribedMultipleTimes_RemovesAllInstances()
    {
        // Arrange
        var eventBus = new EventBus();
        Func<TestEvent, Task> handler = e => Task.CompletedTask;

        eventBus.Subscribe(handler);
        eventBus.Subscribe(handler);
        eventBus.Subscribe(handler);
        eventBus.SubscriptionCount.Should().Be(3);

        // Act
        eventBus.Unsubscribe(handler);

        // Assert
        eventBus.SubscriptionCount.Should().Be(0);
    }

#endregion

#region ClearSubscriptions

    [Fact]
    public void ClearSubscriptions_WithSubscriptions_ClearsAll()
    {
        // Arrange
        var eventBus = new EventBus();
        Func<TestEvent, Task> handler1 = e => Task.CompletedTask;
        Func<TestEvent, Task> handler2 = e => Task.CompletedTask;

        eventBus.Subscribe(handler1);
        eventBus.Subscribe(handler2);
        eventBus.SubscriptionCount.Should().Be(2);

        // Act
        eventBus.ClearSubscriptions();

        // Assert
        eventBus.SubscriptionCount.Should().Be(0);
    }

    [Fact]
    public void ClearSubscriptions_WithNoSubscriptions_DoesNotThrow()
    {
        // Arrange
        var eventBus = new EventBus();

        // Act
        Action act = () => eventBus.ClearSubscriptions();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ClearSubscriptions_AfterClearing_SubscriptionCountIsZero()
    {
        // Arrange
        var eventBus = new EventBus();
        Func<TestEvent, Task> handler = e => Task.CompletedTask;
        eventBus.Subscribe(handler);
        eventBus.SubscriptionCount.Should().Be(1);

        // Act
        eventBus.ClearSubscriptions();

        // Assert
        eventBus.SubscriptionCount.Should().Be(0);
    }

#endregion

#region Edge Cases and Additional Tests

    [Fact]
    public async Task PublishAsync_WithHandlerReturningTask_ExecutesCorrectly()
    {
        // Arrange
        var eventBus = new EventBus();
        var testEvent = new TestEvent();
        bool handlerCalled = false;

        Func<TestEvent, Task> handler = e =>
        {
            handlerCalled = true;
            return Task.FromResult(true);
        };

        eventBus.Subscribe(handler);

        // Act
        await eventBus.PublishAsync(testEvent);

        // Assert
        handlerCalled.Should().BeTrue();
    }

    [Fact]
    public async Task PublishAsync_WithMultipleDifferentEventTypes_DispatchesCorrectly()
    {
        // Arrange
        var eventBus = new EventBus();
        var testEvent = new TestEvent();
        var anotherEvent = new AnotherTestEvent();
        int testEventCallCount = 0;
        int anotherEventCallCount = 0;

        Func<TestEvent, Task> testEventHandler = e =>
        {
            testEventCallCount++;
            return Task.CompletedTask;
        };

        Func<AnotherTestEvent, Task> anotherEventHandler = e =>
        {
            anotherEventCallCount++;
            return Task.CompletedTask;
        };

        eventBus.Subscribe(testEventHandler);
        eventBus.Subscribe(anotherEventHandler);

        // Act
        await eventBus.PublishAsync(testEvent);
        await eventBus.PublishAsync(anotherEvent);

        // Assert
        testEventCallCount.Should().Be(1);
        anotherEventCallCount.Should().Be(1);
    }

    [Fact]
    public void Subscribe_WithSameHandlerMultipleTimes_AddsMultipleSubscriptions()
    {
        // Arrange
        var eventBus = new EventBus();
        Func<TestEvent, Task> handler = e => Task.CompletedTask;

        // Act
        eventBus.Subscribe(handler);
        eventBus.Subscribe(handler);
        eventBus.Subscribe(handler);

        // Assert
        eventBus.SubscriptionCount.Should().Be(3);
    }

    [Fact]
    public async Task PublishAsync_WithDeepInheritanceChain_InvokesBaseHandlers()
    {
        // Arrange
        var eventBus = new EventBus();
        var deepEvent = new DeepDerivedTestEvent();
        bool baseHandlerCalled = false;
        bool derivedHandlerCalled = false;

        Func<TestEvent, Task> baseHandler = e =>
        {
            baseHandlerCalled = true;
            return Task.CompletedTask;
        };

        Func<DerivedTestEvent, Task> derivedHandler = e =>
        {
            derivedHandlerCalled = true;
            return Task.CompletedTask;
        };

        eventBus.Subscribe(baseHandler);
        eventBus.Subscribe(derivedHandler);

        // Act
        await eventBus.PublishAsync(deepEvent);

        // Assert - both handlers should be called due to inheritance
        baseHandlerCalled.Should().BeTrue();
        derivedHandlerCalled.Should().BeTrue();
    }

    [Fact]
    public void SubscriptionCount_AfterMultipleOperations_ReturnsCorrectCount()
    {
        // Arrange
        var eventBus = new EventBus();

        // Act & Assert
        eventBus.SubscriptionCount.Should().Be(0);

        Func<TestEvent, Task> handler1 = e => Task.CompletedTask;
        eventBus.Subscribe(handler1);
        eventBus.SubscriptionCount.Should().Be(1);

        Func<AnotherTestEvent, Task> handler2 = e => Task.CompletedTask;
        eventBus.Subscribe(handler2);
        eventBus.SubscriptionCount.Should().Be(2);

        eventBus.Unsubscribe(handler1);
        eventBus.SubscriptionCount.Should().Be(1);

        eventBus.ClearSubscriptions();
        eventBus.SubscriptionCount.Should().Be(0);
    }

#endregion

#region Helper Classes

    private class TestEvent : Event
    {
        public override string EventType => "TestEvent";
    }

    private class AnotherTestEvent : Event
    {
        public override string EventType => "AnotherTestEvent";
    }

    private class DerivedTestEvent : TestEvent
    {
        public override string EventType => "DerivedTestEvent";
    }

    private class DeepDerivedTestEvent : DerivedTestEvent
    {
        public override string EventType => "DeepDerivedTestEvent";
    }

#endregion
}
