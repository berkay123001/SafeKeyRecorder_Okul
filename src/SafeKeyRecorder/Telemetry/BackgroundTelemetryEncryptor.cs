using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SafeKeyRecorder.Telemetry.Abstractions;
using SafeKeyRecorder.Telemetry.Models;

namespace SafeKeyRecorder.Telemetry;

public sealed class BackgroundTelemetryEncryptor : IBackgroundTelemetryEncryptor
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public Task<EncryptedTelemetryPacket> EncryptAsync(BackgroundTelemetryEnvelope envelope, CancellationToken cancellationToken)
    {
        if (envelope is null)
        {
            throw new ArgumentNullException(nameof(envelope));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var cipherText = JsonSerializer.SerializeToUtf8Bytes(envelope, SerializerOptions);
        var packet = new EncryptedTelemetryPacket(Guid.NewGuid(), cipherText, envelope.Mode);
        return Task.FromResult(packet);
    }
}
