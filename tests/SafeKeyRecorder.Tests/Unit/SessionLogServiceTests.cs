using System;
using System.IO;
using System.Threading.Tasks;
using SafeKeyRecorder.Models;
using SafeKeyRecorder.Services;
using Xunit;

namespace SafeKeyRecorder.Tests.Unit;

public sealed class SessionLogServiceTests : IDisposable
{
    private readonly string _tempRoot;

    public SessionLogServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "SafeKeyRecorderTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    [Fact]
    public async Task Append_ShouldWriteEntryAndEmitAuditEvent()
    {
        using var service = new SessionLogService(_tempRoot, TimeSpan.FromHours(1));
        var session = new ConsentSession();
        session.GrantConsent(DateTimeOffset.UtcNow, true, Path.Combine(_tempRoot, "session_log.txt"));

        service.AttachSession(session);

        var entry = new SessionLogEntry(
            session.SessionId,
            DateTimeOffset.UtcNow,
            "A",
            isPrintable: true,
            modifiers: Array.Empty<string>(),
            wasLoggedToFile: false);

        string? appendedLine = null;
        service.LogAppended += (_, line) => appendedLine = line;

        await service.AppendAsync(entry);

        var expectedLine = $"{entry.RecordedAt:O}, {entry.KeySymbol}";
        var logPath = service.LogFilePath;

        Assert.True(File.Exists(logPath));
        var fileContents = await File.ReadAllTextAsync(logPath).ConfigureAwait(false);
        Assert.Equal(expectedLine, fileContents.TrimEnd());
        Assert.Equal(expectedLine, appendedLine);
    }

    [Fact]
    public async Task Purge_ShouldDeleteFileAndRaiseEvent()
    {
        using var service = new SessionLogService(_tempRoot, TimeSpan.FromHours(1));
        var session = new ConsentSession();
        session.GrantConsent(DateTimeOffset.UtcNow, true, Path.Combine(_tempRoot, "session_log.txt"));
        service.AttachSession(session);

        await File.WriteAllTextAsync(service.LogFilePath, "test").ConfigureAwait(false);

        var purgedRaised = false;
        service.LogPurged += (_, _) => purgedRaised = true;

        await service.PurgeAsync();

        Assert.False(File.Exists(service.LogFilePath));
        Assert.True(purgedRaised);
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
            // Ignore cleanup failures in tests.
        }
    }
}
