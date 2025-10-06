using System.Threading;
using System.Threading.Tasks;
using SafeKeyRecorder.Telemetry.Models;

namespace SafeKeyRecorder.Telemetry.Abstractions;

public interface IBackgroundTelemetryEncryptor
{
    Task<EncryptedTelemetryPacket> EncryptAsync(BackgroundTelemetryEnvelope envelope, CancellationToken cancellationToken);
}
