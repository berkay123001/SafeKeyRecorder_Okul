using System;
using SafeKeyRecorder.Models;

namespace SafeKeyRecorder.Services;

public sealed class AccessibilityService
{
    private AccessibilityPreference _preference;

    public event EventHandler<AccessibilityPreference>? PreferenceChanged;

    public AccessibilityService(AccessibilityPreference? initialPreference = null)
    {
        _preference = initialPreference ?? new AccessibilityPreference();
    }

    public AccessibilityPreference CurrentPreference => _preference;

    public void UpdatePreference(bool highContrastEnabled, bool screenReaderEnabled, DateTimeOffset updatedAt)
    {
        _preference.Update(highContrastEnabled, screenReaderEnabled, updatedAt);
        PreferenceChanged?.Invoke(this, _preference);
    }
}
