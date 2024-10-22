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
using PryGuard.ViewModel;

namespace PryGuard.View
{
    /// <summary>
    /// Interaction logic for HistoryView.xaml
    /// </summary>
    public partial class HistoryView : UserControl
    {
        public HistoryView()
        {
            InitializeComponent();
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
    }

}
