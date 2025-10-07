using Xunit;

namespace SafeKeyRecorder.Tests.UiAutomation;

public class KeyLatencyPlaywrightTests
{
    [Fact(Skip = "Assignment scope does not require latency automation")]
    public void Latency_ShouldRemainBelowThreshold()
    {
        Assert.Fail("Playwright latency automation not implemented yet.");
    }
}
