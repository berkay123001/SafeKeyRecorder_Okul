using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SafeKeyRecorder.Models;
using SafeKeyRecorder.Telemetry.Abstractions;
using SafeKeyRecorder.Telemetry.Models;

namespace SafeKeyRecorder.Telemetry;

public sealed class BackgroundTelemetryExporter : IBackgroundTelemetryExporter
{
    private readonly IClock _clock;
    private readonly IBackgroundTelemetryEncryptor _encryptor;
    private readonly ISecureTelemetryQueue _secureQueue;
    private readonly List<BackgroundTelemetryEvent> _pendingEvents = new();
    private readonly object _syncRoot = new();
    private Guid _consentDecisionId = Guid.Empty;

    public BackgroundTelemetryExporter(
        IClock clock,
        IBackgroundTelemetryEncryptor encryptor,
        ISecureTelemetryQueue secureQueue)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
        _secureQueue = secureQueue ?? throw new ArgumentNullException(nameof(secureQueue));
    }

    public void EnqueueConsentGranted(ConsentDecision decision)
    {
        if (decision is null)
        {
            throw new ArgumentNullException(nameof(decision));
        }

        var evt = CreateEvent("backgroundConsentRecorded");

        lock (_syncRoot)
        {
            _consentDecisionId = decision.ConsentDecisionId;
            _pendingEvents.Add(evt);
        }
    }

    public void EnqueuePauseEvent()
    {
        AddEvent("backgroundPause");
    }

    public void EnqueueResumeEvent()
    {
        AddEvent("backgroundResume");
    }

    public void EnqueueToggleChanged(bool enabled)
    {
        AddEvent("backgroundToggleChanged", enabled ? "Enabled" : "Disabled");
    }

    public async Task FlushAsync(CancellationToken cancellationToken)
    {
        BackgroundTelemetryEnvelope? envelope;

        lock (_syncRoot)
        {
            if (_pendingEvents.Count == 0)
            {
                return;
            }

            envelope = new BackgroundTelemetryEnvelope(
                _consentDecisionId,
                mode: "background",
                queuedAtUtc: _clock.UtcNow);

            foreach (var evt in _pendingEvents)
            {
                evt.QueueState = "Queued";
                envelope.Events.Add(evt);
            }

            _pendingEvents.Clear();
        }

        envelope.FlushedAtUtc = _clock.UtcNow;

        var packet = await _encryptor.EncryptAsync(envelope, cancellationToken).ConfigureAwait(false);

        foreach (var evt in envelope.Events)
        {
            evt.QueueState = "Flushed";
        }

        await _secureQueue.WriteAsync(packet, cancellationToken).ConfigureAwait(false);
    }

    private void AddEvent(string eventType, string? systemState = null)
    {
        var evt = CreateEvent(eventType, systemState);

        lock (_syncRoot)
        {
            _pendingEvents.Add(evt);
        }
    }

    private BackgroundTelemetryEvent CreateEvent(string eventType, string? systemState = null)
    {
        return new BackgroundTelemetryEvent(
            Guid.NewGuid(),
            eventType,
            _clock.UtcNow,
            systemState);
    }
}
