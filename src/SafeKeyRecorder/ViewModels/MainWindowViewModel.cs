using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using SafeKeyRecorder.Background;
using SafeKeyRecorder.Configuration;
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
    private readonly WebhookUploadService _webhookUploadService;
    private readonly WebhookOptions _webhookOptions;
    private WebhookConsentRecord? _webhookConsent;

    private bool _backgroundCaptureEnabled;

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
        BackgroundResumeCoordinator resumeCoordinator,
        WebhookUploadService webhookUploadService,
        WebhookOptions webhookOptions)
    {
        _sessionLogService = sessionLogService ?? throw new ArgumentNullException(nameof(sessionLogService));
        _keyCaptureService = keyCaptureService ?? throw new ArgumentNullException(nameof(keyCaptureService));
        _accessibilityService = accessibilityService ?? throw new ArgumentNullException(nameof(accessibilityService));
        _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
        _showConsentDialogAsync = showConsentDialogAsync ?? throw new ArgumentNullException(nameof(showConsentDialogAsync));
        _backgroundBanner = bannerViewModel ?? throw new ArgumentNullException(nameof(bannerViewModel));
        _consentCoordinator = consentCoordinator ?? throw new ArgumentNullException(nameof(consentCoordinator));
        _resumeCoordinator = resumeCoordinator ?? throw new ArgumentNullException(nameof(resumeCoordinator));
        _webhookUploadService = webhookUploadService ?? throw new ArgumentNullException(nameof(webhookUploadService));
        _webhookOptions = webhookOptions ?? throw new ArgumentNullException(nameof(webhookOptions));

        LogEntries = new ObservableCollection<string>();

        StartConsentCommand = new RelayCommand(StartConsentFlowAsync);
        PurgeLogCommand = new RelayCommand(PurgeLogAsync, _ => _currentSession is not null);
        OpenConsentSettingsCommand = new RelayCommand(OpenConsentSettingsAsync, _ => _currentSession is not null);
        ExportLogCommand = new RelayCommand(ExportLogAsync, _ => CanExportLog());
        BrowseLogPathCommand = new RelayCommand(BrowseLogPathAsync);

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

    public RelayCommand ExportLogCommand { get; }

    public RelayCommand BrowseLogPathCommand { get; }

    public string WebhookEndpoint
    {
        get => _webhookOptions.Endpoint;
        set
        {
            var trimmed = value?.Trim() ?? string.Empty;
            if (string.Equals(_webhookOptions.Endpoint, trimmed, StringComparison.Ordinal))
            {
                return;
            }

            _webhookOptions.Endpoint = trimmed;
            RaisePropertyChanged();
            ExportLogCommand.RaiseCanExecuteChanged();
        }
    }

    public async Task CaptureKeyAsync(string keySymbol, bool isPrintable, string[] modifiers)
    {
        if (!IsRecording)
        {
            return;
        }

        if (_backgroundCaptureEnabled)
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
            ExportLogCommand.RaiseCanExecuteChanged();
            IsPermissionSummaryVisible = false;
            PermissionSummary = string.Empty;
            _resumeCoordinator.DisableBackgroundCapture();
            _backgroundCaptureEnabled = false;
            _webhookConsent = null;
            return;
        }

        var logFilePath = GetLogFilePath();
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
        ExportLogCommand.RaiseCanExecuteChanged();

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

        _webhookConsent = new WebhookConsentRecord(session.SessionId, decision.DecidedAt, decision.Accepted && decision.AllowWebhookUpload);
        ExportLogCommand.RaiseCanExecuteChanged();

        if (decision.AllowBackgroundCapture)
        {
            _resumeCoordinator.EnableBackgroundCapture();
            _backgroundCaptureEnabled = true;
        }
        else
        {
            _resumeCoordinator.DisableBackgroundCapture();
            _backgroundCaptureEnabled = false;
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
            var humanReadable = FormatEntry(entry.RecordedAt, entry.KeySymbol, entry.Modifiers, entry.IsPrintable);
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

    private async Task BrowseLogPathAsync(object? parameter)
    {
        if (_currentSession is null)
        {
            _backgroundBanner.ShowPassive("Önce rıza alın ve oturumu başlatın.");
            return;
        }

        if (!_currentSession.LoggingEnabled)
        {
            _backgroundBanner.ShowPassive("Dosya kaydı kapalıyken konum değiştirilemez.");
            return;
        }

        var dialog = new SaveFileDialog
        {
            Title = "Log dosyası konumu",
            InitialFileName = "session_log.txt",
            Filters = new List<FileDialogFilter>
            {
                new()
                {
                    Name = "Metin dosyaları",
                    Extensions = { "txt" }
                }
            }
        };

        var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;

        if (window is null)
        {
            _backgroundBanner.ShowPassive("Dosya seçici açılamadı.");
            return;
        }

        var result = await dialog.ShowAsync(window).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(result))
        {
            return;
        }

        _sessionLogService.OverrideLogPath(result);
        _backgroundBanner.ShowPassive($"Log dosyası konumu güncellendi: {result}");

        LoggingPathMessage = $"Log dosyası: {result}";
        ExportLogCommand.RaiseCanExecuteChanged();
    }

    private string GetLogFilePath()
    {
        if (_currentSession?.LogFilePath is { Length: > 0 } path)
        {
            return path;
        }

        return _sessionLogService.LogFilePath;
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

        var parts = line.Split('|');
        if (parts.Length < 4)
        {
            return null;
        }

        var timestampPart = parts[0];
        var keyPart = parts[1];
        var printablePart = parts[2];
        var modifiersPart = parts[3];

        if (!DateTimeOffset.TryParse(timestampPart, out var recordedAt))
        {
            recordedAt = DateTimeOffset.UtcNow;
        }

        var isPrintable = printablePart == "1";
        var modifiers = string.IsNullOrWhiteSpace(modifiersPart)
            ? Array.Empty<string>()
            : modifiersPart.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return FormatEntry(recordedAt, keyPart, modifiers, isPrintable);
    }

    private static string FormatEntry(DateTimeOffset recordedAt, string keySymbol, IReadOnlyList<string>? modifiers, bool isPrintable)
    {
        var modifierText = modifiers is { Count: > 0 }
            ? $"[{string.Join('+', modifiers)}] "
            : string.Empty;

        var normalizedSymbol = isPrintable && keySymbol.Length == 1
            ? keySymbol
            : keySymbol.Replace("KeyCode.", string.Empty);

        if (string.Equals(normalizedSymbol, "Space", StringComparison.OrdinalIgnoreCase))
        {
            normalizedSymbol = "␣";
        }

        return $"{recordedAt:HH:mm:ss.fff} {modifierText}{normalizedSymbol}".Trim();
    }

    private bool CanExportLog()
    {
        if (_currentSession is null)
        {
            return false;
        }

        if (_webhookConsent is null)
        {
            return false;
        }

        if (!_webhookConsent.CanUpload)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(_webhookOptions.Endpoint))
        {
            return false;
        }

        return true;
    }

    private async Task ExportLogAsync(object? parameter)
    {
        if (!CanExportLog())
        {
            _backgroundBanner.ShowPassive("Webhook paylaşım izni yok veya log dosyası bulunamadı.");
            return;
        }

        if (!Uri.TryCreate(_webhookOptions.Endpoint, UriKind.Absolute, out _))
        {
            _backgroundBanner.ShowPassive("Geçerli bir webhook adresi girin.");
            return;
        }

        string payload;
        try
        {
            payload = await File.ReadAllTextAsync(_sessionLogService.LogFilePath).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _backgroundBanner.ShowPassive($"Log okunamadı: {ex.Message}");
            return;
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            _backgroundBanner.ShowPassive("Log dosyası boş. Gönderim yapılmadı.");
            return;
        }

        try
        {
            _backgroundBanner.ShowPassive("Webhook'a gönderiliyor...");
            await _webhookUploadService.UploadAsync(_webhookConsent!, payload, CancellationToken.None).ConfigureAwait(false);
            _backgroundBanner.ShowPassive("Log dışa aktarıldı.");
        }
        catch (Exception ex)
        {
            _backgroundBanner.ShowPassive($"Webhook gönderimi başarısız: {ex.Message}");
        }
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
        var webhook = decision.AllowWebhookUpload ? "Webhook paylaşımı: Açık" : "Webhook paylaşımı: Kapalı";

        return $"İzinler → {background} · {logging} · {retention} · {webhook}";
    }
}
