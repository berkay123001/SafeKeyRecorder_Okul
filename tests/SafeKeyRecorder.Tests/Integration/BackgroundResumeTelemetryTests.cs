using System.Threading;
using System.Threading.Tasks;
using Moq;
using SafeKeyRecorder.Background;
using SafeKeyRecorder.Services;
using SafeKeyRecorder.Telemetry;
using Xunit;

namespace SafeKeyRecorder.Tests.Integration;

public class BackgroundResumeTelemetryTests
{
    [Fact]
    public async Task Resume_ShouldRestartCaptureAndEmitResumeEvent()
    {
        var captureService = new Mock<IBackgroundCaptureService>();
        var telemetryExporter = new Mock<IBackgroundTelemetryExporter>();
        var lockMonitor = new Mock<ISystemLockMonitor>();

        var coordinator = new BackgroundResumeCoordinator(
            captureService.Object,
            telemetryExporter.Object,
            lockMonitor.Object);

        await coordinator.HandleLockedAsync(CancellationToken.None);
        await coordinator.HandleUnlockedAsync(CancellationToken.None);

        captureService.Verify(c => c.Stop(), Times.Once);
        captureService.Verify(c => c.StartAsync(true, It.IsAny<CancellationToken>()), Times.Once);
        telemetryExporter.Verify(t => t.EnqueueResumeEvent(), Times.Once);
    }

    [Fact]
    public async Task Resume_WhenBackgroundDisabled_ShouldNotRestartCapture()
    {
        var captureService = new Mock<IBackgroundCaptureService>();
        var telemetryExporter = new Mock<IBackgroundTelemetryExporter>();
        var lockMonitor = new Mock<ISystemLockMonitor>();

        var coordinator = new BackgroundResumeCoordinator(
            captureService.Object,
            telemetryExporter.Object,
            lockMonitor.Object);

        coordinator.DisableBackgroundCapture();

        await coordinator.HandleLockedAsync(CancellationToken.None);
        await coordinator.HandleUnlockedAsync(CancellationToken.None);

        captureService.Verify(c => c.StartAsync(true, It.IsAny<CancellationToken>()), Times.Never);
        telemetryExporter.Verify(t => t.EnqueueResumeEvent(), Times.Never);
    }
}
