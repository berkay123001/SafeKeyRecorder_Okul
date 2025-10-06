using System;
using System.Threading;
using System.Threading.Tasks;
using SafeKeyRecorder.Services;
using SafeKeyRecorder.Services.Abstractions;
using SafeKeyRecorder.Telemetry;

namespace SafeKeyRecorder.Background;

public sealed class BackgroundResumeCoordinator : IDisposable
{
    private readonly IBackgroundCaptureService _captureService;
    private readonly IBackgroundTelemetryExporter _telemetryExporter;
    private readonly ISystemLockMonitor _lockMonitor;
    private readonly object _syncRoot = new();
    private bool _backgroundCaptureEnabled = true;
    private bool _disposed;

    public BackgroundResumeCoordinator(
        IBackgroundCaptureService captureService,
        IBackgroundTelemetryExporter telemetryExporter,
        ISystemLockMonitor lockMonitor)
    {
        _captureService = captureService ?? throw new ArgumentNullException(nameof(captureService));
        _telemetryExporter = telemetryExporter ?? throw new ArgumentNullException(nameof(telemetryExporter));
        _lockMonitor = lockMonitor ?? throw new ArgumentNullException(nameof(lockMonitor));

        _lockMonitor.Locked += OnLocked;
        _lockMonitor.Unlocked += OnUnlocked;
        _lockMonitor.Start();
    }

    public void DisableBackgroundCapture()
    {
        EnsureNotDisposed();

        lock (_syncRoot)
        {
            _backgroundCaptureEnabled = false;
        }
    }

    public void EnableBackgroundCapture()
    {
        EnsureNotDisposed();

        lock (_syncRoot)
        {
            _backgroundCaptureEnabled = true;
        }
    }

    public async Task HandleLockedAsync(CancellationToken cancellationToken)
    {
        EnsureNotDisposed();

        _captureService.Stop();
        _telemetryExporter.EnqueuePauseEvent();

        await Task.CompletedTask;
    }

    public async Task HandleUnlockedAsync(CancellationToken cancellationToken)
    {
        EnsureNotDisposed();

        if (!IsBackgroundCaptureEnabled())
        {
            return;
        }

        await _captureService.StartAsync(true, cancellationToken).ConfigureAwait(false);
        _telemetryExporter.EnqueueResumeEvent();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _lockMonitor.Locked -= OnLocked;
        _lockMonitor.Unlocked -= OnUnlocked;
        _lockMonitor.Stop();
    }

    private void OnLocked(object? sender, EventArgs e)
    {
        _ = HandleLockedAsync(CancellationToken.None);
    }

    private void OnUnlocked(object? sender, EventArgs e)
    {
        _ = HandleUnlockedAsync(CancellationToken.None);
    }

    private bool IsBackgroundCaptureEnabled()
    {
        lock (_syncRoot)
        {
            return _backgroundCaptureEnabled;
        }
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(BackgroundResumeCoordinator));
        }
    }
}
