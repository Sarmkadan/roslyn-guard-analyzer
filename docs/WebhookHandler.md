# WebhookHandler

The `WebhookHandler` class provides functionality for registering, managing, and triggering webhooks within the Roslyn Guard Analyzer project. It enables event-driven notifications by allowing external services to subscribe to specific analyzer events via HTTP callbacks. The class handles the lifecycle of webhook registrations, including activation status, custom headers, and ISO 8601 timestamp formatting.

## API

### `public required string Id`
The unique identifier for the webhook handler. This value must be provided during initialization and is used to uniquely reference the webhook in the system.

### `public required string Url`
The destination URL where the webhook payload will be delivered when triggered. Must be a valid absolute URI.

### `public required string EventType`
The type of analyzer event this webhook subscribes to. Determines which events will invoke this webhook during `TriggerWebhooksAsync`.

### `public Dictionary<string, string> Headers`
A collection of custom HTTP headers to include in webhook requests. Headers are optional and can be modified after initialization.

### `public bool IsActive`
Indicates whether the webhook is currently active and eligible to receive events. Inactive webhooks are skipped during triggering.

### `public DateTime RegisteredAt`
The timestamp when the webhook was registered. Set automatically at creation and immutable thereafter.

### `public WebhookHandler(string id, string url, string eventType)`
Constructs a new `WebhookHandler` instance with the specified required properties.

- **Parameters**:
  - `id`: Unique identifier for the webhook.
  - `url`: Destination URL for webhook payloads.
  - `eventType`: Event type this webhook responds to.
- **Throws**: `ArgumentNullException` if `id`, `url`, or `eventType` is `null`.
- **Throws**: `ArgumentException` if `url` is not a valid absolute URI.

### `public string RegisterWebhook()`
Registers the webhook in the global registry, making it discoverable and eligible for triggering.

- **Returns**: A unique registration token that can be used to unregister the webhook later.
- **Throws**: `InvalidOperationException` if the webhook is already registered.

### `public bool UnregisterWebhook(string token)`
Removes the webhook from the global registry using the provided registration token.

- **Parameters**:
  - `token`: The registration token returned by `RegisterWebhook`.
- **Returns**: `true` if the webhook was successfully unregistered; `false` if the token was invalid or the webhook was not found.
- **Throws**: `ArgumentNullException` if `token` is `null`.

### `public bool DeactivateWebhook(string token)`
Temporarily disables the webhook using its registration token without removing it from the registry.

- **Parameters**:
  - `token`: The registration token returned by `RegisterWebhook`.
- **Returns**: `true` if the webhook was successfully deactivated; `false` if the token was invalid or the webhook was already inactive.
- **Throws**: `ArgumentNullException` if `token` is `null`.

### `public async Task TriggerWebhooksAsync(string eventType, object payload)`
Invokes all active webhooks subscribed to the specified event type with the given payload.

- **Parameters**:
  - `eventType`: The event type to match against webhook subscriptions.
  - `payload`: The payload object to serialize and send to matching webhooks.
- **Throws**: `ArgumentNullException` if `eventType` or `payload` is `null`.
- **Remarks**: Webhook requests are sent concurrently. Failed requests (non-2xx responses) are logged but do not interrupt processing.

### `public IReadOnlyList<WebhookRegistration> GetAllWebhooks()`
Retrieves all registered webhooks across all event types.

- **Returns**: An immutable list of all `WebhookRegistration` objects currently in the registry.

### `public IReadOnlyList<WebhookRegistration> GetWebhooksForEvent(string eventType)`
Retrieves all webhooks registered for a specific event type.

- **Parameters**:
  - `eventType`: The event type to filter by.
- **Returns**: An immutable list of `WebhookRegistration` objects matching the event type.
- **Throws**: `ArgumentNullException` if `eventType` is `null`.

### `public static string ToIso8601String(DateTime dateTime)`
Converts a `DateTime` to an ISO 8601 formatted string (e.g., `2024-05-20T14:30:00Z`).

- **Parameters**:
  - `dateTime`: The `DateTime` to format.
- **Returns**: A string representation of the `DateTime` in ISO 8601 format.
- **Throws**: `ArgumentOutOfRangeException` if `dateTime` is outside the supported range.

## Usage

### Registering and Triggering a Webhook
