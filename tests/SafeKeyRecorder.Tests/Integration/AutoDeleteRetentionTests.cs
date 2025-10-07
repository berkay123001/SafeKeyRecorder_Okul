using Xunit;

namespace SafeKeyRecorder.Tests.Integration;

public class AutoDeleteRetentionTests
{
    [Fact(Skip = "Assignment scope excludes auto-delete retention workflow")]
    public void AutoDelete_ShouldPromptAndSchedulePurge()
    {
        // TODO: Once ViewModels exist, simulate enabling logging and selecting purge checkbox.
        Assert.Fail("Auto delete retention workflow not implemented yet.");
    }
}
