using CefSharp;

using System;
using System.Net.Http;
using System.Windows;

using System.IO;


namespace PryGuard.Core.ChromeApi.Handlers;

public class CustomContextMenuHandler : IContextMenuHandler
    {
        // Custom command identifiers (use values above 26500 to avoid conflicts)
        private const int OpenLinkInNewTabCommand = 26501;
        private const int CopyLinkAddressCommand = 26502;
        private const int ViewImageCommand = 26503;
        private const int SaveImageAsCommand = 26504;
        private const int CopyImageCommand = 26505;
        private const int CopyImageAddressCommand = 26506;
        private const int InspectElementCommand = 26507;
        // Add more custom command IDs as needed

        // Optional: Event to handle opening links in new tabs
        public event Action<string> OpenUrlInNewTabRequested;

        public void OnBeforeContextMenu(
            IWebBrowser browserControl,
            IBrowser browser,
            IFrame frame,
            IContextMenuParams parameters,
            IMenuModel model)
        {
            // Clear the existing menu
            model.Clear();

            // Navigation commands
            model.AddItem(CefMenuCommand.Back, "Back");
            model.SetEnabled(CefMenuCommand.Back, browser.CanGoBack);

            model.AddItem(CefMenuCommand.Forward, "Forward");
            model.SetEnabled(CefMenuCommand.Forward, browser.CanGoForward);

            model.AddItem(CefMenuCommand.Reload, "Reload");
            model.AddSeparator();

            // Edit commands if in an editable area
            if (parameters.IsEditable)
            {
                model.AddItem(CefMenuCommand.Cut, "Cut");
                model.AddItem(CefMenuCommand.Copy, "Copy");
                model.AddItem(CefMenuCommand.Paste, "Paste");
                model.AddSeparator();
            }
            else if (!string.IsNullOrEmpty(parameters.SelectionText))
            {
                // If text is selected, offer Copy
                model.AddItem(CefMenuCommand.Copy, "Copy");
                model.AddSeparator();
            }

            // Link commands
            if (!string.IsNullOrEmpty(parameters.LinkUrl))
            {
                model.AddItem((CefMenuCommand)OpenLinkInNewTabCommand, "Open Link in New Tab");
                model.AddItem((CefMenuCommand)CopyLinkAddressCommand, "Copy Link Address");
                model.AddSeparator();
            }

            // Image commands
            if (!string.IsNullOrEmpty(parameters.SourceUrl) && parameters.MediaType == ContextMenuMediaType.Image)
            {
                model.AddItem((CefMenuCommand)ViewImageCommand, "View Image");
                model.AddItem((CefMenuCommand)SaveImageAsCommand, "Save Image As...");
                model.AddItem((CefMenuCommand)CopyImageCommand, "Copy Image");
                model.AddItem((CefMenuCommand)CopyImageAddressCommand, "Copy Image Address");
                model.AddSeparator();
            }

            // Miscellaneous commands
            model.AddItem(CefMenuCommand.Print, "Print");
            model.AddItem(CefMenuCommand.ViewSource, "View Source");
            model.AddSeparator();

       
        }

        public bool OnContextMenuCommand(
            IWebBrowser browserControl,
            IBrowser browser,
            IFrame frame,
            IContextMenuParams parameters,
            CefMenuCommand commandId,
            CefEventFlags eventFlags)
        {
            switch ((int)commandId)
            {
                case (int)CefMenuCommand.Back:
                    browser.GoBack();
                    return true;

                case (int)CefMenuCommand.Forward:
                    browser.GoForward();
                    return true;

                case (int)CefMenuCommand.Reload:
                    browser.Reload();
                    return true;

                case (int)CefMenuCommand.Cut:
                    frame.Cut();
                    return true;

                case (int)CefMenuCommand.Copy:
                    frame.Copy();
                    return true;

                case (int)CefMenuCommand.Paste:
                    frame.Paste();
                    return true;

                case (int)CefMenuCommand.Print:
                    browser.GetHost().Print();
                    return true;

                case (int)CefMenuCommand.ViewSource:
                    frame.ViewSource();
                    return true;

                


                case OpenLinkInNewTabCommand:
                    OpenUrlInNewTabRequested?.Invoke(parameters.LinkUrl);
                    return true;

                case CopyLinkAddressCommand:
                    Clipboard.SetText(parameters.LinkUrl);
                    return true;

                case ViewImageCommand:
                    frame.LoadUrl(parameters.SourceUrl);
                    return true;

                case SaveImageAsCommand:
                    SaveImage(parameters.SourceUrl);
                    return true;

                case CopyImageCommand:
                    CopyImageToClipboard(parameters.SourceUrl);
                    return true;

                case CopyImageAddressCommand:
                    Clipboard.SetText(parameters.SourceUrl);
                    return true;

                default:
                    return false;
            }
        }

        public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
        {
            // No action needed
        }

        public bool RunContextMenu(
            IWebBrowser browserControl,
            IBrowser browser,
            IFrame frame,
            IContextMenuParams parameters,
            IMenuModel model,
            IRunContextMenuCallback callback)
        {
            // Return false to use the default context menu implementation
            return false;
        }

        // Helper methods for image commands
        private async void SaveImage(string imageUrl)
        {
            string defaultFileName;

            // Determine the default filename based on whether it's a data URL or a normal URL
            if (imageUrl.StartsWith("data:"))
            {
                // Extract the MIME type from the data URL (e.g., "image/png")
                var mimeType = imageUrl.Substring(5, imageUrl.IndexOf(";") - 5);
                var extension = mimeType.Split('/')[1]; // Extract the extension (e.g., "png")
                defaultFileName = $"Image.{extension}"; // Set a generic default name
            }
            else
            {
                // Use the filename from the URL path
                defaultFileName = System.IO.Path.GetFileName(imageUrl);
            }

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All Files|*.*",
                FileName = defaultFileName
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    byte[] imageBytes;

                    if (imageUrl.StartsWith("data:"))
                    {
                        // Handle base64 encoded image data
                        var base64Data = imageUrl.Substring(imageUrl.IndexOf(",") + 1);
                        imageBytes = Convert.FromBase64String(base64Data);
                    }
                    else
                    {
                        // Handle URL images
                        using (var httpClient = new HttpClient())
                        {
                            imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
                        }
                    }

                    await File.WriteAllBytesAsync(saveFileDialog.FileName, imageBytes);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }



        private void CopyImageToClipboard(string imageUrl)
        {
            // Implement logic to copy the image to the clipboard
            MessageBox.Show($"Copy Image {imageUrl}", "Copy Image", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
