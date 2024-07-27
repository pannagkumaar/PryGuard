using PryGuard.ViewModel;
using System.Windows.Input;

namespace PryGuard.View;

public partial class PryGuardProfilesView : IBaseView
{
    public BaseViewModel ViewModel { get; set; }

    public PryGuardProfilesView()
    {
        InitializeComponent();
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        this.DragMove();
    }
}
