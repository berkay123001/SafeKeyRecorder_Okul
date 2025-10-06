using System;
using SafeKeyRecorder.Services.Abstractions;

namespace SafeKeyRecorder.Services;

public sealed class SystemLockMonitor : ISystemLockMonitor
{
    private bool _isRunning;

    public event EventHandler? Locked;

    public event EventHandler? Unlocked;

    public void Start()
    {
        _isRunning = true;
    }

    public void Stop()
    {
        _isRunning = false;
    }

    public void Dispose()
    {
        _isRunning = false;
    }

    // Expose methods for other components to trigger events
    public void RaiseLocked() => Locked?.Invoke(this, EventArgs.Empty);

    public void RaiseUnlocked()
    {
        if (!_isRunning)
        {
            return;
        }

        Unlocked?.Invoke(this, EventArgs.Empty);
    }
}
