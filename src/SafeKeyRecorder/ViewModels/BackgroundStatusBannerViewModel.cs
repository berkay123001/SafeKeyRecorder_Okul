using System;
using Avalonia.Media;
using SafeKeyRecorder.Background;

namespace SafeKeyRecorder.ViewModels;

public sealed class BackgroundStatusBannerViewModel : ViewModelBase, IBackgroundStatusBanner
{
    private string _message = "Kayıt pasif.";
    private BannerVisualState _visualState = BannerVisualState.Passive;
    private bool _isVisible = true;
    private IBrush _backgroundBrush = Brushes.LightGray;
    private bool _isToggleChecked = true;
    private bool _suppressToggleFeedback;
    private bool _stateVisible = true;

    public string Message
    {
        get => _message;
        private set
        {
            if (_message == value)
            {
                return;
            }

            _message = value;
            RaisePropertyChanged();
        }
    }

    public bool IsToggleChecked
    {
        get => _isToggleChecked;
        set
        {
            if (_isToggleChecked == value)
            {
                return;
            }

            _isToggleChecked = value;
            RaisePropertyChanged();

            if (!_suppressToggleFeedback)
            {
                ToggleChanged?.Invoke(this, value);
                ApplyVisibility();
            }
        }
    }

    public IBrush BackgroundBrush
    {
        get => _backgroundBrush;
        private set
        {
            if (Equals(_backgroundBrush, value))
            {
                return;
            }

            _backgroundBrush = value;
            RaisePropertyChanged();
        }
    }

    public BannerVisualState VisualState
    {
        get => _visualState;
        private set
        {
            if (_visualState == value)
            {
                return;
            }

            _visualState = value;
            RaisePropertyChanged();
        }
    }

    public bool IsVisible
    {
        get => _isVisible;
        private set
        {
            if (_isVisible == value)
            {
                return;
            }

            _isVisible = value;
            RaisePropertyChanged();
        }
    }

    public event EventHandler? StateChanged;
    public event EventHandler<bool>? ToggleChanged;

    public void ShowBackgroundEnabled()
    {
        ForceToggle(true);
        UpdateState("Arka plan kayıt modu aktif.", BannerVisualState.Background, true);
    }

    public void ShowBackgroundDisabled()
    {
        ForceToggle(false);
        UpdateState("Arka plan modu kapalı. Odak içi kayıt sürüyor.", BannerVisualState.Foreground, true);
    }

    public void ShowPassive(string message)
    {
        ForceToggle(false);
        UpdateState(message, BannerVisualState.Passive, true);
    }

    public void Hide()
    {
        ForceToggle(false);
        UpdateState(Message, VisualState, false);
    }

    private void UpdateState(string message, BannerVisualState visualState, bool visible)
    {
        Message = message;
        VisualState = visualState;
        _stateVisible = visible;
        BackgroundBrush = SelectBrush(visualState);
        ApplyVisibility();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyVisibility()
    {
        var combined = _stateVisible && _isToggleChecked;
        if (_isVisible == combined)
        {
            return;
        }

        _isVisible = combined;
        RaisePropertyChanged(nameof(IsVisible));
    }

    private void ForceToggle(bool value)
    {
        if (_isToggleChecked == value)
        {
            return;
        }

        _suppressToggleFeedback = true;
        IsToggleChecked = value;
        _suppressToggleFeedback = false;
        ApplyVisibility();
    }

    private static IBrush SelectBrush(BannerVisualState state) => state switch
    {
        BannerVisualState.Foreground => Brushes.LightGreen,
        BannerVisualState.Background => Brushes.Gold,
        _ => Brushes.LightGray
    };
}
