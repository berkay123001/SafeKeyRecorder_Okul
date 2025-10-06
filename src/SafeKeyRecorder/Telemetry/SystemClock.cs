using System;
using SafeKeyRecorder.Telemetry.Abstractions;

namespace SafeKeyRecorder.Telemetry;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
