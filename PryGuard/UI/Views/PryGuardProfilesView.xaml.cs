using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System;
using PryGuard.UI.Views;
using PryGuard.DataModels;
using PryGuard.UI.ViewModels;

namespace PryGuard.UI.Views
{
    public partial class PryGuardProfilesView : Window, IBaseView
    {
        // BaseViewModel property as required by IBaseView
        public BaseViewModel ViewModel { get; set; }

        // Starting point for drag operations
        private Point _startPoint;

        public PryGuardProfilesView()
        {
            InitializeComponent();

            // Initialize the ViewModel
            var viewModel = new PryGuardProfilesViewModel();

            // Assign the ViewModel to the ViewModel property
            ViewModel = viewModel;

            // Set the DataContext for data binding
            this.DataContext = ViewModel;

            // Bind the ViewModel's RequestClose action to the window's Close method
            if (ViewModel is PryGuardProfilesViewModel profilesViewModel)
            {
                profilesViewModel.RequestClose = () => this.Close();
            }

            // Subscribe to the Closed event to dispose of the ViewModel
            this.Closed += PryGuardProfilesView_Closed;
        }

        /// <summary>
        /// Event handler for the window's Closed event.
        /// Disposes of the ViewModel to release resources.
        /// </summary>
        private void PryGuardProfilesView_Closed(object sender, EventArgs e)
        {
            // Dispose of the ViewModel if it implements IDisposable
            if (ViewModel is IDisposable disposableViewModel)
            {
                disposableViewModel.Dispose();
            }
        }

        /// <summary>
        /// Allows the window to be dragged when the mouse is pressed.
        /// </summary>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Initiate window drag on left mouse button press
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// Captures the starting point of the mouse when the user begins a drag operation on a profile.
        /// </summary>
        private void ProfileBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);  // Capture the starting point of the mouse
        }

        /// <summary>
        /// Initiates a drag-and-drop operation if the mouse has moved beyond the system-defined threshold.
        /// </summary>
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

                    var profile = border.DataContext as ProfileTab;
                    if (profile == null) return;

                    // Create a DataObject with the profile information
                    DataObject data = new DataObject("profile", profile);

                    // Initiate the drag-and-drop operation
                    DragDrop.DoDragDrop(border, data, DragDropEffects.Move);
                }
            }
        }

        /// <summary>
        /// Handles the drop event by swapping the positions of the dragged profile and the target profile.
        /// </summary>
        private void ProfileBorder_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("profile"))
            {
                var droppedProfile = e.Data.GetData("profile") as ProfileTab;
                var targetBorder = sender as Border;
                var targetProfile = targetBorder?.DataContext as ProfileTab;

                if (droppedProfile != null && targetProfile != null && droppedProfile != targetProfile)
                {
                    var viewModel = this.DataContext as PryGuardProfilesViewModel;
                    viewModel?.MoveProfile(droppedProfile, targetProfile);
                }
            }
        }
    }
}
