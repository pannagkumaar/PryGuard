using PryGuard.Model;
using PryGuard.ViewModel;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System;

namespace PryGuard.View;

public partial class PryGuardProfilesView : IBaseView
{
    public BaseViewModel ViewModel { get; set; }
    private Point _startPoint;

    public PryGuardProfilesView()
    {
        InitializeComponent();
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        this.DragMove();
    }
    private void ProfileBorder_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _startPoint = e.GetPosition(null);  // Capture the starting point of the mouse
    }

    private void ProfileBorder_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            Point currentPosition = e.GetPosition(null);

            // Check if the mouse has moved beyond a small threshold before starting the drag
            if (Math.Abs(currentPosition.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(currentPosition.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                var border = sender as Border;
                if (border == null) return;

                var profile = (ProfileTab)border.DataContext;
                DataObject data = new DataObject("profile", profile);

                DragDrop.DoDragDrop(border, data, DragDropEffects.Move);
            }
        }
    }

    private void ProfileBorder_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent("profile"))
        {
            var droppedProfile = e.Data.GetData("profile") as ProfileTab;
            var targetBorder = sender as Border;
            var targetProfile = targetBorder?.DataContext as ProfileTab;

            if (droppedProfile != null && targetProfile != null)
            {
                var viewModel = DataContext as PryGuardProfilesViewModel;
                viewModel?.MoveProfile(droppedProfile, targetProfile);
            }
        }
    }
}
