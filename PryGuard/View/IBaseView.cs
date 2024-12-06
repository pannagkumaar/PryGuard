using PryGuard.ViewModel;

namespace PryGuard.View;
public interface IBaseView
{
    BaseViewModel ViewModel { get; set; }

    void Close();
}
