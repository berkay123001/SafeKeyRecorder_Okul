using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SafeKeyRecorder.Telemetry.Abstractions;
using SafeKeyRecorder.Telemetry.Models;

namespace SafeKeyRecorder.Telemetry;

public sealed class SecureTelemetryQueue : ISecureTelemetryQueue
{
    private readonly string _queueDirectory;

    public SecureTelemetryQueue(string? queueDirectory = null)
    {
        var defaultDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "SafeKeyRecorder",
            "telemetry");

        _queueDirectory = queueDirectory ?? defaultDirectory;
    }

    public async Task WriteAsync(EncryptedTelemetryPacket packet, CancellationToken cancellationToken)
    {
        if (packet is null)
        {
            throw new ArgumentNullException(nameof(packet));
        }

        cancellationToken.ThrowIfCancellationRequested();

        Directory.CreateDirectory(_queueDirectory);

        var payload = new
        {
            packet.PacketId,
            packet.Mode,
            CipherText = Convert.ToBase64String(packet.CipherText)
        };

        var json = JsonSerializer.Serialize(payload);
        var fileName = $"{packet.PacketId:yyyyMMddHHmmssfff}.json";
        var fullPath = Path.Combine(_queueDirectory, fileName);

        await File.WriteAllTextAsync(fullPath, json, cancellationToken).ConfigureAwait(false);
    }
}
