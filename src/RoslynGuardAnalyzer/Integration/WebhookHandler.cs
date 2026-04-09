#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoslynGuardAnalyzer.Integration;

/// <summary>
/// Manages webhook registrations and dispatches analysis results to external endpoints.
/// Useful for CI/CD pipeline integrations and notifications.
/// </summary>
public sealed class WebhookHandler
{
    public sealed class WebhookRegistration
    {
        public required string Id { get; init; }
        public required string Url { get; init; }
        public required string EventType { get; init; } // AnalysisCompleted, ViolationDetected, etc.
        public Dictionary<string, string> Headers { get; init; } = [];
        public bool IsActive { get; set; } = true;
        public DateTime RegisteredAt { get; init; } = DateTime.UtcNow;
    }

    private readonly List<WebhookRegistration> _webhooks = [];
    private readonly HttpClientFactory _httpClientFactory;
    private readonly object _lockObject = new();

    public WebhookHandler(HttpClientFactory? httpClientFactory = null)
    {
        _httpClientFactory = httpClientFactory ?? new HttpClientFactory();
    }

    /// <summary>
    /// Registers a new webhook for a specific event type.
    /// </summary>
    public string RegisterWebhook(string url, string eventType, Dictionary<string, string>? headers = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Webhook URL cannot be null or empty", nameof(url));

        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Event type cannot be null or empty", nameof(eventType));

        lock (_lockObject)
        {
            var registration = new WebhookRegistration
            {
                Id = Guid.NewGuid().ToString(),
                Url = url,
                EventType = eventType,
                Headers = headers ?? new Dictionary<string, string>()
            };

            _webhooks.Add(registration);
            return registration.Id;
        }
    }

    /// <summary>
    /// Unregisters a webhook by ID.
    /// </summary>
    public bool UnregisterWebhook(string webhookId)
    {
        lock (_lockObject)
        {
            var webhook = _webhooks.FirstOrDefault(w => w.Id == webhookId);
            return webhook is not null && _webhooks.Remove(webhook);
        }
    }

    /// <summary>
    /// Deactivates a webhook without removing it.
    /// </summary>
    public bool DeactivateWebhook(string webhookId)
    {
        lock (_lockObject)
        {
            var webhook = _webhooks.FirstOrDefault(w => w.Id == webhookId);
            if (webhook is not null)
            {
                webhook.IsActive = false;
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Triggers webhooks for a specific event type with the given payload.
    /// </summary>
    public async Task TriggerWebhooksAsync(string eventType, string jsonPayload)
    {
        List<WebhookRegistration> matchingWebhooks;

        lock (_lockObject)
        {
            matchingWebhooks = _webhooks
                .Where(w => w.IsActive && w.EventType == eventType)
                .ToList();
        }

        var tasks = matchingWebhooks.Select(webhook =>
            SendWebhookAsync(webhook, jsonPayload)
        ).ToList();

        await Task.WhenAll(tasks.Where(t => t is not null)!);
    }

    /// <summary>
    /// Sends a webhook request to a registered endpoint.
    /// </summary>
    private async Task SendWebhookAsync(WebhookRegistration webhook, string payload)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(webhook.Url, $"webhook-{webhook.Id}");

            // Add custom headers
            foreach (var header in webhook.Headers)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }

            // Add X-Event-Type header
            client.DefaultRequestHeaders.Add("X-Event-Type", webhook.EventType);
            client.DefaultRequestHeaders.Add("X-Timestamp", DateTime.UtcNow.ToIso8601String());

            var response = await _httpClientFactory.ExecuteWithRetryAsync(client, async c =>
                await c.PostAsync(webhook.Url, new StringContent(payload, System.Text.Encoding.UTF8, "application/json")));

            if (!response.IsSuccessStatusCode)
            {
                Console.Error.WriteLine($"Webhook {webhook.Id} returned status {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error sending webhook {webhook.Id}: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets all registered webhooks.
    /// </summary>
    public IReadOnlyList<WebhookRegistration> GetAllWebhooks()
    {
        lock (_lockObject)
        {
            return _webhooks.AsReadOnly();
        }
    }

    /// <summary>
    /// Gets active webhooks for a specific event type.
    /// </summary>
    public IReadOnlyList<WebhookRegistration> GetWebhooksForEvent(string eventType)
    {
        lock (_lockObject)
        {
            return _webhooks
                .Where(w => w.IsActive && w.EventType == eventType)
                .ToList()
                .AsReadOnly();
        }
    }

    /// <summary>
    /// Gets the count of registered webhooks.
    /// </summary>
    public int WebhookCount
    {
        get
        {
            lock (_lockObject)
            {
                return _webhooks.Count;
            }
        }
    }
}

internal static class DateTimeExtensions
{
    public static string ToIso8601String(this DateTime dt)
    {
        return dt.ToString("yyyy-MM-ddTHH:mm:ssZ");
    }
}
