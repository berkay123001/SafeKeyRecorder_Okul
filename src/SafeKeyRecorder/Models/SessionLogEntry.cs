using System;

namespace SafeKeyRecorder.Models;

public sealed class SessionLogEntry
{
    public Guid SessionId { get; }

    public DateTimeOffset RecordedAt { get; }

    public string KeySymbol { get; }

    public bool IsPrintable { get; }

    public string[] Modifiers { get; }

    public bool WasLoggedToFile { get; }

    public SessionLogEntry(Guid sessionId, DateTimeOffset recordedAt, string keySymbol, bool isPrintable, string[] modifiers, bool wasLoggedToFile)
    {
        SessionId = sessionId;
        RecordedAt = recordedAt;
        KeySymbol = keySymbol ?? throw new ArgumentNullException(nameof(keySymbol));
        IsPrintable = isPrintable;
        Modifiers = modifiers ?? Array.Empty<string>();
        WasLoggedToFile = wasLoggedToFile;
    }
}
