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
using PryGuard.UI.ViewModels;

namespace PryGuard.UI.Views
{
    /// done by tania
    public partial class BookmarkView : UserControl
    {
        private bool _isDarkTheme;

        public BookmarkView()
        {
            InitializeComponent();
            SetLightTheme(); // This will set the initial theme to light
            themeToggleButton.Content = "Dark Theme"; // Since the initial theme is light, the button should say "Dark Theme"
        }


        private void OnLinkClick(object sender, MouseButtonEventArgs e)
        {
            var link = (sender as TextBlock)?.Text;
            if (link != null)
            {
                var viewModel = DataContext as PryGuardBrowserViewModel;
                if (viewModel?.LoadHistoryLinkCommand.CanExecute(link) == true)
                {
                    viewModel.LoadHistoryLinkCommand.Execute(link);
                }
            }

            e.Handled = true;
        }

        private void SetLightTheme()
        {
            var lightTheme = new ResourceDictionary { Source = new Uri("/UI/Themes/LightTheme.xaml", UriKind.Relative) };
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(lightTheme);
            _isDarkTheme = false;
        }

        private void SetDarkTheme()
        {
            var darkTheme = new ResourceDictionary { Source = new Uri("/UI/Themes/DarkTheme.xaml", UriKind.Relative) };
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(darkTheme);
            _isDarkTheme = true;
        }

        private void OnThemeToggle_Checked(object sender, RoutedEventArgs e)
        {
            SetDarkTheme();
            themeToggleButton.Content = "Light Theme";
        }

        private void OnThemeToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            SetLightTheme();
            themeToggleButton.Content = "Dark Theme";
        }

    }
    // done by tania
}
