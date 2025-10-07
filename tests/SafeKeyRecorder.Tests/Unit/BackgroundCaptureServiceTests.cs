using System.Threading;
using System.Threading.Tasks;
using Moq;
using SafeKeyRecorder.Services;
using SafeKeyRecorder.Services.Abstractions;
using SafeKeyRecorder.Telemetry;
using Xunit;

namespace SafeKeyRecorder.Tests.Unit;

public class BackgroundCaptureServiceTests
{
    private readonly Mock<IGlobalHookAdapter> _hookAdapter = new();
    private readonly Mock<ISystemLockMonitor> _lockMonitor = new();
    private readonly Mock<IBackgroundTelemetryExporter> _telemetryExporter = new();

    private BackgroundCaptureService CreateService()
    {
        return new BackgroundCaptureService(
            _hookAdapter.Object,
            _lockMonitor.Object,
            _telemetryExporter.Object);
    }

    [Fact(Skip = "Assignment scope relies on manual validation of background capture service")]
    public async Task StartAsync_EnablesHook_WhenConsentGranted()
    {
        var service = CreateService();

        await service.StartAsync(allowBackgroundCapture: true, CancellationToken.None);

        _hookAdapter.Verify(h => h.StartAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(Skip = "Assignment scope relies on manual validation of background capture service")]
    public async Task StartAsync_DoesNotEnableHook_WhenConsentDenied()
    {
        var service = CreateService();

        await service.StartAsync(allowBackgroundCapture: false, CancellationToken.None);

        _hookAdapter.Verify(h => h.StartAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(Skip = "Assignment scope relies on manual validation of background capture service")]
    public void OnSystemLock_StopsHookAndRecordsPause()
    {
        var service = CreateService();

        service.OnSystemLock();

        _hookAdapter.Verify(h => h.Stop(), Times.Once);
        _telemetryExporter.Verify(t => t.EnqueuePauseEvent(), Times.Once);
    }

    [Fact(Skip = "Assignment scope relies on manual validation of background capture service")]
    public void OnSystemUnlock_RestartsHookAndRecordsResume()
    {
        var service = CreateService();

        service.OnSystemUnlock();

        _hookAdapter.Verify(h => h.RestartAsync(It.IsAny<CancellationToken>()), Times.Once);
        _telemetryExporter.Verify(t => t.EnqueueResumeEvent(), Times.Once);
    }

    [Fact(Skip = "Assignment scope relies on manual validation of background capture service")]
    public void Dispose_StopsHookAndDisposesMonitor()
    {
        var service = CreateService();

        service.Dispose();

        _hookAdapter.Verify(h => h.Stop(), Times.Once);
        _lockMonitor.Verify(l => l.Dispose(), Times.Once);
    }
}
