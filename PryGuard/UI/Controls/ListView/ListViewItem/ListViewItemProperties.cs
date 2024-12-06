using System.Windows;
namespace PryGuard.UI.Controls.ListView.ListViewItem;

public class ListViewItemProperties : DependencyObject
{
    #region TimeHistory
    public static readonly DependencyProperty TimeHistoryProperty =
        DependencyProperty.RegisterAttached("TimeHistory", typeof(string), typeof(ListViewItemProperties), new PropertyMetadata(null));

    public static string GetTimeHistory(System.Windows.Controls.ListViewItem element)
    {
        return (string)element.GetValue(TimeHistoryProperty);
    }

    public static void SetTimeHistory(System.Windows.Controls.ListViewItem element, string value)
    {
        element.SetValue(TimeHistoryProperty, value);
    }
    #endregion

    #region DescHistory
    public static readonly DependencyProperty DescHistoryProperty =
       DependencyProperty.RegisterAttached("DescHistory", typeof(string), typeof(ListViewItemProperties), new PropertyMetadata(null));

    public static string GetDescHistory(System.Windows.Controls.ListViewItem element)
    {
        return (string)element.GetValue(DescHistoryProperty);
    }

    public static void SetDescHistory(System.Windows.Controls.ListViewItem element, string value)
    {
        element.SetValue(DescHistoryProperty, value);
    }
    #endregion

    #region LinkPreview
    public static readonly DependencyProperty LinkPreviewProperty =
      DependencyProperty.RegisterAttached("LinkPreview", typeof(string), typeof(ListViewItemProperties), new PropertyMetadata(null));

    public static string GetLinkPreview(System.Windows.Controls.ListViewItem element)
    {
        return (string)element.GetValue(LinkPreviewProperty);
    }

    public static void SetLinkPreview(System.Windows.Controls.ListViewItem element, string value)
    {
        element.SetValue(LinkPreviewProperty, value);
    }
    #endregion

    #region FullLink
    public static readonly DependencyProperty FullLinkProperty =
      DependencyProperty.RegisterAttached("FullLink", typeof(string), typeof(ListViewItemProperties), new PropertyMetadata(null));

    public static string GetFullLink(System.Windows.Controls.ListViewItem element)
    {
        return (string)element.GetValue(FullLinkProperty);
    }

    public static void SetFullLink(System.Windows.Controls.ListViewItem element, string value)
    {
        element.SetValue(FullLinkProperty, value);
    }
    #endregion
}
