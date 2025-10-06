using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using SafeKeyRecorder.Telemetry;
using SafeKeyRecorder.Telemetry.Abstractions;
using SafeKeyRecorder.Telemetry.Models;
using Xunit;

namespace SafeKeyRecorder.Tests.Unit;

public class TelemetryExporterTests
{
    private readonly Mock<IClock> _clock = new();
    private readonly Mock<IBackgroundTelemetryEncryptor> _encryptor = new();
    private readonly Mock<ISecureTelemetryQueue> _secureQueue = new();

    private BackgroundTelemetryExporter CreateExporter()
    {
        _clock.Setup(c => c.UtcNow).Returns(new DateTimeOffset(2025, 10, 4, 18, 30, 0, TimeSpan.Zero));

        return new BackgroundTelemetryExporter(
            _clock.Object,
            _encryptor.Object,
            _secureQueue.Object);
    }

    [Fact]
    public async Task FlushAsync_EncryptsAndQueuesPendingEvents()
    {
        var exporter = CreateExporter();
        exporter.EnqueueResumeEvent();

        BackgroundTelemetryEnvelope? capturedEnvelope = null;
        var packet = new EncryptedTelemetryPacket(Guid.NewGuid(), Array.Empty<byte>(), "background");
        _encryptor
            .Setup(e => e.EncryptAsync(It.IsAny<BackgroundTelemetryEnvelope>(), It.IsAny<CancellationToken>()))
            .Callback<BackgroundTelemetryEnvelope, CancellationToken>((env, _) => capturedEnvelope = env)
            .ReturnsAsync(packet);

        await exporter.FlushAsync(CancellationToken.None);

        Assert.NotNull(capturedEnvelope);
        Assert.Equal("background", capturedEnvelope!.Mode);
        var evt = Assert.Single(capturedEnvelope.Events);
        Assert.Equal("backgroundResume", evt.EventType);

        _secureQueue.Verify(q => q.WriteAsync(packet, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FlushAsync_WhenNoEvents_DoesNotHitQueue()
    {
        var exporter = CreateExporter();

        await exporter.FlushAsync(CancellationToken.None);

        _encryptor.Verify(
            e => e.EncryptAsync(It.IsAny<BackgroundTelemetryEnvelope>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _secureQueue.Verify(
            q => q.WriteAsync(It.IsAny<EncryptedTelemetryPacket>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
