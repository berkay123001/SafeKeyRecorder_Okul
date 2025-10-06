using System;
using System.IO;
using System.Threading.Tasks;
using SafeKeyRecorder.Models;
using SafeKeyRecorder.Services;
using Xunit;

namespace SafeKeyRecorder.Tests.Unit;

public sealed class KeyCaptureServiceTests : IDisposable
{
    private readonly string _tempRoot;

    public KeyCaptureServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "SafeKeyRecorderTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    [Fact]
    public async Task Capture_ShouldEmitEntryAndAppendToLog()
    {
        using var sessionLogService = new SessionLogService(_tempRoot, TimeSpan.FromHours(1));
        var captureService = new KeyCaptureService(sessionLogService);
        var session = new ConsentSession();
        session.GrantConsent(DateTimeOffset.UtcNow, true, Path.Combine(_tempRoot, "session_log.txt"));

        captureService.AttachSession(session);

        SessionLogEntry? observedEntry = null;
        captureService.KeyCaptured += (_, entry) => observedEntry = entry;

        await captureService.CaptureAsync("A", isPrintable: true, new[] { "Shift" }).ConfigureAwait(false);

        Assert.NotNull(observedEntry);
        Assert.Equal(session.SessionId, observedEntry!.SessionId);
        Assert.Equal("A", observedEntry.KeySymbol);
        Assert.True(observedEntry.IsPrintable);
        Assert.Contains("Shift", observedEntry.Modifiers);

        var logContents = await File.ReadAllTextAsync(sessionLogService.LogFilePath).ConfigureAwait(false);
        Assert.Contains("A", logContents, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Capture_ShouldThrowIfConsentNotGranted()
    {
        using var sessionLogService = new SessionLogService(_tempRoot, TimeSpan.FromHours(1));
        var captureService = new KeyCaptureService(sessionLogService);
        var session = new ConsentSession();
        captureService.AttachSession(session);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => captureService.CaptureAsync("A", isPrintable: true)).ConfigureAwait(false);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempRoot))
            {
                Directory.Delete(_tempRoot, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup race during test teardown.
        }
    }
}
