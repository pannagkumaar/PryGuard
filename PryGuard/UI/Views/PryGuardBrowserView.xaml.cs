using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PryGuard.DataModels;
using PryGuard.UI.Views;
using PryGuard.UI.ViewModels;
namespace PryGuard.UI.Views;
public partial class PryGuardBrowserView : IBaseView
{
    public BaseViewModel ViewModel { get; set; }

    public PryGuardBrowserView()
    {
        
       
        InitializeComponent();
        
        //DataContext = new PryGuardBrowserViewModel(this, new PryGuardProfile());
    }
    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { DragMove(); }
}
