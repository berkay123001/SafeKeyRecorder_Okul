using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SafeKeyRecorder.Models;
using SafeKeyRecorder.Services.Abstractions;

namespace SafeKeyRecorder.Services;

public sealed class KeyCaptureService : IKeyCaptureSink
{
    private readonly SessionLogService _sessionLogService;
    private ConsentSession? _session;

    public event EventHandler<SessionLogEntry>? KeyCaptured;

    public KeyCaptureService(SessionLogService sessionLogService)
    {
        _sessionLogService = sessionLogService ?? throw new ArgumentNullException(nameof(sessionLogService));
    }

    public void AttachSession(ConsentSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _sessionLogService.AttachSession(session);
    }

    public async Task CaptureAsync(
        string keySymbol,
        bool isPrintable,
        string[]? modifiers = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keySymbol))
        {
            throw new ArgumentException("Key symbol must be provided.", nameof(keySymbol));
        }

        if (_session is null)
        {
            throw new InvalidOperationException("No consent session is attached.");
        }

        if (_session.ConsentGrantedAt is null)
        {
            throw new InvalidOperationException("Consent has not been granted.");
        }

        var normalizedModifiers = modifiers is null ? Array.Empty<string>() : modifiers.ToArray();
        var recordedAt = DateTimeOffset.UtcNow;
        var entry = new SessionLogEntry(
            _session.SessionId,
            recordedAt,
            keySymbol,
            isPrintable,
            normalizedModifiers,
            _session.LoggingEnabled);

        KeyCaptured?.Invoke(this, entry);

        await _sessionLogService.AppendAsync(entry, cancellationToken).ConfigureAwait(false);
    }
}
