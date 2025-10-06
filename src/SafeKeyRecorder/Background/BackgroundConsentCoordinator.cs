using System;
using System.Threading;
using System.Threading.Tasks;
using SafeKeyRecorder.Models;
using SafeKeyRecorder.Services.Abstractions;
using SafeKeyRecorder.Telemetry;

namespace SafeKeyRecorder.Background;

public sealed class BackgroundConsentCoordinator
{
    private readonly IBackgroundCaptureService _captureService;
    private readonly IBackgroundTelemetryExporter _telemetryExporter;
    private readonly IComplianceAuditLogger _auditLogger;
    private readonly IBackgroundStatusBanner _statusBanner;

    public BackgroundConsentCoordinator(
        IBackgroundCaptureService captureService,
        IBackgroundTelemetryExporter telemetryExporter,
        IComplianceAuditLogger auditLogger,
        IBackgroundStatusBanner statusBanner)
    {
        _captureService = captureService ?? throw new ArgumentNullException(nameof(captureService));
        _telemetryExporter = telemetryExporter ?? throw new ArgumentNullException(nameof(telemetryExporter));
        _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
        _statusBanner = statusBanner ?? throw new ArgumentNullException(nameof(statusBanner));
    }

    public async Task ApplyDecisionAsync(ConsentDecision decision, CancellationToken cancellationToken)
    {
        if (decision is null)
        {
            throw new ArgumentNullException(nameof(decision));
        }

        if (decision.Accepted && decision.AllowBackgroundCapture)
        {
            _auditLogger.LogConsentGranted(decision, "background");
            await _captureService.StartAsync(true, cancellationToken).ConfigureAwait(false);
            _telemetryExporter.EnqueueConsentGranted(decision);
            _statusBanner.ShowBackgroundEnabled();
            return;
        }

        if (decision.Accepted)
        {
            _captureService.Stop();
            _auditLogger.LogConsentGranted(decision, "foreground");
            _statusBanner.ShowBackgroundDisabled();
            return;
        }

        _captureService.Stop();
        _auditLogger.LogConsentDeclined(decision.DecidedAt, "background");
        _statusBanner.ShowBackgroundDisabled();
        _statusBanner.ShowPassive("Kullanıcı arka plan modunu reddetti.");
    }

    public async Task SetBackgroundCaptureAsync(bool enabled, CancellationToken cancellationToken)
    {
        if (enabled)
        {
            await _captureService.StartAsync(true, cancellationToken).ConfigureAwait(false);
            _telemetryExporter.EnqueueToggleChanged(true);
            _statusBanner.ShowBackgroundEnabled();
            return;
        }

        _captureService.Stop();
        _telemetryExporter.EnqueueToggleChanged(false);
        _statusBanner.ShowBackgroundDisabled();
    }
}
