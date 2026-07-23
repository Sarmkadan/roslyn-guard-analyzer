#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RoslynGuardAnalyzer.Tests;

public class WebhookHandlerTests
{
    private readonly RoslynGuardAnalyzer.Integration.HttpClientFactory _httpClientFactory = new();
    private const string TestUrl = "https://example.com/webhook";
    private const string TestEventType = "AnalysisCompleted";

    #region Constructor Tests

    [Fact] public void Constructor_WithNullHttpClientFactory_CreatesDefaultInstance() =>
        Assert.NotNull(new RoslynGuardAnalyzer.Integration.WebhookHandler(httpClientFactory: null));

    [Fact] public void Constructor_WithValidHttpClientFactory_InitializesInstance() =>
        Assert.NotNull(new RoslynGuardAnalyzer.Integration.WebhookHandler(new()));

    [Fact] public void Constructor_WithDefaultParameters_InitializesEmptyWebhookList()
    {
        var handler = new RoslynGuardAnalyzer.Integration.WebhookHandler();
        Assert.Empty(handler.GetAllWebhooks());
        Assert.Equal(0, handler.WebhookCount);
    }

    #endregion

    #region RegisterWebhook Tests

    [Fact] public void RegisterWebhook_WithValidParameters_ReturnsNonEmptyId()
    {
        var handler = new RoslynGuardAnalyzer.Integration.WebhookHandler(_httpClientFactory);
        var id = handler.RegisterWebhook(TestUrl, TestEventType);
        Assert.NotEmpty(id);
        Assert.True(Guid.TryParse(id, out _));
    }

    [Fact] public void RegisterWebhook_WithValidParameters_AddsWebhookToCollection()
    {
        var handler = new RoslynGuardAnalyzer.Integration.WebhookHandler(_httpClientFactory);
        Assert.Equal(0, handler.WebhookCount);
        handler.RegisterWebhook(TestUrl, TestEventType);
        Assert.Equal(1, handler.WebhookCount);
    }

    [Fact] public void RegisterWebhook_WithValidParameters_CreatesActiveWebhook()
    {
        var handler = new RoslynGuardAnalyzer.Integration.WebhookHandler(_httpClientFactory);
        var id = handler.RegisterWebhook(TestUrl, TestEventType);
        var webhook = handler.GetAllWebhooks().First(w => w.Id == id);
        Assert.True(webhook.IsActive);
        Assert.Equal(TestUrl, webhook.Url);
        Assert.Equal(TestEventType, webhook.EventType);
    }

    [Fact] public void RegisterWebhook_WithCustomHeaders_AddsHeadersToWebhook()
    {
        var handler = new RoslynGuardAnalyzer.Integration.WebhookHandler(_httpClientFactory);
        var headers = new Dictionary<string, string> { { "Authorization", "Bearer token123" } };
        var id = handler.RegisterWebhook(TestUrl, TestEventType, headers);
        var webhook = handler.GetAllWebhooks().First(w => w.Id == id);
        Assert.Single(webhook.Headers);
        Assert.Equal("Bearer token123", webhook.Headers["Authorization"]);
    }

    [Fact] public void RegisterWebhook_WithNullHeaders_CreatesWebhookWithEmptyHeaders()
    {
        var handler = new RoslynGuardAnalyzer.Integration.WebhookHandler(_httpClientFactory);
        var id = handler.RegisterWebhook(TestUrl, TestEventType, headers: null);
        var webhook = handler.GetAllWebhooks().First(w => w.Id == id);
        Assert.Empty(webhook.Headers);
    }

    [Theory, InlineData(null), InlineData(""), InlineData("   ")]
    public void RegisterWebhook_WithNullOrEmptyUrl_ThrowsArgumentException(string? url)
    {
        var handler = new RoslynGuardAnalyzer.Integration.WebhookHandler(_httpClientFactory);
        Assert.Throws<ArgumentException>(() => handler.RegisterWebhook(url!, TestEventType));
    }

    [Theory, InlineData(null), InlineData(""), InlineData("   ")]
    public void RegisterWebhook_WithNullOrEmptyEventType_ThrowsArgumentException(string? eventType)
    {
        var handler = new RoslynGuardAnalyzer.Integration.WebhookHandler(_httpClientFactory);
        Assert.Throws<ArgumentException>(() => handler.RegisterWebhook(TestUrl, eventType!));
    }

    #endregion

    #region UnregisterWebhook Tests

    [Fact] public void UnregisterWebhook_WithValidId_ReturnsTrueAndRemovesWebhook()
    {
        var handler = new RoslynGuardAnalyzer.Integration.WebhookHandler(_httpClientFactory);
        var id = handler.RegisterWebhook(TestUrl, TestEventType);
        Assert.True(handler.UnregisterWebhook(id));
        Assert.Empty(handler.GetAllWebhooks());
    }

    [Fact] public void UnregisterWebhook_WithInvalidId_ReturnsFalse()
    {
        var handler = new RoslynGuardAnalyzer.Integration.WebhookHandler(_httpClientFactory);
        handler.RegisterWebhook(TestUrl, TestEventType);
        Assert.False(handler.UnregisterWebhook("invalid-id"));
        Assert.Single(handler.GetAllWebhooks());
    }

    [Fact] public void UnregisterWebhook_WithEmptyId_ReturnsFalse()
    {
        var handler = new RoslynGuardAnalyzer.Integration.WebhookHandler(_httpClientFactory);
        Assert.False(handler.UnregisterWebhook(string.Empty));
    }

    #endregion

    #region DeactivateWebhook Tests

    [Fact] public void DeactivateWebhook_WithValidId_DeactivatesWebhook()
    {
        var handler = new RoslynGuardAnalyzer.Integration.WebhookHandler(_httpClientFactory);
        var id = handler.RegisterWebhook(TestUrl, TestEventType);
        Assert.True(handler.DeactivateWebhook(id));
        Assert.False(handler.GetAllWebhooks().First(w => w.Id == id).IsActive);
    }

    [Fact] public void DeactivateWebhook_WithInvalidId_ReturnsFalse()
    {
        var handler = new RoslynGuardAnalyzer.Integration.WebhookHandler(_httpClientFactory);
        handler.RegisterWebhook(TestUrl, TestEventType);
        Assert.False(handler.DeactivateWebhook("invalid-id"));
    }

    [Fact] public void DeactivateWebhook_WithEmptyId_ReturnsFalse()
    {
        var handler = new RoslynGuardAnalyzer.Integration.WebhookHandler(_httpClientFactory);
        Assert.False(handler.DeactivateWebhook(string.Empty));
    }

    #endregion

    #region GetWebhooksForEvent Tests

    [Fact] public void GetWebhooksForEvent_WithMatchingEventType_ReturnsActiveWebhooks()
    {
        var handler = new RoslynGuardAnalyzer.Integration.WebhookHandler(_httpClientFactory);
        handler.RegisterWebhook(TestUrl, TestEventType);
        handler.RegisterWebhook("https://example.com/other", "OtherEvent");
        var webhooks = handler.GetWebhooksForEvent(TestEventType);
        Assert.Single(webhooks);
        Assert.Equal(TestEventType, webhooks[0].EventType);
    }

    [Fact] public void GetWebhooksForEvent_WithNonMatchingEventType_ReturnsEmptyList()
    {
        var handler = new RoslynGuardAnalyzer.Integration.WebhookHandler(_httpClientFactory);
        handler.RegisterWebhook(TestUrl, TestEventType);
        Assert.Empty(handler.GetWebhooksForEvent("NonExistent"));
    }

    [Fact] public void GetWebhooksForEvent_WithDeactivatedWebhooks_ReturnsOnlyActive()
    {
        var handler = new RoslynGuardAnalyzer.Integration.WebhookHandler(_httpClientFactory);
        var id1 = handler.RegisterWebhook(TestUrl, TestEventType);
        var id2 = handler.RegisterWebhook("https://example.com/webhook2", TestEventType);
        handler.DeactivateWebhook(id2);
        var webhooks = handler.GetWebhooksForEvent(TestEventType);
        Assert.Single(webhooks);
        Assert.Equal(id1, webhooks[0].Id);
    }

    #endregion

    #region GetAllWebhooks Tests

    [Fact] public void GetAllWebhooks_WithNoWebhooks_ReturnsEmptyList()
    {
        var handler = new RoslynGuardAnalyzer.Integration.WebhookHandler(_httpClientFactory);
        Assert.Empty(handler.GetAllWebhooks());
    }

    [Fact] public void GetAllWebhooks_WithMultipleWebhooks_ReturnsAll()
    {
        var handler = new RoslynGuardAnalyzer.Integration.WebhookHandler(_httpClientFactory);
        handler.RegisterWebhook(TestUrl, TestEventType);
        handler.RegisterWebhook("https://example.com/other", "OtherEvent");
        Assert.Equal(2, handler.WebhookCount);
    }

    #endregion

    #region Thread Safety Tests

    [Fact] public async Task RegisterWebhook_IsThreadSafe()
    {
        var handler = new RoslynGuardAnalyzer.Integration.WebhookHandler(_httpClientFactory);
        var tasks = Enumerable.Range(0, 10).Select(i => Task.Run(() =>
            handler.RegisterWebhook($"https://example.com/webhook{i}", TestEventType))).ToList();
        await Task.WhenAll(tasks);
        Assert.Equal(10, handler.WebhookCount);
    }

    [Fact] public async Task UnregisterWebhook_IsThreadSafe()
    {
        var handler = new RoslynGuardAnalyzer.Integration.WebhookHandler(_httpClientFactory);
        var ids = Enumerable.Range(0, 10).Select(_ => handler.RegisterWebhook(TestUrl, TestEventType)).ToList();
        var tasks = ids.Select(id => Task.Run(() => handler.UnregisterWebhook(id))).ToList();
        await Task.WhenAll(tasks);
        Assert.Equal(0, handler.WebhookCount);
    }

    #endregion
}