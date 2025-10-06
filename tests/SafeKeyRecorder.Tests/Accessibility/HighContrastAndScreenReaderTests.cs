using System;
using SafeKeyRecorder.Models;
using SafeKeyRecorder.Services;

namespace SafeKeyRecorder.Tests.Accessibility;

public class HighContrastAndScreenReaderTests
{
    [Fact]
    public void UpdatePreference_ShouldRaiseEventWithNewValues()
    {
        var initial = new AccessibilityPreference();
        var service = new AccessibilityService(initial);

        AccessibilityPreference? observed = null;
        service.PreferenceChanged += (_, preference) => observed = preference;

        var updatedAt = DateTimeOffset.UtcNow;
        service.UpdatePreference(highContrastEnabled: true, screenReaderEnabled: true, updatedAt);

        Assert.NotNull(observed);
        Assert.True(observed!.HighContrastEnabled);
        Assert.True(observed.ScreenReaderEnabled);
        Assert.Equal(updatedAt, observed.UpdatedAt);
    }

    [Fact]
    public void Constructor_ShouldDefaultToDisabledPreferences()
    {
        var service = new AccessibilityService();

        var preference = service.CurrentPreference;

        Assert.False(preference.HighContrastEnabled);
        Assert.False(preference.ScreenReaderEnabled);
    }
}
