// DownloadsPage.xaml.cs
using System.Windows.Controls;
using PryGuard.Core.ChromeApi.Handlers;

namespace PryGuard.View
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
