# Webhook integration

`WebhookHandler` registers HTTP endpoints and delivers caller-provided JSON to
the active endpoints for a named event. It is an opt-in integration component;
the analyzer does not automatically connect analysis events to webhooks.

## Request format

Each matching endpoint receives an HTTP `POST` request with:

- `Content-Type: application/json; charset=utf-8`
- `Accept: application/json`
- `User-Agent: RoslynGuardAnalyzer/1.0`
- `X-Event-Type`: the event type supplied when the endpoint was registered
- `X-Timestamp`: the UTC send time in `yyyy-MM-ddTHH:mm:ssZ` format
- Any custom request headers supplied during registration

The request body is exactly the `jsonPayload` string passed to
`TriggerWebhooksAsync`. `WebhookHandler` does not serialize an object, validate
the JSON, add an envelope, or impose a schema. Producers and consumers must
therefore agree on the payload shape. For example, an analysis-completed
integration might use:

```json
{
  "eventId": "9be72126-360a-47f7-a389-2e86d8ad7d30",
  "projectPath": "/workspace/MyProject.csproj",
  "success": true,
  "violationCount": 1,
  "violations": [
    {
      "ruleId": "RG001",
      "message": "Example violation",
      "filePath": "src/Example.cs",
      "line": 24
    }
  ]
}
```

This is an example contract, not a shape enforced by the handler. Use a stable
schema of your own and version it if consumers need compatibility guarantees.

## Integrate a producer

1. Create one long-lived handler for the application or pipeline run.
2. Register each destination URL with an event-type string and any authentication
   headers. Event matching is exact and case-sensitive.
3. Serialize the event data to JSON.
4. Call `TriggerWebhooksAsync` with the same event-type string and await it.
5. Retain the returned registration ID if the endpoint may need to be deactivated
   or removed later.

```csharp
using System.Text.Json;
using RoslynGuardAnalyzer.Integration;

var webhooks = new WebhookHandler();

string registrationId = webhooks.RegisterWebhook(
    "https://ci.example.com/hooks/roslyn-guard",
    "AnalysisCompleted",
    new Dictionary<string, string>
    {
        ["Authorization"] = "Bearer <token>"
    });

var payload = new
{
    eventId = Guid.NewGuid(),
    projectPath,
    success = true,
    violationCount = violations.Count,
    violations
};

await webhooks.TriggerWebhooksAsync(
    "AnalysisCompleted",
    JsonSerializer.Serialize(payload));
```

Do not add `Content-Type` as a custom header; the handler sets it on the request
content. Avoid custom `X-Event-Type`, `X-Timestamp`, `Accept`, and `User-Agent`
headers because the handler also supplies them.

## Implement the receiver

The receiving service should expose an HTTPS endpoint that accepts `POST`
requests, authenticates any configured credentials, and parses the agreed JSON
schema. Use `X-Event-Type` to route event types when several events share an
endpoint. Return a 2xx status after the event has been accepted; make processing
idempotent by including and recording a producer-generated identifier such as
`eventId`, because transient failures can cause retries.

Do not use `X-Timestamp` as an event occurrence time. It is generated immediately
before delivery and represents the send attempt, so include the domain event's
own timestamp in the JSON when that distinction matters.

## Registration lifecycle

Registrations are held in memory and are not restored after process restart.
Register them again during application startup.

```csharp
IReadOnlyList<WebhookHandler.WebhookRegistration> all =
    webhooks.GetAllWebhooks();

IReadOnlyList<WebhookHandler.WebhookRegistration> activeForEvent =
    webhooks.GetWebhooksForEvent("AnalysisCompleted");

webhooks.DeactivateWebhook(registrationId); // retained but no longer receives events
webhooks.UnregisterWebhook(registrationId); // removed completely
```

`WebhookCount` includes active and inactive registrations. There is no reactivate
operation; unregister and register a new endpoint when activation is needed
again.

## Delivery behavior

- All active registrations whose event type exactly matches are invoked
  concurrently, and `TriggerWebhooksAsync` waits for every delivery attempt.
- Each request has a 10-second HTTP client timeout.
- The underlying `HttpClientFactory` applies its configured retry and circuit
  breaker behavior. With the default options, transient exceptions and HTTP 408,
  429, and 5xx responses can be retried.
- A non-success response, timeout, or delivery exception is written to standard
  error and is not returned to the caller. `TriggerWebhooksAsync` therefore does
  not provide per-endpoint delivery results.
- No signature, secret rotation, delivery queue, persistence, or replay facility
  is provided. Supply authentication headers over HTTPS and add those facilities
  in the surrounding application if required.

To customize retry behavior, construct an `HttpClientFactory` with
`HttpClientFactoryOptions` and pass it to `WebhookHandler`. If the application
owns that factory, dispose it during shutdown.
