using System;

namespace SafeKeyRecorder.Telemetry.Models;

public sealed class EncryptedTelemetryPacket
{
    public EncryptedTelemetryPacket(Guid packetId, byte[] cipherText, string mode)
    {
        PacketId = packetId;
        CipherText = cipherText;
        Mode = mode;
    }

    public Guid PacketId { get; }

    public byte[] CipherText { get; }

    public string Mode { get; }
}
