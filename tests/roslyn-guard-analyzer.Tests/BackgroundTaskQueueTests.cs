using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using RoslynGuardAnalyzer.Services;

namespace RoslynGuardAnalyzer.Tests;

public class BackgroundTaskQueueTests
{
    [Fact]
    public void EnqueueTask_ShouldAddItem_AndReturnId()
    {
        // Arrange
        var queue = new BackgroundTaskQueue();

        // Act
        var id = queue.EnqueueTask(ct => Task.CompletedTask);

        // Assert
        Assert.NotNull(id);
        Assert.Equal(1, queue.Count);
    }

    [Fact]
    public void EnqueueTask_ShouldThrow_WhenWorkIsNull()
    {
        // Arrange
        var queue = new BackgroundTaskQueue();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => queue.EnqueueTask(null!));
    }

    [Fact]
    public async Task DequeueAsync_ShouldRespectPriority()
    {
        // Arrange
        var queue = new BackgroundTaskQueue();
        queue.EnqueueTask(ct => Task.CompletedTask, priority: 1);
        queue.EnqueueTask(ct => Task.CompletedTask, priority: 10);
        queue.EnqueueTask(ct => Task.CompletedTask, priority: 5);

        // Act
        var task1 = await queue.DequeueAsync(CancellationToken.None);
        var task2 = await queue.DequeueAsync(CancellationToken.None);
        var task3 = await queue.DequeueAsync(CancellationToken.None);

        // Assert
        Assert.Equal(10, task1!.Priority);
        Assert.Equal(5, task2!.Priority);
        Assert.Equal(1, task3!.Priority);
    }

    [Fact]
    public void Clear_ShouldRemoveAllTasks()
    {
        // Arrange
        var queue = new BackgroundTaskQueue();
        queue.EnqueueTask(ct => Task.CompletedTask);
        queue.EnqueueTask(ct => Task.CompletedTask);

        // Act
        queue.Clear();

        // Assert
        Assert.Equal(0, queue.Count);
    }

    [Fact]
    public async Task BackgroundTaskProcessor_ShouldProcessEnqueuedTask()
    {
        // Arrange
        var queue = new BackgroundTaskQueue();
        var taskRan = false;

        queue.EnqueueTask(ct =>
        {
            taskRan = true;
            return Task.CompletedTask;
        });

        // Act
        // Use synchronous 'using' because BackgroundTaskProcessor implements IDisposable, not IAsyncDisposable
        using var processor = new BackgroundTaskProcessor(queue);
        processor.Start();

        // Give the processor time to pick up the task
        await Task.Delay(200);

        await processor.StopAsync();

        // Assert
        Assert.True(taskRan);
    }
}
