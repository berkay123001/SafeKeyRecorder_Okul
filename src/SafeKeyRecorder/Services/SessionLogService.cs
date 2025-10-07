using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SafeKeyRecorder.Models;

namespace SafeKeyRecorder.Services;

public sealed class SessionLogService : IDisposable
{
    private readonly string _rootPath;
    private readonly TimeSpan _retentionWindow;
    private readonly object _sync = new();

    private ConsentSession? _session;
    private Timer? _retentionTimer;

    public event EventHandler<string>? LogAppended;
    public event EventHandler? LogPurged;

    public SessionLogService(string? rootPath = null, TimeSpan? retentionWindow = null)
    {
        _rootPath = rootPath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "SafeKeyRecorder");
        _retentionWindow = retentionWindow ?? TimeSpan.FromHours(24);
    }

    public string LogFilePath => _session?.LogFilePath ?? Path.Combine(_rootPath, "session_log.txt");

    public void AttachSession(ConsentSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));

        if (_session.LoggingEnabled)
        {
            EnsureStorage(_session.RetentionExpiresAt ?? DateTimeOffset.UtcNow);
        }
    }

    public async Task AppendAsync(SessionLogEntry entry, CancellationToken cancellationToken = default)
    {
        if (_session is null)
        {
            throw new InvalidOperationException("Session has not been attached to the log service.");
        }

        if (!_session.LoggingEnabled)
        {
            return;
        }

        EnsureStorage(entry.RecordedAt);

        var modifiers = entry.Modifiers is { Length: > 0 }
            ? string.Join('+', entry.Modifiers)
            : string.Empty;
        var printableFlag = entry.IsPrintable ? "1" : "0";
        var line = $"{entry.RecordedAt:O}|{entry.KeySymbol}|{printableFlag}|{modifiers}";
        var logPath = LogFilePath;

        await WriteLineAsync(logPath, line, cancellationToken).ConfigureAwait(false);

        LogAppended?.Invoke(this, line);
    }

    public void OverrideLogPath(string newPath)
    {
        if (string.IsNullOrWhiteSpace(newPath))
        {
            throw new ArgumentException("Yeni log dosyası yolu belirtilmelidir.", nameof(newPath));
        }

        if (_session is null)
        {
            throw new InvalidOperationException("Log yolu güncellenmeden önce oturum bağlanmalıdır.");
        }

        var directory = Path.GetDirectoryName(newPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _session.UpdateLogging(true, newPath, DateTimeOffset.UtcNow);
    }

    public Task PurgeAsync(CancellationToken cancellationToken = default)
    {
        var logPath = LogFilePath;

        lock (_sync)
        {
            _retentionTimer?.Dispose();
            _retentionTimer = null;
        }

        if (File.Exists(logPath))
        {
            File.Delete(logPath);
        }

        LogPurged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public void ScheduleAutomaticPurge(DateTimeOffset expiry)
    {
        var dueTime = expiry - DateTimeOffset.UtcNow;
        if (dueTime <= TimeSpan.Zero)
        {
            _ = PurgeAsync();
            return;
        }

        lock (_sync)
        {
            _retentionTimer?.Dispose();
            _retentionTimer = new Timer(_ => _ = PurgeAsync(), null, dueTime, Timeout.InfiniteTimeSpan);
        }
    }

    private void EnsureStorage(DateTimeOffset referenceTime)
    {
        Directory.CreateDirectory(_rootPath);

        if (_session is null)
        {
            return;
        }

        if (_session.LogFilePath is null)
        {
            _session.UpdateLogging(true, LogFilePath, referenceTime);
        }

        var expiry = _session.RetentionExpiresAt ?? referenceTime.Add(_retentionWindow);
        ScheduleAutomaticPurge(expiry);
    }

    private static async Task WriteLineAsync(string path, string line, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
        await using var writer = new StreamWriter(stream);
        await writer.WriteLineAsync(line.AsMemory(), cancellationToken).ConfigureAwait(false);
    }

    public void Dispose()
    {
        lock (_sync)
        {
            _retentionTimer?.Dispose();
            _retentionTimer = null;
        }
    }
}
