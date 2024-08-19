using PryGuard.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

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

    private void Window_StateChanged(object sender, EventArgs e)
    {
        if (WindowState == WindowState.Maximized)
        {
            LayoutScaleTransform.ScaleX = 1.2;
            LayoutScaleTransform.ScaleY = 1.2;
        }
        else
        {
            LayoutScaleTransform.ScaleX = 1.0;
            LayoutScaleTransform.ScaleY = 1.0;
        }
    }


}
