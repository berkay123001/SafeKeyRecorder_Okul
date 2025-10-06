using System;

namespace SafeKeyRecorder.Models;

public sealed record ConsentDecision(bool Accepted, bool LoggingEnabled, bool AutoDeleteRequested, DateTimeOffset DecidedAt)
{
    public bool AllowBackgroundCapture { get; init; }

    public Guid ConsentDecisionId { get; init; } = Guid.NewGuid();
}
