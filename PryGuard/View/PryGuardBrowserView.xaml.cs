using PryGuard.ViewModel;
using System.Windows.Input;

namespace PryGuard.View;
public partial class PryGuardBrowserView : IBaseView
{
    public BaseViewModel ViewModel { get; set; }

    public PryGuardBrowserView()
    {
        InitializeComponent();
    }
    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { DragMove(); }
}
