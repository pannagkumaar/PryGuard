using System;
using System.Windows.Input;

namespace PryGuard.Resources.Commands;
public interface IDelegateCommand : ICommand
{
    void RaiseCanExecuteChanged();
}
public class DelegateCommand : IDelegateCommand
{
    Action<object> execute;
    Func<object, bool> canExecute;

    public event EventHandler CanExecuteChanged;

    public DelegateCommand(Action<object> execute, Func<object, bool> canExecute)
    {
        this.execute = execute;
        this.canExecute = canExecute;
    }

    public DelegateCommand(Action<object> execute)
    {
        this.execute = execute;
        canExecute = AlwaysCanExecute;
    }

    public void Execute(object param)
    {
        execute(param);
    }

    public bool CanExecute(object param)
    {
        return canExecute(param);
    }

    public void RaiseCanExecuteChanged()
    {
        if (CanExecuteChanged != null)
            CanExecuteChanged(this, EventArgs.Empty);
    }

    private bool AlwaysCanExecute(object param)
    {
        return true;
    }
}

