using System.Windows.Input;

namespace FinanceTracker.Helpers;

public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;

    // Constructor accepting the execute action and optional canExecute predicate
    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    // Determines if the command can execute
    public bool CanExecute(object? parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }

    // Executes the command action
    public void Execute(object? parameter)
    {
        _execute(parameter);
    }

    // Event to notify WPF that CanExecute might have changed
    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
}