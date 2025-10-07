using System;
using SafeKeyRecorder.Models;

namespace SafeKeyRecorder.ViewModels;

public sealed class ConsentDialogViewModel : ViewModelBase
{
    private const string DefaultConsentMessage =
        "Bu uygulama bir keylogger simülasyonudur. Devam ederek tuş vuruşlarının yalnızca bu pencere içinde ve eğitim amaçlı kaydedilmesini kabul ediyorsunuz.";

    private const string BackgroundConsentNotice =
        "Arka plan modu, uygulama odakta olmasa bile tuş vuruşlarını kaydeder. Bu seçenek yalnızca bilgilendirilmiş rıza ile etkinleştirilebilir ve banner/tray uyarıları sürekli görünür kalır.";

    private readonly Action<ConsentDecision>? _onDecision;
    private bool _isLoggingEnabled;
    private bool _allowBackgroundCapture;
    private bool _isAutoDeleteRequested = true;
    private bool _allowWebhookUpload;
    private bool _isDecisionMade;

    public ConsentDialogViewModel(Action<ConsentDecision>? onDecision = null, string? consentMessage = null)
    {
        _onDecision = onDecision;
        ConsentMessage = consentMessage ?? DefaultConsentMessage;

        AcceptCommand = new RelayCommand(_ => CompleteDecision(true), _ => !_isDecisionMade);
        DeclineCommand = new RelayCommand(_ => CompleteDecision(false), _ => !_isDecisionMade);
    }

    public string ConsentMessage { get; }

    public string BackgroundConsentExplanation => BackgroundConsentNotice;

    public bool IsLoggingEnabled
    {
        get => _isLoggingEnabled;
        set
        {
            if (_isLoggingEnabled == value)
            {
                return;
            }

            _isLoggingEnabled = value;
            RaisePropertyChanged();
        }
    }

    public bool AllowBackgroundCapture
    {
        get => _allowBackgroundCapture;
        set
        {
            if (_allowBackgroundCapture == value)
            {
                return;
            }

            _allowBackgroundCapture = value;
            RaisePropertyChanged();
        }
    }

    public bool AllowWebhookUpload
    {
        get => _allowWebhookUpload;
        set
        {
            if (_allowWebhookUpload == value)
            {
                return;
            }

            _allowWebhookUpload = value;
            RaisePropertyChanged();
        }
    }

    public bool IsAutoDeleteRequested
    {
        get => _isAutoDeleteRequested;
        set
        {
            if (_isAutoDeleteRequested == value)
            {
                return;
            }

            _isAutoDeleteRequested = value;
            RaisePropertyChanged();
        }
    }

    public RelayCommand AcceptCommand { get; }

    public RelayCommand DeclineCommand { get; }

    public event EventHandler<ConsentDecision>? DecisionCompleted;

    private void CompleteDecision(bool accepted)
    {
        if (_isDecisionMade)
        {
            return;
        }

        _isDecisionMade = true;
        AcceptCommand.RaiseCanExecuteChanged();
        DeclineCommand.RaiseCanExecuteChanged();

        var decision = new ConsentDecision(
            accepted,
            accepted && IsLoggingEnabled,
            accepted && IsAutoDeleteRequested,
            DateTimeOffset.UtcNow)
        {
            AllowBackgroundCapture = accepted && AllowBackgroundCapture,
            AllowWebhookUpload = accepted && AllowWebhookUpload
        };

        DecisionCompleted?.Invoke(this, decision);
        _onDecision?.Invoke(decision);
    }
}
