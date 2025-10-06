using System;
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
    private readonly TrayIcon _trayIcon;
    private readonly BackgroundStatusBannerViewModel _banner;
    private readonly WindowIcon _backgroundIcon;
    private readonly WindowIcon _foregroundIcon;
    private readonly WindowIcon _idleIcon;
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

        _banner.StateChanged += OnBannerStateChanged;
        _banner.ToggleChanged += OnBannerToggleChanged;

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
        _banner.ToggleChanged -= OnBannerToggleChanged;

        _trayIcon.IsVisible = false;
        _trayIcon.Dispose();
    }

    private void OnBannerStateChanged(object? sender, EventArgs e) => UpdateTrayIcon();

    private void OnBannerToggleChanged(object? sender, bool _) => UpdateTrayIcon();

    private void UpdateTrayIcon()
    {
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
        _trayIcon.IsVisible = _banner.IsVisible;

        switch (_banner.VisualState)
        {
            case BannerVisualState.Background:
                _trayIcon.Icon = _backgroundIcon;
                _trayIcon.ToolTipText = "Arka plan kayıt modu aktif.";
                break;
            case BannerVisualState.Foreground:
                _trayIcon.Icon = _foregroundIcon;
                _trayIcon.ToolTipText = "Arka plan modu kapalı. Odak içi kayıt sürüyor.";
                break;
            default:
                _trayIcon.Icon = _idleIcon;
                _trayIcon.ToolTipText = "Kayıt pasif.";
                break;
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
}
