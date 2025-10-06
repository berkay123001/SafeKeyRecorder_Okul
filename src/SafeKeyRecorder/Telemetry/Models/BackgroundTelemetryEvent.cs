using System;

namespace SafeKeyRecorder.Telemetry.Models;

public sealed class BackgroundTelemetryEvent
{
    public BackgroundTelemetryEvent(Guid eventId, string eventType, DateTimeOffset capturedAtUtc, string? systemLockState = null)
    {
        EventId = eventId;
        EventType = eventType;
        CapturedAtUtc = capturedAtUtc;
        SystemLockState = systemLockState;
    }

    public Guid EventId { get; }

    public string EventType { get; }

    public DateTimeOffset CapturedAtUtc { get; }

    public string? SystemLockState { get; }

    public string QueueState { get; set; } = "Pending";
}
