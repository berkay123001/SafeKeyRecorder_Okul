using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SafeKeyRecorder.Configuration;
using SafeKeyRecorder.Models;
using SafeKeyRecorder.Telemetry;

namespace SafeKeyRecorder.Services;

public sealed class WebhookUploadService
{
    private readonly HttpClient _httpClient;
    private readonly WebhookOptions _options;
    private readonly IComplianceAuditLogger _auditLogger;

    public WebhookUploadService(HttpClient httpClient, WebhookOptions options, IComplianceAuditLogger auditLogger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
    }

    public async Task UploadAsync(WebhookConsentRecord consent, string payload, CancellationToken cancellationToken)
    {
        if (consent is null)
        {
            throw new ArgumentNullException(nameof(consent));
        }

        if (payload is null)
        {
            throw new ArgumentNullException(nameof(payload));
        }

        if (!consent.CanUpload)
        {
            return;
        }

        var endpoint = new Uri(_options.Endpoint, UriKind.Absolute);
        var attemptNumber = 0;
        var maxAttempts = Math.Max(1, _options.RetryCount);
        var delay = _options.RetryDelay;

        while (attemptNumber < maxAttempts)
        {
            attemptNumber++;

            using var request = CreateRequest(endpoint, payload);

            try
            {
                var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                var success = response.IsSuccessStatusCode;
                var statusCode = response.StatusCode;
                var error = success ? null : $"StatusCode={statusCode}";

                _auditLogger.LogWebhookAttempt(new WebhookTransmissionAttempt(
                    consent.SessionId,
                    endpoint,
                    DateTimeOffset.UtcNow,
                    statusCode,
                    success,
                    error,
                    attemptNumber));

                if (success)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                _auditLogger.LogWebhookAttempt(new WebhookTransmissionAttempt(
                    consent.SessionId,
                    endpoint,
                    DateTimeOffset.UtcNow,
                    null,
                    success: false,
                    error: ex.Message,
                    attemptNumber));

                if (attemptNumber >= maxAttempts)
                {
                    return;
                }
            }

            if (attemptNumber < maxAttempts)
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private HttpRequestMessage CreateRequest(Uri endpoint, string payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(payload, System.Text.Encoding.UTF8, "text/plain")
        };

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.BearerToken);
        request.Content.Headers.TryAddWithoutValidation("X-SKR-Payload", _options.PayloadName);

        return request;
    }
}
