using Avalonia.Controls;

namespace SafeKeyRecorder.Views.Components;

public partial class BackgroundStatusBanner : UserControl
{
    public BackgroundStatusBanner()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
    }
}
