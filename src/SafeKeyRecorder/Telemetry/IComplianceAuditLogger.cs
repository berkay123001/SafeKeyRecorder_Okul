using System;
using SafeKeyRecorder.Models;

namespace SafeKeyRecorder.Telemetry;

public interface IComplianceAuditLogger
{
    void LogConsentGranted(ConsentDecision decision, string mode);

    void LogConsentDeclined(DateTimeOffset occurredAt, string mode);

    void LogWebhookAttempt(WebhookTransmissionAttempt attempt);
}
