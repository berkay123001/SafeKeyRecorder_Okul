using System.Threading;
using System.Threading.Tasks;

namespace SafeKeyRecorder.Services.Abstractions;

public interface IBackgroundCaptureService
{
    Task StartAsync(bool allowBackgroundCapture, CancellationToken cancellationToken);

    void Stop();

    void OnSystemLock();

    void OnSystemUnlock();
}
