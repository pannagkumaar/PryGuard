using PryGuard.Services.Commands;
using System.Windows.Controls;

public class CustomTabItem : TabItem
{
    // Custom properties
    // Use 'new' keyword to hide the base class property


    public string Title { get; set; }
    public string Address { get; set; }
    public RelayCommand CloseTabCommand { get; set; }


}