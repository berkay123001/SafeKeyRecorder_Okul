using System;
using System.Threading;
using System.Threading.Tasks;
using SafeKeyRecorder.Services.Abstractions;
using SafeKeyRecorder.Telemetry;

namespace SafeKeyRecorder.Services;

public sealed class BackgroundCaptureService : IBackgroundCaptureService, IDisposable
{
    private readonly IGlobalHookAdapter _hookAdapter;
    private readonly ISystemLockMonitor _lockMonitor;
    private readonly IBackgroundTelemetryExporter _telemetryExporter;
    private readonly object _syncRoot = new();
    private bool _backgroundCaptureEnabled;
    private bool _hookRunning;
    private bool _disposed;

    public BackgroundCaptureService(
        IGlobalHookAdapter hookAdapter,
        ISystemLockMonitor lockMonitor,
        IBackgroundTelemetryExporter telemetryExporter)
    {
        _hookAdapter = hookAdapter ?? throw new ArgumentNullException(nameof(hookAdapter));
        _lockMonitor = lockMonitor ?? throw new ArgumentNullException(nameof(lockMonitor));
        _telemetryExporter = telemetryExporter ?? throw new ArgumentNullException(nameof(telemetryExporter));

        _lockMonitor.Locked += OnLockMonitorLocked;
        _lockMonitor.Unlocked += OnLockMonitorUnlocked;
    }

    public async Task StartAsync(bool allowBackgroundCapture, CancellationToken cancellationToken)
    {
        EnsureNotDisposed();

        lock (_syncRoot)
        {
            _backgroundCaptureEnabled = allowBackgroundCapture;
        }

        if (!allowBackgroundCapture)
        {
            StopHook();
            _lockMonitor.Stop();
            return;
        }

        _lockMonitor.Start();
        await _hookAdapter.StartAsync(cancellationToken).ConfigureAwait(false);

        lock (_syncRoot)
        {
            _hookRunning = true;
        }
    }

    public void Stop()
    {
        EnsureNotDisposed();
        StopHook();
        _lockMonitor.Stop();
    }

    public void OnSystemLock()
    {
        EnsureNotDisposed();

        if (!IsBackgroundCaptureActive())
        {
            return;
        }

        StopHook();
        _telemetryExporter.EnqueuePauseEvent();
    }

    public void OnSystemUnlock()
    {
        EnsureNotDisposed();

        if (!IsBackgroundCaptureEnabled())
        {
            return;
        }

        _hookAdapter.RestartAsync(CancellationToken.None).GetAwaiter().GetResult();

        lock (_syncRoot)
        {
            _hookRunning = true;
        }

        _telemetryExporter.EnqueueResumeEvent();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _lockMonitor.Locked -= OnLockMonitorLocked;
        _lockMonitor.Unlocked -= OnLockMonitorUnlocked;

        StopHook();
        _lockMonitor.Stop();
        _lockMonitor.Dispose();
    }

    private void OnLockMonitorLocked(object? sender, EventArgs e) => OnSystemLock();

    private void OnLockMonitorUnlocked(object? sender, EventArgs e) => OnSystemUnlock();

    private void StopHook()
    {
        lock (_syncRoot)
        {
            if (!_hookRunning)
            {
                return;
            }

            _hookRunning = false;
        }

        _hookAdapter.Stop();
    }

    private bool IsBackgroundCaptureEnabled()
    {
        lock (_syncRoot)
        {
            return _backgroundCaptureEnabled;
        }
    }

    private bool IsBackgroundCaptureActive()
    {
        lock (_syncRoot)
        {
            return _backgroundCaptureEnabled && _hookRunning;
        }
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(BackgroundCaptureService));
        }
    }
}
