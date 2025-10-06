using System;

namespace SafeKeyRecorder.Models;

public sealed class AccessibilityPreference
{
    public bool HighContrastEnabled { get; private set; }

    public bool ScreenReaderEnabled { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public AccessibilityPreference(bool highContrastEnabled = false, bool screenReaderEnabled = false, DateTimeOffset? updatedAt = null)
    {
        HighContrastEnabled = highContrastEnabled;
        ScreenReaderEnabled = screenReaderEnabled;
        UpdatedAt = updatedAt ?? DateTimeOffset.UtcNow;
    }

    public void Update(bool highContrastEnabled, bool screenReaderEnabled, DateTimeOffset updatedAt)
    {
        HighContrastEnabled = highContrastEnabled;
        ScreenReaderEnabled = screenReaderEnabled;
        UpdatedAt = updatedAt;
    }
}
