using System.Threading;
using System.Threading.Tasks;

namespace SafeKeyRecorder.Services.Abstractions;

public interface IKeyCaptureSink
{
    Task CaptureAsync(string keySymbol, bool isPrintable, string[]? modifiers = null, CancellationToken cancellationToken = default);
}
