using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SharpHook;
using SharpHook.Data;
using SharpHook.Native;
using SafeKeyRecorder.Services.Abstractions;

namespace SafeKeyRecorder.Services;

public sealed class GlobalHookAdapter : IGlobalHookAdapter, IDisposable
{
    private readonly IKeyCaptureSink _captureSink;
    private readonly object _syncRoot = new();
    private SimpleGlobalHook? _globalHook;
    private CancellationTokenSource? _hookCts;
    private Task? _hookTask;
    private bool _disposed;
    private KeyCode? _lastPrintableKeyCode;
    private DateTimeOffset _lastPrintableTimestamp;

    public GlobalHookAdapter(IKeyCaptureSink captureSink)
    {
        _captureSink = captureSink ?? throw new ArgumentNullException(nameof(captureSink));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        EnsureNotDisposed();

        lock (_syncRoot)
        {
            if (_globalHook is not null)
            {
                return Task.CompletedTask;
            }

            _hookCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _globalHook = new SimpleGlobalHook();
            _globalHook.KeyPressed += OnKeyPressed;
            _globalHook.KeyTyped += OnKeyTyped;

            _hookTask = Task.Run(() => RunHookAsync(_hookCts!.Token), CancellationToken.None);
        }

        return Task.CompletedTask;
    }

    public Task RestartAsync(CancellationToken cancellationToken)
    {
        EnsureNotDisposed();
        StopInternal(waitForCompletion: true);
        return StartAsync(cancellationToken);
    }

    public void Stop()
    {
        EnsureNotDisposed();
        StopInternal(waitForCompletion: true);
    }

    private void StopInternal(bool waitForCompletion)
    {
        EnsureNotDisposed();

        Task? hookTask = null;

        lock (_syncRoot)
        {
            if (_globalHook is null)
            {
                return;
            }

            _globalHook.KeyPressed -= OnKeyPressed;
            _globalHook.KeyTyped -= OnKeyTyped;
            _hookCts?.Cancel();
            _hookCts?.Dispose();
            _hookCts = null;
            hookTask = _hookTask;
            _hookTask = null;

            try
            {
                _globalHook.Stop();
            }
            catch (HookException ex)
            {
                Debug.WriteLine($"[SafeKeyRecorder] Global hook stop hatası yoksayıldı: {ex.Message}");
            }

            _globalHook.Dispose();
            _globalHook = null;
        }

        if (waitForCompletion && hookTask is not null)
        {
            try
            {
                hookTask.Wait(TimeSpan.FromSeconds(1));
            }
            catch (AggregateException ex) when (ex.InnerException is OperationCanceledException)
            {
                // Ignore expected cancellation
            }
            finally
            {
                hookTask.Dispose();
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        StopInternal(waitForCompletion: true);
    }

    private void OnKeyTyped(object? sender, KeyboardHookEventArgs e)
    {
        var data = e.Data;
        if (data.KeyChar == KeyboardEventData.UndefinedChar)
        {
            return;
        }

        QueueCapture(data.KeyChar.ToString(), true, GetModifiers(e.RawEvent.Mask));

        _lastPrintableKeyCode = data.KeyCode;
        _lastPrintableTimestamp = DateTimeOffset.UtcNow;
    }

    private void OnKeyPressed(object? sender, KeyboardHookEventArgs e)
    {
        var data = e.Data;
        var isPrintableChar = data.KeyChar != KeyboardEventData.UndefinedChar && !char.IsControl(data.KeyChar);

        if (isPrintableChar || IsRecentPrintableKey(data.KeyCode))
        {
            // KeyTyped event will handle printable keys to avoid duplicates
            return;
        }

        QueueCapture(data.KeyCode.ToString(), false, GetModifiers(e.RawEvent.Mask));
    }

    private void QueueCapture(string keySymbol, bool isPrintable, string[] modifiers)
    {
        if (string.IsNullOrEmpty(keySymbol))
        {
            return;
        }

        _ = Task.Run(() => _captureSink.CaptureAsync(keySymbol, isPrintable, modifiers, fromGlobalHook: true, cancellationToken: CancellationToken.None));
    }

    private static string[] GetModifiers(EventMask mask)
    {
        var modifiers = new List<string>(3);

        if (mask.HasCtrl())
        {
            modifiers.Add("Ctrl");
        }

        if (mask.HasAlt())
        {
            modifiers.Add("Alt");
        }

        if (mask.HasShift())
        {
            modifiers.Add("Shift");
        }

        return modifiers.ToArray();
    }

    private bool IsRecentPrintableKey(KeyCode keyCode)
    {
        if (_lastPrintableKeyCode is null)
        {
            return false;
        }

        if (_lastPrintableKeyCode.Value != keyCode)
        {
            return false;
        }

        return (DateTimeOffset.UtcNow - _lastPrintableTimestamp) <= TimeSpan.FromMilliseconds(200);
    }

    private void RunHookAsync(CancellationToken cancellationToken)
    {
        using var registration = cancellationToken.Register(() =>
        {
            lock (_syncRoot)
            {
                _globalHook?.Stop();
            }
        });

        try
        {
            _globalHook?.Run();
        }
        catch (HookException)
        {
            // TODO: telemetry/logging for hook failures
        }
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(GlobalHookAdapter));
        }
    }
}
