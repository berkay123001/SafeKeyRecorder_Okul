using System;
using System.Net;

namespace SafeKeyRecorder.Models;

public sealed class WebhookTransmissionAttempt
{
    public WebhookTransmissionAttempt(
        Guid sessionId,
        Uri endpoint,
        DateTimeOffset attemptedAt,
        HttpStatusCode? statusCode,
        bool success,
        string? error = null,
        int attemptNumber = 1)
    {
        SessionId = sessionId;
        Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        AttemptedAt = attemptedAt;
        StatusCode = statusCode;
        Success = success;
        Error = error;
        AttemptNumber = attemptNumber;
    }

    public Guid SessionId { get; }

    public Uri Endpoint { get; }

    public DateTimeOffset AttemptedAt { get; }

    public HttpStatusCode? StatusCode { get; }

    public bool Success { get; }

    public string? Error { get; }

    public int AttemptNumber { get; }
}
