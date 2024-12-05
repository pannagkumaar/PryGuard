using PryGuard.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PryGuard.UI.Views
{
    public partial class ProfileSelectionWindow : Window
    {
        public PryGuardProfile SelectedProfile { get; private set; }

        public ProfileSelectionWindow(List<PryGuardProfile> profiles)
        {
            InitializeComponent();
            ProfileComboBox.ItemsSource = profiles;
            ProfileComboBox.DisplayMemberPath = "Name";
        }

        private void OnImportButtonClick(object sender, RoutedEventArgs e)
        {
            SelectedProfile = ProfileComboBox.SelectedItem as PryGuardProfile;
            DialogResult = true;
            Close();
        }
        private void OnCloseButtonClick(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

    }

}
