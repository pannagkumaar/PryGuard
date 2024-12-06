using System.Windows.Controls;
using System;
using PryGuard.Resources.Commands;

public class CustomTabItem : TabItem
{
    public string Title { get; set; }   
    public int Tag { get; set; }
    public bool IsIncognito { get; set; }
    public string Address { get; set; }
    public RelayCommand CloseTabCommand { get; set; }

    // New Properties
    public DateTime LastUsed { get; set; }
    public bool IsSuspended { get; set; } = false;
    public string SuspendedUrl { get; set; } // To store the URL before suspension

    public CustomTabItem()
    {
        LastUsed = DateTime.Now;
    }
}
