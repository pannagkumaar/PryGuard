using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace PryGuard.UI.ViewModels;
public abstract class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    public BaseViewModel() { }

    protected void Set<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        if (propertyName != null)
            OnPropertyChanged(propertyName);
    }
    public T GetViewModel<T>() where T : BaseViewModel
    {
        return this as T;
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChangedEventHandler propertyChanged = PropertyChanged;
        if (propertyChanged == null)
            return;
        propertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
    public virtual void Close()
    {

    }
    protected void NextStep(BaseViewModel viewModel)
    {
        ViewManager.Show(viewModel);
        ViewManager.Close(this);
    }
}