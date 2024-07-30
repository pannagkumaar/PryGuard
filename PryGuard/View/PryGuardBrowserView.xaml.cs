using System.Windows;
using System.Windows.Controls;
using PryGuard.ViewModel;
using System.Windows.Input;
using PryGuard.Model;
namespace PryGuard.View;
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
