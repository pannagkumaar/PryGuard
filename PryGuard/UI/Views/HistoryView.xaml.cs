using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PryGuard.UI.ViewModels;

namespace PryGuard.UI.Views
{
    public partial class HistoryView : UserControl
    {
        private bool _isDarkTheme;

        public HistoryView()
        {
            InitializeComponent();
            SetLightTheme();
            themeToggleButton.Content = _isDarkTheme ? "Light Theme" : "Dark Theme";
        }

        private void OnLinkClick(object sender, MouseButtonEventArgs e)
        {
            var link = (sender as TextBlock)?.Text;
            if (link != null)
            {
                var viewModel = DataContext as PryGuardBrowserViewModel;
                viewModel?.LoadHistoryLinkCommand.Execute(link);
            }
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
}
