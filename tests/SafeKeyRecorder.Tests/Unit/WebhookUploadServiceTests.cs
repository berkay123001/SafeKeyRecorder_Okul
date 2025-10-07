using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using SafeKeyRecorder.Configuration;
using SafeKeyRecorder.Models;
using SafeKeyRecorder.Services;
using SafeKeyRecorder.Telemetry;
using SafeKeyRecorder.Tests.TestSupport;
using Xunit;

namespace SafeKeyRecorder.Tests.Unit;

public sealed class WebhookUploadServiceTests : IDisposable
{
    private readonly WebhookTestServer _server = new();
    private readonly Mock<IComplianceAuditLogger> _auditLogger = new();
    private readonly WebhookOptions _options;

    public WebhookUploadServiceTests()
    {
        _options = new WebhookOptions
        {
            Endpoint = _server.BaseAddress.ToString(),
            BearerToken = "test-token",
            RetryCount = 2,
            RetryDelay = TimeSpan.FromMilliseconds(50)
        };
    }

    [Fact]
    public async Task UploadAsync_ShouldSendLogWithRequiredHeaders_WhenConsentGranted()
    {
        _server.EnqueueResponse(HttpStatusCode.Accepted);
        var service = CreateService();
        var consent = CreateConsent(granted: true);

        await service.UploadAsync(consent, "payload", CancellationToken.None);

        Assert.Single(_server.Requests);
        var request = _server.Requests[0];
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal(_server.BaseAddress, request.Uri);
        Assert.Equal("Bearer test-token", request.GetHeader("Authorization"));
        Assert.Equal("text/plain; charset=utf-8", request.GetHeader("Content-Type"));
        Assert.Equal("payload", request.Body);
    }

    [Fact]
    public async Task UploadAsync_ShouldRetry_WhenServerReturnsError()
    {
        _server.EnqueueResponse(HttpStatusCode.InternalServerError);
        _server.EnqueueResponse(HttpStatusCode.Accepted);
        var service = CreateService();
        var consent = CreateConsent(granted: true);

        await service.UploadAsync(consent, "payload", CancellationToken.None);

        Assert.Equal(2, _server.Requests.Count);
        _auditLogger.Verify(logger => logger.LogWebhookAttempt(It.IsAny<WebhookTransmissionAttempt>()), Times.Exactly(2));
    }

    [Fact]
    public async Task UploadAsync_ShouldNotSend_WhenConsentRevoked()
    {
        var service = CreateService();
        var consent = CreateConsent(granted: false);

        await service.UploadAsync(consent, "payload", CancellationToken.None);

        Assert.Empty(_server.Requests);
        _auditLogger.Verify(logger => logger.LogWebhookAttempt(It.IsAny<WebhookTransmissionAttempt>()), Times.Never);
    }

    public void Dispose()
    {
        _server.Dispose();
    }

    private WebhookUploadService CreateService()
    {
        return new WebhookUploadService(_server.CreateClient(), _options, _auditLogger.Object);
    }

    private static WebhookConsentRecord CreateConsent(bool granted)
    {
        return new WebhookConsentRecord(Guid.NewGuid(), granted ? DateTimeOffset.UtcNow : null, granted);
    }
}
