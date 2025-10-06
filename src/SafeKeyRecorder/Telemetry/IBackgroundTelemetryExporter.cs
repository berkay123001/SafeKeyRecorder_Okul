using System.Threading;
using System.Threading.Tasks;
using SafeKeyRecorder.Models;

namespace SafeKeyRecorder.Telemetry;

public interface IBackgroundTelemetryExporter
{
    void EnqueueConsentGranted(ConsentDecision decision);

    void EnqueuePauseEvent();

    void EnqueueResumeEvent();

    void EnqueueToggleChanged(bool enabled);

    Task FlushAsync(CancellationToken cancellationToken);
}
