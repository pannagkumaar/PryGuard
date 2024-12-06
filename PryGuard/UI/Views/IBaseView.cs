using PryGuard.UI.ViewModels;

namespace PryGuard.UI.Views;
public interface IBaseView
{
    BaseViewModel ViewModel { get; set; }

    void Close();
}
