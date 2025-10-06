using System;
using SafeKeyRecorder.Models;
using SafeKeyRecorder.Telemetry;

namespace SafeKeyRecorder.Tests.Unit;

public class ComplianceAuditLoggerTests
{
    [Fact]
    public void LogConsentGranted_ShouldRecordEntryWithTimestamp()
    {
        var logger = new ComplianceAuditLogger();
        var session = new ConsentSession();
        var grantedAt = DateTimeOffset.UtcNow;
        session.GrantConsent(grantedAt, loggingEnabled: true, logFilePath: "/tmp/file.txt");

        logger.LogConsentGranted(session);

        Assert.Single(logger.Entries);
        Assert.StartsWith("CONSENT_GRANTED|", logger.Entries[0]);
        Assert.Contains(session.SessionId.ToString(), logger.Entries[0], StringComparison.Ordinal);
    }

    [Fact]
    public void LogConsentDeclined_ShouldRecordEntry()
    {
        var logger = new ComplianceAuditLogger();
        var sessionId = Guid.NewGuid();
        var declinedAt = DateTimeOffset.UtcNow;

        logger.LogConsentDeclined(sessionId, declinedAt);

        Assert.Contains("CONSENT_DECLINED", logger.Entries[0], StringComparison.Ordinal);
        Assert.Contains(sessionId.ToString(), logger.Entries[0], StringComparison.Ordinal);
    }

    [Fact]
    public void LogRetentionDecision_ShouldIncludeAutoDeleteFlag()
    {
        var logger = new ComplianceAuditLogger();
        var session = new ConsentSession();
        session.GrantConsent(DateTimeOffset.UtcNow, true, "/tmp/file.txt");
        session.SetAutoDelete(true);

        logger.LogRetentionDecision(session);

        Assert.Contains("autoDelete=True", logger.Entries[0], StringComparison.Ordinal);
    }

    [Fact]
    public void LogPurge_ShouldIncludeReason()
    {
        var logger = new ComplianceAuditLogger();
        var sessionId = Guid.NewGuid();
        var purgedAt = DateTimeOffset.UtcNow;

        logger.LogPurge(sessionId, purgedAt, dueToAutoDelete: true);

        Assert.Contains("PURGE", logger.Entries[0], StringComparison.Ordinal);
        Assert.Contains("auto=True", logger.Entries[0], StringComparison.Ordinal);
    }
}
