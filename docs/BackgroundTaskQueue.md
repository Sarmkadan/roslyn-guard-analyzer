# BackgroundTaskQueue

A queue for managing background tasks with configurable priority and lifecycle control. It is designed to process tasks asynchronously while allowing explicit start/stop control and task prioritization.

## API

### `public required string Id`
A unique identifier for the queue instance. Used to distinguish between different queues in a system.

### `public required Func<CancellationToken, Task> Work`
The delegate that defines the work to be performed when a task is dequeued. Accepts a `CancellationToken` for cooperative cancellation.

### `public int Priority`
The priority level of the queue. Higher values indicate higher priority. Defaults to `0`.

### `public DateTime EnqueuedAt`
The timestamp when the queue was created.

### `public string EnqueueTask`
The identifier for the task currently being enqueued.

### `public async Task<BackgroundTask?> DequeueAsync()`
Removes and returns the highest-priority task from the queue, or `null` if the queue is empty or stopped.

- **Returns**: A `BackgroundTask` instance if available, otherwise `null`.
- **Throws**: `InvalidOperationException` if the queue is stopped.

### `public void Clear()`
Removes all tasks from the queue.

### `public void Start()`
Starts processing tasks in the queue. Tasks will be dequeued and executed according to their priority.

### `public void Stop()`
Stops processing tasks immediately. Any tasks still in the queue remain until `Start()` is called again.

### `public BackgroundTaskProcessor`
Gets the processor responsible for executing dequeued tasks.

### `public void Start()`
Starts the background task processor.

### `public async Task StopAsync()`
Stops the background task processor asynchronously, allowing ongoing tasks to complete.

- **Returns**: A `Task` representing the asynchronous stop operation.

### `public void Dispose()`
Releases all resources used by the queue and stops processing.

## Usage

### Example 1: Basic Usage with Priority
