using System.Windows.Controls;
using System;
using System.Diagnostics;
using PryGuard.Core.Browser.Settings;
using PryGuard.UI.Views;
using PryGuard.Core.Browser.Model.Configs;
using PryGuard.UI.ViewModels;

namespace PryGuard.UI.Views
{
    public partial class PryGuardProfileSettingsView : IBaseView
    {
        public BaseViewModel ViewModel { get; set; }
        private bool _isPageLoaded = false;

        public PryGuardProfileSettingsView()
        {
            InitializeComponent();
            this.DataContext = new PryGuardProfileSettingsViewModel();
            this.Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _isPageLoaded = true;
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void ComboBoxOS_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!_isPageLoaded)
            {
                // Page is not fully loaded, exit the method
                return;
            }

            var comboBox = sender as ComboBox;
            if (comboBox != null && comboBox.SelectedItem != null)
            {
                var viewModel = this.DataContext as PryGuardProfileSettingsViewModel;
                if (viewModel != null)
                {
                    OSVersion selectedOSVersion;
                    if (Enum.TryParse(comboBox.SelectedItem.ToString(), out selectedOSVersion))
                    {
                        viewModel.PryGuardProf.FakeProfile.OsVersion = selectedOSVersion;
                        viewModel.PryGuardProf.FakeProfile.UserAgent = FakeProfileFactory.GenerateUserAgent(viewModel.PryGuardProf.FakeProfile);
                    }
                }
            }
        }

        private void ComboBoxlang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isPageLoaded)
            {
                // Page is not fully loaded, exit the method
                return;
            }

            var comboBox = sender as ComboBox;
            if (comboBox != null && comboBox.SelectedItem != null)
            {
                var viewModel = this.DataContext as PryGuardProfileSettingsViewModel;
                if (viewModel != null)
                {
                    BrowserLanguage selectedLanguage;
                    if (Enum.TryParse(comboBox.SelectedItem.ToString(), out selectedLanguage))
                    {
                        viewModel.PryGuardProf.FakeProfile.ChromeLanguageInfo = BrowserLanguageHelper.GetFullInfo(selectedLanguage);
                    }
                }
            }
        }

        private void rbMem_Checked(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void RadioButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
