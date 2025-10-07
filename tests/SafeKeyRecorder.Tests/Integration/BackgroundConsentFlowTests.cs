using System.Threading;
using System.Threading.Tasks;
using Moq;
using SafeKeyRecorder.Background;
using SafeKeyRecorder.Models;
using SafeKeyRecorder.Services;
using SafeKeyRecorder.Services.Abstractions;
using SafeKeyRecorder.Telemetry;
using Xunit;

namespace SafeKeyRecorder.Tests.Integration;

public class BackgroundConsentFlowTests
{
    [Fact]
    public async Task AcceptingBackgroundConsent_ShouldEnableCaptureAndEmitTelemetry()
    {
        var captureService = new Mock<IBackgroundCaptureService>();
        var telemetryExporter = new Mock<IBackgroundTelemetryExporter>();
        var auditLogger = new Mock<IComplianceAuditLogger>();
        var banner = new Mock<IBackgroundStatusBanner>();

        var coordinator = new BackgroundConsentCoordinator(
            captureService.Object,
            telemetryExporter.Object,
            auditLogger.Object,
            banner.Object);

        var decision = new ConsentDecision(
            Accepted: true,
            LoggingEnabled: true,
            AutoDeleteRequested: true,
            DecidedAt: System.DateTimeOffset.UtcNow)
        {
            AllowBackgroundCapture = true
        };

        await coordinator.ApplyDecisionAsync(decision, CancellationToken.None);

        captureService.Verify(c => c.StartAsync(true, It.IsAny<CancellationToken>()), Times.Once);
        telemetryExporter.Verify(t => t.EnqueueConsentGranted(decision), Times.Once);
        banner.Verify(b => b.ShowBackgroundEnabled(), Times.Once);
    }

    [Fact]
    public async Task DecliningBackgroundConsent_ShouldDisableCaptureAndLogAudit()
    {
        var captureService = new Mock<IBackgroundCaptureService>();
        var telemetryExporter = new Mock<IBackgroundTelemetryExporter>();
        var auditLogger = new Mock<IComplianceAuditLogger>();
        var banner = new Mock<IBackgroundStatusBanner>();

        var coordinator = new BackgroundConsentCoordinator(
            captureService.Object,
            telemetryExporter.Object,
            auditLogger.Object,
            banner.Object);

        var decision = new ConsentDecision(
            Accepted: false,
            LoggingEnabled: false,
            AutoDeleteRequested: true,
            DecidedAt: System.DateTimeOffset.UtcNow)
        {
            AllowBackgroundCapture = false
        };

        await coordinator.ApplyDecisionAsync(decision, CancellationToken.None);

        captureService.Verify(c => c.Stop(), Times.Once);
        auditLogger.Verify(a => a.LogConsentDeclined(decision.DecidedAt, "background"), Times.Once);
        banner.Verify(b => b.ShowBackgroundDisabled(), Times.Once);
    }
}
