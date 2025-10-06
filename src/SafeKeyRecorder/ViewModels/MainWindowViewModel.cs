using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using SafeKeyRecorder.Background;
using SafeKeyRecorder.Models;
using SafeKeyRecorder.Services;
using SafeKeyRecorder.Telemetry;

namespace SafeKeyRecorder.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly SessionLogService _sessionLogService;
    private readonly KeyCaptureService _keyCaptureService;
    private readonly AccessibilityService _accessibilityService;
    private readonly ComplianceAuditLogger _auditLogger;
    private readonly Func<Task<ConsentDecision?>> _showConsentDialogAsync;
    private readonly BackgroundConsentCoordinator _consentCoordinator;
    private readonly BackgroundResumeCoordinator _resumeCoordinator;

    private ConsentSession? _currentSession;
    private string _consentStatus = "Rıza bekleniyor.";
    private string _loggingPathMessage = string.Empty;
    private bool _isLoggingPathVisible;
    private string _permissionSummary = string.Empty;
    private bool _isPermissionSummaryVisible;
    private bool _isRecording;
    private string _accessibilityStatus = "Varsayılan tema";
    private readonly BackgroundStatusBannerViewModel _backgroundBanner;

    public MainWindowViewModel(
        SessionLogService sessionLogService,
        KeyCaptureService keyCaptureService,
        AccessibilityService accessibilityService,
        ComplianceAuditLogger auditLogger,
        Func<Task<ConsentDecision?>> showConsentDialogAsync,
        BackgroundStatusBannerViewModel bannerViewModel,
        BackgroundConsentCoordinator consentCoordinator,
        BackgroundResumeCoordinator resumeCoordinator)
    {
        _sessionLogService = sessionLogService ?? throw new ArgumentNullException(nameof(sessionLogService));
        _keyCaptureService = keyCaptureService ?? throw new ArgumentNullException(nameof(keyCaptureService));
        _accessibilityService = accessibilityService ?? throw new ArgumentNullException(nameof(accessibilityService));
        _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
        _showConsentDialogAsync = showConsentDialogAsync ?? throw new ArgumentNullException(nameof(showConsentDialogAsync));
        _backgroundBanner = bannerViewModel ?? throw new ArgumentNullException(nameof(bannerViewModel));
        _consentCoordinator = consentCoordinator ?? throw new ArgumentNullException(nameof(consentCoordinator));
        _resumeCoordinator = resumeCoordinator ?? throw new ArgumentNullException(nameof(resumeCoordinator));

        LogEntries = new ObservableCollection<string>();

        StartConsentCommand = new RelayCommand(StartConsentFlowAsync);
        PurgeLogCommand = new RelayCommand(PurgeLogAsync, _ => _currentSession is not null);
        OpenConsentSettingsCommand = new RelayCommand(OpenConsentSettingsAsync, _ => _currentSession is not null);

        _keyCaptureService.KeyCaptured += OnKeyCaptured;
        _sessionLogService.LogAppended += OnLogAppended;
        _sessionLogService.LogPurged += OnLogPurged;
        _accessibilityService.PreferenceChanged += OnAccessibilityChanged;

        _backgroundBanner.ShowPassive("Kayıt pasif.");
    }

    public ObservableCollection<string> LogEntries { get; }

    public string ConsentStatus
    {
        get => _consentStatus;
        private set
        {
            if (_consentStatus == value)
            {
                return;
            }

            _consentStatus = value;
            RaisePropertyChanged();
        }
    }

    public string LoggingPathMessage
    {
        get => _loggingPathMessage;
        private set
        {
            if (_loggingPathMessage == value)
            {
                return;
            }

            _loggingPathMessage = value;
            RaisePropertyChanged();
        }
    }

    public bool IsLoggingPathVisible
    {
        get => _isLoggingPathVisible;
        private set
        {
            if (_isLoggingPathVisible == value)
            {
                return;
            }

            _isLoggingPathVisible = value;
            RaisePropertyChanged();
        }
    }

    public string PermissionSummary
    {
        get => _permissionSummary;
        private set
        {
            if (_permissionSummary == value)
            {
                return;
            }

            _permissionSummary = value;
            RaisePropertyChanged();
        }
    }

    public bool IsPermissionSummaryVisible
    {
        get => _isPermissionSummaryVisible;
        private set
        {
            if (_isPermissionSummaryVisible == value)
            {
                return;
            }

            _isPermissionSummaryVisible = value;
            RaisePropertyChanged();
        }
    }

    public BackgroundStatusBannerViewModel BackgroundBanner => _backgroundBanner;

    public bool IsRecording
    {
        get => _isRecording;
        private set
        {
            if (_isRecording == value)
            {
                return;
            }

            _isRecording = value;
            RaisePropertyChanged();
        }
    }

    public string AccessibilityStatus
    {
        get => _accessibilityStatus;
        private set
        {
            if (_accessibilityStatus == value)
            {
                return;
            }

            _accessibilityStatus = value;
            RaisePropertyChanged();
        }
    }

    public RelayCommand StartConsentCommand { get; }

    public RelayCommand PurgeLogCommand { get; }

    public RelayCommand OpenConsentSettingsCommand { get; }

    public async Task CaptureKeyAsync(string keySymbol, bool isPrintable, string[] modifiers)
    {
        if (!IsRecording)
        {
            return;
        }

        await _keyCaptureService.CaptureFromForegroundAsync(keySymbol, isPrintable, modifiers).ConfigureAwait(false);
    }

    private async Task StartConsentFlowAsync(object? parameter)
    {
        _backgroundBanner.ShowPassive("Rıza diyaloğu açılıyor...");

        var decision = await _showConsentDialogAsync().ConfigureAwait(false);

        if (decision is null)
        {
            _backgroundBanner.ShowPassive("Rıza diyaloğu kapatıldı. Oturum başlatılmadı.");
            return;
        }

        await ApplyConsentDecisionAsync(decision).ConfigureAwait(false);
    }

    private async Task PurgeLogAsync(object? parameter)
    {
        await _sessionLogService.PurgeAsync().ConfigureAwait(false);

        LogEntries.Clear();
        _backgroundBanner.ShowPassive("Log dosyası manuel olarak temizlendi.");

        if (_currentSession is not null)
        {
            _auditLogger.LogPurge(_currentSession.SessionId, DateTimeOffset.UtcNow, dueToAutoDelete: false);
        }
    }

    private async Task ApplyConsentDecisionAsync(ConsentDecision decision)
    {
        if (!decision.Accepted)
        {
            var sessionId = _currentSession?.SessionId ?? Guid.NewGuid();
            _auditLogger.LogConsentDeclined(sessionId, decision.DecidedAt);

            ConsentStatus = "Rıza reddedildi.";
            _backgroundBanner.ShowPassive("Kullanıcı rızayı reddetti. Kayıt yapılmayacak.");
            IsRecording = false;
            _currentSession = null;
            PurgeLogCommand.RaiseCanExecuteChanged();
            OpenConsentSettingsCommand.RaiseCanExecuteChanged();
            IsPermissionSummaryVisible = false;
            PermissionSummary = string.Empty;
            _resumeCoordinator.DisableBackgroundCapture();
            return;
        }

        var logFilePath = _sessionLogService.LogFilePath;
        var session = new ConsentSession();
        session.GrantConsent(decision.DecidedAt, decision.LoggingEnabled, logFilePath);
        session.SetAutoDelete(decision.AutoDeleteRequested);

        _sessionLogService.AttachSession(session);
        _keyCaptureService.AttachSession(session);

        _auditLogger.LogConsentGranted(session);
        _auditLogger.LogRetentionDecision(session);

        _currentSession = session;
        ConsentStatus = "Rıza alındı. İzinler kaydedildi.";
        IsRecording = true;
        PurgeLogCommand.RaiseCanExecuteChanged();
        OpenConsentSettingsCommand.RaiseCanExecuteChanged();

        LogEntries.Clear();

        if (decision.LoggingEnabled)
        {
            LoggingPathMessage = $"Log dosyası: {session.LogFilePath}";
        }
        else
        {
            LoggingPathMessage = "Dosya kaydı kapalı.";
        }

        IsLoggingPathVisible = true;
        PermissionSummary = BuildPermissionSummary(decision);
        IsPermissionSummaryVisible = true;

        _backgroundBanner.ShowPassive(
            decision.LoggingEnabled
                ? "Tuş kaydı aktif – dosya kaydı açık."
                : "Tuş kaydı aktif – dosya kaydı kapalı.");

        await _consentCoordinator.ApplyDecisionAsync(decision, CancellationToken.None).ConfigureAwait(false);

        if (decision.AllowBackgroundCapture)
        {
            _resumeCoordinator.EnableBackgroundCapture();
        }
        else
        {
            _resumeCoordinator.DisableBackgroundCapture();
        }
    }

    private void OnKeyCaptured(object? sender, SessionLogEntry entry)
    {
        if (entry.WasLoggedToFile)
        {
            // LogAppended event will render this entry.
            return;
        }

        void AddEntry()
        {
            var humanReadable = FormatEntry(entry.RecordedAt, entry.KeySymbol, entry.Modifiers);
            LogEntries.Add(humanReadable);
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            AddEntry();
        }
        else
        {
            Dispatcher.UIThread.Post(AddEntry);
        }
    }

    private void OnLogAppended(object? sender, string line)
    {
        void UpdateLog()
        {
            var formatted = TryFormatLogLine(line);
            if (formatted is null)
            {
                return;
            }

            LogEntries.Add(formatted);
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            UpdateLog();
        }
        else
        {
            Dispatcher.UIThread.Post(UpdateLog);
        }
    }

    private void OnLogPurged(object? sender, EventArgs e)
    {
        LogEntries.Clear();
        _backgroundBanner.ShowPassive("Log dosyası temizlendi.");
    }

    private void OnAccessibilityChanged(object? sender, AccessibilityPreference preference)
    {
        var states = preference switch
        {
            { HighContrastEnabled: true, ScreenReaderEnabled: true } => "Yüksek kontrast + ekran okuyucu",
            { HighContrastEnabled: true } => "Yüksek kontrast modu",
            { ScreenReaderEnabled: true } => "Ekran okuyucu etiketleri",
            _ => "Varsayılan tema"
        };

        AccessibilityStatus = states;
    }

    private static string? TryFormatLogLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return null;
        }

        var separatorIndex = line.IndexOf(',');
        if (separatorIndex < 0)
        {
            return null;
        }

        var timestampPart = line[..separatorIndex].Trim();
        var keyPart = line[(separatorIndex + 1)..].Trim();

        if (!DateTimeOffset.TryParse(timestampPart, out var recordedAt))
        {
            recordedAt = DateTimeOffset.UtcNow;
        }

        return FormatEntry(recordedAt, keyPart, Array.Empty<string>());
    }

    private static string FormatEntry(DateTimeOffset recordedAt, string keySymbol, IReadOnlyList<string>? modifiers)
    {
        var modifierText = modifiers is { Count: > 0 }
            ? $"[{string.Join('+', modifiers)}] "
            : string.Empty;

        return $"{recordedAt:HH:mm:ss.fff} {modifierText}{keySymbol}".Trim();
    }

    public void Dispose()
    {
        _keyCaptureService.KeyCaptured -= OnKeyCaptured;
        _sessionLogService.LogAppended -= OnLogAppended;
        _sessionLogService.LogPurged -= OnLogPurged;
        _accessibilityService.PreferenceChanged -= OnAccessibilityChanged;

        if (_currentSession?.AutoDeleteRequested == true)
        {
            _sessionLogService.PurgeAsync().GetAwaiter().GetResult();
        }

        _sessionLogService.Dispose();
        _resumeCoordinator.Dispose();
    }

    private Task OpenConsentSettingsAsync(object? parameter) => StartConsentFlowAsync(parameter);
    private static string BuildPermissionSummary(ConsentDecision decision)
    {
        var background = decision.AllowBackgroundCapture ? "Arka plan: Açık" : "Arka plan: Kapalı";
        var logging = decision.LoggingEnabled ? "Dosya kaydı: Açık" : "Dosya kaydı: Kapalı";
        var retention = decision.AutoDeleteRequested ? "Otomatik silme: Açık" : "Otomatik silme: Kapalı";

        return $"İzinler → {background} · {logging} · {retention}";
    }
}
