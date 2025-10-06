using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using SafeKeyRecorder.Background;
using SafeKeyRecorder.Models;
using SafeKeyRecorder.Services;
using SafeKeyRecorder.Telemetry;
using SafeKeyRecorder.ViewModels;
using SafeKeyRecorder.Views;
using SafeKeyRecorder.Tray;

namespace SafeKeyRecorder;

public partial class MainWindow : Window
{
    private readonly SessionLogService _sessionLogService;
    private readonly KeyCaptureService _keyCaptureService;
    private readonly AccessibilityService _accessibilityService;
    private readonly ComplianceAuditLogger _auditLogger;
    private readonly BackgroundStatusBannerViewModel _bannerViewModel;
    private readonly BackgroundConsentCoordinator _consentCoordinator;
    private readonly BackgroundResumeCoordinator _resumeCoordinator;
    private readonly BackgroundCaptureService _captureService;
    private readonly SystemLockMonitor _lockMonitor;
    private readonly BackgroundTelemetryExporter _telemetryExporter;
    private readonly MainWindowViewModel _viewModel;
    private BackgroundStatusTrayIcon? _trayIcon;

    public MainWindow()
    {
        InitializeComponent();

        _sessionLogService = new SessionLogService();
        _keyCaptureService = new KeyCaptureService(_sessionLogService);
        _accessibilityService = new AccessibilityService();
        _auditLogger = new ComplianceAuditLogger();
        _bannerViewModel = new BackgroundStatusBannerViewModel();

        _telemetryExporter = new BackgroundTelemetryExporter(new SystemClock(), new BackgroundTelemetryEncryptor(), new SecureTelemetryQueue());
        _lockMonitor = new SystemLockMonitor();
        _captureService = new BackgroundCaptureService(new GlobalHookAdapter(_keyCaptureService), _lockMonitor, _telemetryExporter);

        _consentCoordinator = new BackgroundConsentCoordinator(_captureService, _telemetryExporter, _auditLogger, _bannerViewModel);
        _resumeCoordinator = new BackgroundResumeCoordinator(_captureService, _telemetryExporter, _lockMonitor);

        _viewModel = new MainWindowViewModel(
            _sessionLogService,
            _keyCaptureService,
            _accessibilityService,
            _auditLogger,
            ShowConsentDialogAsync,
            _bannerViewModel,
            _consentCoordinator,
            _resumeCoordinator);

        DataContext = _viewModel;
        this.KeyDown += OnKeyDown;

        _trayIcon = new BackgroundStatusTrayIcon(this, _viewModel.BackgroundBanner);
    }

    private async Task<ConsentDecision?> ShowConsentDialogAsync()
    {
        var dialog = new ConsentDialog();
        var tcs = new TaskCompletionSource<ConsentDecision?>();

        var viewModel = new ConsentDialogViewModel(decision =>
        {
            if (!tcs.Task.IsCompleted)
            {
                tcs.TrySetResult(decision);
            }
        });

        dialog.DataContext = viewModel;

        var result = await dialog.ShowDialog<bool>(this).ConfigureAwait(false);

        if (!result && !tcs.Task.IsCompleted)
        {
            tcs.TrySetResult(null);
        }

        return await tcs.Task.ConfigureAwait(false);
    }

    private async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        var keySymbol = e.Key.ToString();
        var isPrintable = keySymbol.Length == 1;
        var modifiers = new[]
        {
            e.KeyModifiers.HasFlag(KeyModifiers.Control) ? "Ctrl" : null,
            e.KeyModifiers.HasFlag(KeyModifiers.Alt) ? "Alt" : null,
            e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? "Shift" : null,
        };

        var filteredModifiers = Array.FindAll(modifiers, m => m is not null)!;
        await vm.CaptureKeyAsync(keySymbol, isPrintable, filteredModifiers!).ConfigureAwait(false);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        _trayIcon?.Dispose();
        _trayIcon = null;

        _resumeCoordinator.Dispose();
        _captureService.Dispose();
        _viewModel.Dispose();
        _sessionLogService.Dispose();
    }
}