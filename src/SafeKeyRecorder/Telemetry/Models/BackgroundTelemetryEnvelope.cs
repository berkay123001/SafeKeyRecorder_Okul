using System;
using System.Collections.Generic;

namespace SafeKeyRecorder.Telemetry.Models;

public sealed class BackgroundTelemetryEnvelope
{
    public BackgroundTelemetryEnvelope(Guid consentDecisionId, string mode, DateTimeOffset queuedAtUtc)
    {
        ConsentDecisionId = consentDecisionId;
        Mode = mode;
        QueuedAtUtc = queuedAtUtc;
        Events = new List<BackgroundTelemetryEvent>();
    }

    public Guid ConsentDecisionId { get; }

    public string Mode { get; }

    public DateTimeOffset QueuedAtUtc { get; }

    public DateTimeOffset? FlushedAtUtc { get; set; }

    public List<BackgroundTelemetryEvent> Events { get; }

    public byte[]? EncryptionNonce { get; set; }

    public byte[]? Signature { get; set; }
}
