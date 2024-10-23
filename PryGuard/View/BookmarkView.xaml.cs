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
    /// done by tania
    public partial class BookmarkView : UserControl
    {
        public BookmarkView()
        {
            InitializeComponent();
            this.DataContext = new PryGuardBrowserViewModel();
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

    }
    // done by tania
}
