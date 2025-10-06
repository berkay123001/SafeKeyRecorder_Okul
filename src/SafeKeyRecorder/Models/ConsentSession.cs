using System;

namespace SafeKeyRecorder.Models;

[Flags]
public enum AccessibilityMode
{
    Default = 0,
    HighContrast = 1,
    ScreenReader = 2,
    Both = HighContrast | ScreenReader
}

public sealed class ConsentSession
{
    public Guid SessionId { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? ConsentGrantedAt { get; private set; }

    public bool LoggingEnabled { get; private set; }

    public string? LogFilePath { get; private set; }

    public bool AutoDeleteRequested { get; private set; }

    public DateTimeOffset? RetentionExpiresAt { get; private set; }

    public AccessibilityMode AccessibilityMode { get; private set; } = AccessibilityMode.Default;

    public ConsentSession(Guid? sessionId = null, DateTimeOffset? createdAt = null)
    {
        SessionId = sessionId ?? Guid.NewGuid();
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
    }

    public void GrantConsent(DateTimeOffset grantedAt, bool loggingEnabled, string? logFilePath)
    {
        ConsentGrantedAt = grantedAt;
        UpdateLogging(loggingEnabled, logFilePath, grantedAt);
    }

    public void UpdateLogging(bool enabled, string? logFilePath, DateTimeOffset referenceTime)
    {
        LoggingEnabled = enabled;
        LogFilePath = enabled ? EnsureLogFilePath(logFilePath) : null;
        RetentionExpiresAt = enabled ? referenceTime.AddHours(24) : null;
    }

    public void SetAutoDelete(bool autoDeleteRequested)
    {
        AutoDeleteRequested = autoDeleteRequested;
    }

    public void SetAccessibilityMode(AccessibilityMode mode)
    {
        if (mode == AccessibilityMode.Both)
        {
            AccessibilityMode = AccessibilityMode.Both;
            return;
        }

        AccessibilityMode = mode;
    }

    private static string EnsureLogFilePath(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Log file path must be provided when logging is enabled.", nameof(value));
        }

        return value;
    }
}
