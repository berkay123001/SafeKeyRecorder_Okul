using System.Threading;
using System.Threading.Tasks;
using SafeKeyRecorder.Telemetry.Models;

namespace SafeKeyRecorder.Telemetry.Abstractions;

public interface ISecureTelemetryQueue
{
    Task WriteAsync(EncryptedTelemetryPacket packet, CancellationToken cancellationToken);
}
