using CefSharp;
using System;
using System.Net.Http;
using System.Windows;
using System.IO;
using Microsoft.Win32;
using System.Threading.Tasks;

namespace PryGuard.Core.Browser.Handlers
{
    /// <summary>
    /// Handles the custom context menu for the browser.
    /// </summary>
    public class CustomContextMenuHandler : IContextMenuHandler
    {
        // Custom command identifiers
        private const int OpenLinkInNewTabCommand = 26501;
        private const int CopyLinkAddressCommand = 26502;
        private const int ViewImageCommand = 26503;
        private const int SaveImageAsCommand = 26504;
        private const int CopyImageCommand = 26505;
        private const int CopyImageAddressCommand = 26506;

        /// <summary>
        /// Event triggered when a link should be opened in a new tab.
        /// </summary>
        public event Action<string> OpenUrlInNewTabRequested;

        public void OnBeforeContextMenu(
            IWebBrowser browserControl,
            IBrowser browser,
            IFrame frame,
            IContextMenuParams parameters,
            IMenuModel model)
        {
            model.Clear();

            // Navigation commands
            AddNavigationCommands(model, browser);

            // Editable area commands
            if (parameters.IsEditable)
            {
                AddEditCommands(model);
            }
            else if (!string.IsNullOrEmpty(parameters.SelectionText))
            {
                // Selection commands
                model.AddItem(CefMenuCommand.Copy, "Copy");
                model.AddSeparator();
            }

            // Link commands
            if (!string.IsNullOrEmpty(parameters.LinkUrl))
            {
                AddLinkCommands(model);
            }

            // Image commands
            if (!string.IsNullOrEmpty(parameters.SourceUrl) && parameters.MediaType == ContextMenuMediaType.Image)
            {
                AddImageCommands(model);
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
            try
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
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            return false; // Use default context menu implementation
        }

        private async void SaveImage(string imageUrl)
        {
            string defaultFileName = ExtractDefaultFileName(imageUrl);

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All Files|*.*",
                FileName = defaultFileName
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    byte[] imageBytes = await GetImageBytesAsync(imageUrl);
                    await File.WriteAllBytesAsync(saveFileDialog.FileName, imageBytes);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private static async Task<byte[]> GetImageBytesAsync(string imageUrl)
        {
            if (imageUrl.StartsWith("data:"))
            {
                var base64Data = imageUrl.Substring(imageUrl.IndexOf(",") + 1);
                return Convert.FromBase64String(base64Data);
            }

            using var httpClient = new HttpClient();
            return await httpClient.GetByteArrayAsync(imageUrl);
        }

        private static string ExtractDefaultFileName(string imageUrl)
        {
            if (imageUrl.StartsWith("data:"))
            {
                var mimeType = imageUrl.Substring(5, imageUrl.IndexOf(";") - 5);
                var extension = mimeType.Split('/')[1];
                return $"Image.{extension}";
            }

            return Path.GetFileName(imageUrl) ?? "Image";
        }

        private void CopyImageToClipboard(string imageUrl)
        {
            // Implement logic to copy the image to the clipboard
            MessageBox.Show($"Copy Image {imageUrl}", "Copy Image", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static void AddNavigationCommands(IMenuModel model, IBrowser browser)
        {
            model.AddItem(CefMenuCommand.Back, "Back");
            model.SetEnabled(CefMenuCommand.Back, browser.CanGoBack);

            model.AddItem(CefMenuCommand.Forward, "Forward");
            model.SetEnabled(CefMenuCommand.Forward, browser.CanGoForward);

            model.AddItem(CefMenuCommand.Reload, "Reload");
            model.AddSeparator();
        }

        private static void AddEditCommands(IMenuModel model)
        {
            model.AddItem(CefMenuCommand.Cut, "Cut");
            model.AddItem(CefMenuCommand.Copy, "Copy");
            model.AddItem(CefMenuCommand.Paste, "Paste");
            model.AddSeparator();
        }

        private static void AddLinkCommands(IMenuModel model)
        {
            model.AddItem((CefMenuCommand)OpenLinkInNewTabCommand, "Open Link in New Tab");
            model.AddItem((CefMenuCommand)CopyLinkAddressCommand, "Copy Link Address");
            model.AddSeparator();
        }

        private static void AddImageCommands(IMenuModel model)
        {
            model.AddItem((CefMenuCommand)ViewImageCommand, "View Image");
            model.AddItem((CefMenuCommand)SaveImageAsCommand, "Save Image As...");
            model.AddItem((CefMenuCommand)CopyImageCommand, "Copy Image");
            model.AddItem((CefMenuCommand)CopyImageAddressCommand, "Copy Image Address");
            model.AddSeparator();
        }
    }
}
