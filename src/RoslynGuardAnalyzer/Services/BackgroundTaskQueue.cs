#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RoslynGuardAnalyzer.Services;

/// <summary>
/// Queue for background tasks to be processed asynchronously.
/// Supports priority levels and task scheduling.
/// </summary>
public sealed class BackgroundTaskQueue
{
    /// <summary>
    /// Represents a background task to be processed.
    /// </summary>
    public sealed class BackgroundTask
    {
        /// <summary>
        /// Unique identifier for the task.
        /// </summary>
        public required string Id { get; init; }
        /// <summary>
        /// The work to be performed as a function that takes a cancellation token and returns a task.
        /// </summary>
        public required Func<CancellationToken, Task> Work { get; init; }
        /// <summary>
        /// The priority of the task. Higher values indicate higher priority.
        /// </summary>
        public int Priority { get; init; } = 0; // Higher = higher priority
        /// <summary>
        /// The date and time when the task was enqueued.
        /// </summary>
        public DateTime EnqueuedAt { get; init; } = DateTime.UtcNow;
    }

    private readonly ConcurrentQueue<BackgroundTask> _queue = [];
    private readonly SemaphoreSlim _semaphore = new(0);
    private volatile bool _isRunning;

    /// <summary>
    /// Enqueues a background task.
    /// </summary>
    /// <param name="work">The work to be performed as a function that takes a cancellation token and returns a task.</param>
    /// <param name="priority">The priority of the task. Higher values indicate higher priority. Default is 0.</param>
    /// <returns>The unique identifier of the enqueued task.</returns>
    /// <exception cref="ArgumentNullException">Thrown when work is null.</exception>
    public string EnqueueTask(Func<CancellationToken, Task> work, int priority = 0)
    {
        if (work is null)
            throw new ArgumentNullException(nameof(work));

        var task = new BackgroundTask
        {
            Id = Guid.NewGuid().ToString(),
            Work = work,
            Priority = priority
        };

        _queue.Enqueue(task);
        _semaphore.Release();

        return task.Id;
    }

    /// <summary>
    /// Dequeues the next background task to process.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The dequeued background task, or null if no tasks are available.</returns>
    public async Task<BackgroundTask?> DequeueAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        // Get all tasks, sort by priority, re-queue all but the first
        var tasks = new List<BackgroundTask>();
        while (_queue.TryDequeue(out var task))
        {
            tasks.Add(task);
        }

        if (tasks.Count == 0)
            return null;

        // Sort by priority (descending) then by enqueue time (ascending)
        var sorted = tasks
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.EnqueuedAt)
            .ToList();

        var toReturn = sorted[0];

        // Re-queue the rest
        for (int i = 1; i < sorted.Count; i++)
        {
            _queue.Enqueue(sorted[i]);
            _semaphore.Release();
        }

        return toReturn;
    }

    /// <summary>
    /// Gets the number of tasks currently in the queue.
    /// </summary>
    public int Count => _queue.Count;

    /// <summary>
    /// Clears all pending tasks.
    /// </summary>
    public void Clear()
    {
        while (_queue.TryDequeue(out _))
        {
        }
    }

    /// <summary>
    /// Starts processing background tasks.
    /// </summary>
    public void Start()
    {
        _isRunning = true;
    }

    /// <summary>
    /// Stops processing background tasks.
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
    }

    /// <summary>
    /// Checks if the queue is currently running.
    /// </summary>
    public bool IsRunning => _isRunning;
}

/// <summary>
/// Hosted background task processor service.
/// </summary>
public sealed class BackgroundTaskProcessor : IDisposable
{
    private readonly BackgroundTaskQueue _queue;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private Task? _processingTask;

    /// <summary>
    /// Initializes a new instance of the BackgroundTaskProcessor class.
    /// </summary>
    /// <param name="queue">The background task queue to process tasks from.</param>
    /// <exception cref="ArgumentNullException">Thrown when queue is null.</exception>
    public BackgroundTaskProcessor(BackgroundTaskQueue queue)
    {
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _cancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Starts the background task processor.
    /// </summary>
    public void Start()
    {
        if (_processingTask is not null)
            return;

        _queue.Start();
        _processingTask = ProcessQueueAsync(_cancellationTokenSource.Token);
    }

    /// <summary>
    /// Stops the background task processor gracefully.
    /// </summary>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    public async Task StopAsync()
    {
        _queue.Stop();
        _cancellationTokenSource.Cancel();

        if (_processingTask is not null)
        {
            try
            {
                await _processingTask;
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }
    }

    /// <summary>
    /// Processes tasks from the queue continuously.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous processing operation.</returns>
    private async Task ProcessQueueAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var task = await _queue.DequeueAsync(cancellationToken);
                if (task is not null)
                {
                    try
                    {
                        await task.Work(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error processing background task {task.Id}: {ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error in background task processor: {ex.Message}");
                await Task.Delay(1000, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Releases the resources used by the BackgroundTaskProcessor.
    /// </summary>
    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
    }
}
