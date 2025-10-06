using System.Threading;
using System.Threading.Tasks;

namespace SafeKeyRecorder.Services.Abstractions;

public interface IGlobalHookAdapter
{
    Task StartAsync(CancellationToken cancellationToken);

    Task RestartAsync(CancellationToken cancellationToken);

    void Stop();
}
