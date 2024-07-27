using PryGuard.ViewModel;

namespace PryGuard.View;
public partial class PryGuardProfileSettingsView : IBaseView
{
    public BaseViewModel ViewModel { get; set; }
    public PryGuardProfileSettingsView()
    {
        InitializeComponent();
    }

    private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        this.DragMove();
    }

    private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {

    }

    private void rbMem_Checked(object sender, System.Windows.RoutedEventArgs e)
    {

    }

    private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
    {

    }
}
