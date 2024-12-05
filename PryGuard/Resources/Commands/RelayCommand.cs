using System;
using System.Windows;
using System.Windows.Input;

namespace PryGuard.Resources.Commands;
public class RelayCommand : ICommand
{
    #region Fields

    readonly Action<object> _action;
    readonly Predicate<object> _canExecute;
    private Action<object, RoutedEventArgs> newFingerprintButton_Click;

    #endregion

    #region Constructors
    public RelayCommand(Action action)
    {
        _action = x => action();
    }

    public RelayCommand(Action<object> action)
    {
        _action = action;
    }
    public RelayCommand(Action<object> execute, Predicate<object> canExecute)
    {
        if (execute == null)
            throw new ArgumentNullException("execute");

        _action = execute;
        _canExecute = canExecute;
    }

    public RelayCommand(Action<object, RoutedEventArgs> newFingerprintButton_Click)
    {
        this.newFingerprintButton_Click = newFingerprintButton_Click;
    }
    #endregion

    #region ICommand Members

    public bool CanExecute(object parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }

    public event EventHandler CanExecuteChanged
    {
        add
        {
            if (_canExecute != null)
            {
                CommandManager.RequerySuggested += value;
            }
        }

        remove
        {
            if (_canExecute != null)
            {
                CommandManager.RequerySuggested -= value;
            }
        }
    }

    public void Execute(object parameter)
    {
        _action(parameter);
    }

    #endregion

    #region RaiseCanExecuteChanged

    public void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }

    #endregion
}

public class RelayCommand<T> : ICommand
{
    #region Fields

    private readonly Action<T> _execute;
    private readonly Predicate<T> _canExecute;

    #endregion

    #region Constructors

    public RelayCommand(Action<T> execute)
        : this(execute, null)
    {
    }

    public RelayCommand(Action<T> execute, Predicate<T> canExecute)
    {
        if (execute == null)
            throw new ArgumentNullException("execute");

        _execute = execute;
        _canExecute = canExecute;
    }

    #endregion

    #region ICommand Members

    public bool CanExecute(object parameter)
    {
        return _canExecute == null || _canExecute((T)parameter);
    }

    public event EventHandler CanExecuteChanged
    {
        add
        {
            if (_canExecute != null)
            {
                CommandManager.RequerySuggested += value;
            }
        }

        remove
        {
            if (_canExecute != null)
            {
                CommandManager.RequerySuggested -= value;
            }
        }
    }

    public void Execute(object parameter)
    {
        _execute((T)parameter);
    }

    #endregion

    #region RaiseCanExecuteChanged

    public void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }

    #endregion
}
