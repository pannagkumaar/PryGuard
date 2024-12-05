// DownloadsPage.xaml.cs
using System.Windows.Controls;
using PryGuard.Core.Browser.Handlers;

namespace PryGuard.UI.Views
{
    public partial class DownloadPage : UserControl
    {
        public DownloadPage()
        {
            InitializeComponent();
            DataContext = DownloadManager.Instance;
        }
    }
}
