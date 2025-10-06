using System;

namespace SafeKeyRecorder.Telemetry.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
