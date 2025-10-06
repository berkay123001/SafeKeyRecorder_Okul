using System;
using Avalonia.Media;
using Avalonia.Threading;
using SafeKeyRecorder.Background;

namespace SafeKeyRecorder.ViewModels;

public sealed class BackgroundStatusBannerViewModel : ViewModelBase, IBackgroundStatusBanner
{
    private string _message = "Kayıt pasif.";
    private BannerVisualState _visualState = BannerVisualState.Passive;
    private bool _isVisible = true;
    private IBrush _backgroundBrush = Brushes.LightGray;
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

    public void ShowBackgroundEnabled()
    {
        UpdateState("Arka plan kayıt modu aktif.", BannerVisualState.Background, true);
    }

    public void ShowBackgroundDisabled()
    {
        UpdateState("Arka plan modu kapalı. Odak içi kayıt sürüyor.", BannerVisualState.Foreground, true);
    }

    public void ShowPassive(string message)
    {
        UpdateState(message, BannerVisualState.Passive, true);
    }

    public void Hide()
    {
        UpdateState(Message, VisualState, false);
    }

    private void UpdateState(string message, BannerVisualState visualState, bool visible)
    {
        void Apply()
        {
            Message = message;
            VisualState = visualState;
            _stateVisible = visible;
            BackgroundBrush = SelectBrush(visualState);
            ApplyVisibility();
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            Apply();
        }
        else
        {
            Dispatcher.UIThread.Post(Apply);
        }
    }

    private void ApplyVisibility()
    {
        var combined = _stateVisible;
        if (_isVisible == combined)
        {
            return;
        }

        _isVisible = combined;
        RaisePropertyChanged(nameof(IsVisible));
    }

    private static IBrush SelectBrush(BannerVisualState state) => state switch
    {
        BannerVisualState.Foreground => new SolidColorBrush(Color.FromRgb(56, 142, 60)),
        BannerVisualState.Background => new SolidColorBrush(Color.FromRgb(230, 145, 56)),
        _ => new SolidColorBrush(Color.FromRgb(66, 66, 66))
    };
}
