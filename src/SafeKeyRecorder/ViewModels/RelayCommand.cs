using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;

namespace SafeKeyRecorder.ViewModels;

public sealed class RelayCommand : ICommand
{
    private readonly Action<object?>? _execute;
    private readonly Func<object?, Task>? _executeAsync;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action execute)
        : this(_ => execute(), (Func<object?, bool>?)null)
    {
    }

    public RelayCommand(Action execute, Func<bool> canExecute)
        : this(_ => execute(), _ => canExecute())
    {
    }

    public RelayCommand(Action<object?> execute)
        : this(execute, (Func<object?, bool>?)null)
    {
    }

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public RelayCommand(Func<object?, Task> executeAsync)
        : this(executeAsync, null)
    {
    }

    public RelayCommand(Func<object?, Task> executeAsync, Func<object?, bool>? canExecute)
    {
        _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public async void Execute(object? parameter)
    {
        if (_executeAsync is not null)
        {
            await _executeAsync(parameter).ConfigureAwait(false);
            return;
        }

        _execute?.Invoke(parameter);
    }

    public void RaiseCanExecuteChanged()
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        Dispatcher.UIThread.Post(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));
    }
}
