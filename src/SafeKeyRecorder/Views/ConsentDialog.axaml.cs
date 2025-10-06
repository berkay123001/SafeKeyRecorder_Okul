using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SafeKeyRecorder.ViewModels;

namespace SafeKeyRecorder.Views;

public partial class ConsentDialog : Window
{
    public ConsentDialog()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is ConsentDialogViewModel vm)
        {
            vm.DecisionCompleted -= OnDecisionCompleted;
            vm.DecisionCompleted += OnDecisionCompleted;
        }
    }

    private void OnDecisionCompleted(object? sender, Models.ConsentDecision decision)
    {
        Close(decision.Accepted);
    }
}
