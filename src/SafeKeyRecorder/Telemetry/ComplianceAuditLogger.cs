using System;
using System.Collections.Generic;
using SafeKeyRecorder.Models;

namespace SafeKeyRecorder.Telemetry;

public sealed class ComplianceAuditLogger : IComplianceAuditLogger
{
    private readonly List<string> _entries = new();

    public IReadOnlyList<string> Entries => _entries;

    public void LogConsentGranted(ConsentDecision decision, string mode)
    {
        if (decision is null)
        {
            throw new ArgumentNullException(nameof(decision));
        }

        if (string.IsNullOrWhiteSpace(mode))
        {
            throw new ArgumentException("Mode must be provided.", nameof(mode));
        }

        _entries.Add(
            $"CONSENT_GRANTED_BACKGROUND|mode={mode}|decisionId={decision.ConsentDecisionId}|logging={decision.LoggingEnabled}|timestamp={decision.DecidedAt:O}");
    }

    public void LogConsentDeclined(DateTimeOffset occurredAt, string mode)
    {
        if (string.IsNullOrWhiteSpace(mode))
        {
            throw new ArgumentException("Mode must be provided.", nameof(mode));
        }

        _entries.Add($"CONSENT_DECLINED_BACKGROUND|mode={mode}|timestamp={occurredAt:O}");
    }

    public void LogConsentGranted(ConsentSession session)
    {
        if (session is null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        if (session.ConsentGrantedAt is null)
        {
            throw new InvalidOperationException("Consent timestamp must be set before logging consent grant.");
        }

        _entries.Add($"CONSENT_GRANTED|{session.SessionId}|{session.ConsentGrantedAt:O}|logging={session.LoggingEnabled}");
    }

    public void LogConsentDeclined(Guid sessionId, DateTimeOffset occurredAt)
    {
        _entries.Add($"CONSENT_DECLINED|{sessionId}|{occurredAt:O}");
    }

    public void LogRetentionDecision(ConsentSession session)
    {
        if (session is null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        var expiry = session.RetentionExpiresAt?.ToString("O") ?? "none";
        _entries.Add($"RETENTION|{session.SessionId}|autoDelete={session.AutoDeleteRequested}|expires={expiry}");
    }

    public void LogPurge(Guid sessionId, DateTimeOffset purgedAt, bool dueToAutoDelete)
    {
        _entries.Add($"PURGE|{sessionId}|{purgedAt:O}|auto={dueToAutoDelete}");
    }
}
