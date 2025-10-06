using System;

namespace SafeKeyRecorder.Services.Abstractions;

public interface ISystemLockMonitor : IDisposable
{
    event EventHandler? Locked;

    event EventHandler? Unlocked;

    void Start();

    void Stop();
}
