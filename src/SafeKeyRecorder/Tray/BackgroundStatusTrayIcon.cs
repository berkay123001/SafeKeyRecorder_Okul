using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using SafeKeyRecorder.Background;
using SafeKeyRecorder.ViewModels;

namespace SafeKeyRecorder.Tray;

public sealed class BackgroundStatusTrayIcon : IDisposable
{
    private readonly TrayIcon? _trayIcon;
    private readonly BackgroundStatusBannerViewModel _banner;
    private readonly WindowIcon _backgroundIcon;
    private readonly WindowIcon _foregroundIcon;
    private readonly WindowIcon _idleIcon;
    private readonly bool _isTrayAvailable;
    private bool _disposed;

    public BackgroundStatusTrayIcon(Window owner, BackgroundStatusBannerViewModel banner)
    {
        if (owner is null)
        {
            throw new ArgumentNullException(nameof(owner));
        }

        _banner = banner ?? throw new ArgumentNullException(nameof(banner));

        _backgroundIcon = CreateIcon(Color.FromRgb(249, 199, 79));
        _foregroundIcon = CreateIcon(Color.FromRgb(144, 238, 144));
        _idleIcon = CreateIcon(Color.FromRgb(160, 160, 160));

        _isTrayAvailable = IsTraySupported();

        if (!_isTrayAvailable)
        {
            Debug.WriteLine("[SafeKeyRecorder] Sistem tepsisi bu platformda devre dışı (TrayIcon desteklenmiyor).");
            return;
        }

        try
        {
            _trayIcon = new TrayIcon
            {
                Icon = _idleIcon,
                ToolTipText = "Kayıt pasif.",
                IsVisible = true
            };

            _trayIcon.Clicked += (_, _) =>
            {
                if (owner.WindowState == WindowState.Minimized)
                {
                    owner.WindowState = WindowState.Normal;
                }

                owner.Show();
                owner.Activate();
            };

        }
        catch (Exception ex)
        {
            _isTrayAvailable = false;
            _trayIcon = null;
            Debug.WriteLine($"[SafeKeyRecorder] Sistem tepsisi oluşturulamadı: {ex.Message}");
            return;
        }

        _banner.StateChanged += OnBannerStateChanged;

        UpdateTrayIcon();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _banner.StateChanged -= OnBannerStateChanged;

        if (_trayIcon is not null)
        {
            _trayIcon.IsVisible = false;
            _trayIcon.Dispose();
        }
    }

    private void OnBannerStateChanged(object? sender, EventArgs e) => UpdateTrayIcon();

    private void UpdateTrayIcon()
    {
        if (_disposed || !_isTrayAvailable || _trayIcon is null)
        {
            return;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            ApplyTrayState();
        }
        else
        {
            Dispatcher.UIThread.Post(ApplyTrayState);
        }
    }

    private void ApplyTrayState()
    {
        if (_disposed || !_isTrayAvailable || _trayIcon is null)
        {
            return;
        }

        var shouldShow = _banner.IsVisible;

        WindowIcon icon;
        string tooltip;

        switch (_banner.VisualState)
        {
            case BannerVisualState.Background:
                icon = _backgroundIcon;
                tooltip = "Arka plan kayıt modu aktif.";
                break;
            case BannerVisualState.Foreground:
                icon = _foregroundIcon;
                tooltip = "Arka plan modu kapalı. Odak içi kayıt sürüyor.";
                break;
            default:
                icon = _idleIcon;
                tooltip = "Kayıt pasif.";
                break;
        }

        if (shouldShow)
        {
            _trayIcon.Icon = icon;
            _trayIcon.ToolTipText = tooltip;
            _trayIcon.IsVisible = true;
        }
        else
        {
            _trayIcon.IsVisible = false;
        }
    }

    private static WindowIcon CreateIcon(Color color)
    {
        var pixelSize = new PixelSize(16, 16);
        var dpi = new Vector(96, 96);

        using var renderTarget = new RenderTargetBitmap(pixelSize, dpi);
        using (var context = renderTarget.CreateDrawingContext())
        {
            context.FillRectangle(new SolidColorBrush(color), new Rect(0, 0, pixelSize.Width, pixelSize.Height));
        }

        using var stream = new MemoryStream();
        renderTarget.Save(stream);
        stream.Position = 0;

        return new WindowIcon(stream);
    }

    private static bool IsTraySupported()
    {
        // Avalonia's TrayIcon currently has stable support on Windows and macOS.
        // Linux support depends on libappindicator / StatusNotifier availability and is unstable.

        if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS())
        {
            return true;
        }

        // Best-effort: disable on other platforms to avoid runtime crashes.
        return false;
    }
}
