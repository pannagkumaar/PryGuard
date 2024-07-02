using System;
using System.Windows.Forms;
using generator1.Core;
using Microsoft.Playwright;
using System.Threading.Tasks;

namespace generator1
{
    public partial class FingerprintPage : Form
    {
        private FakeProfile _fingerprint;
        private IPlaywright _playwright;
        private IBrowser _browser;

        public FingerprintPage(FakeProfile fingerprint)
        {
            InitializeComponent();
            _fingerprint = fingerprint;
            DisplayFingerprint();
        }

        private void DisplayFingerprint()
        {
            // Display the fingerprint details in a multiline TextBox
            txtFingerprint.Multiline = true;
            txtFingerprint.ScrollBars = ScrollBars.Vertical;

            txtFingerprint.Text = $"Fingerprint Details:\r\n\r\n{_fingerprint.ToString()}";
        }

        private async Task InjectFingerprintIntoBrowser(FakeProfile fingerprint)
        {
            try
            {
                _playwright = await Playwright.CreateAsync();

                BrowserTypeLaunchOptions launchOptions = null;

                switch (fingerprint.BrowserTypeType)
                {
                    case EBrowserType.Chrome:
                        launchOptions = new BrowserTypeLaunchOptions
                        {
                            Headless = false,
                            ExecutablePath = @"C:\Users\panna\AppData\Local\ms-playwright\chromium-1124\chrome-win\chrome.exe"
                        };
                        _browser = await _playwright.Chromium.LaunchAsync(launchOptions);
                        break;
                    case EBrowserType.Firefox:
                        launchOptions = new BrowserTypeLaunchOptions
                        {
                            Headless = false,
                            ExecutablePath = @"C:\Users\panna\AppData\Local\ms-playwright\firefox-1454\firefox\firefox.exe"
                        };
                        _browser = await _playwright.Firefox.LaunchAsync(launchOptions);
                        break;
                    case EBrowserType.Webkit:
                        launchOptions = new BrowserTypeLaunchOptions
                        {
                            Headless = false,
                            ExecutablePath = @"C:\Users\panna\AppData\Local\ms-playwright\webkit-2035\Playwright.exe"
                        };
                        _browser = await _playwright.Webkit.LaunchAsync(launchOptions);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var contextOptions = new BrowserNewContextOptions
                {
                    UserAgent = fingerprint.UserAgent,
                    ViewportSize = new ViewportSize { Width = fingerprint.ScreenSize.Width, Height = fingerprint.ScreenSize.Height },
                    Locale = fingerprint.ChromeLanguageInfo.Language.ToString()
                };

                var context = await _browser.NewContextAsync(contextOptions);
                var page = await context.NewPageAsync();

                try
                {
                    await page.GotoAsync("https://www.whatismybrowser.com/");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to navigate: {ex.Message}", "Navigation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // Keep the browser open
                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to launch browser: {ex.Message}", "Browser Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnLoadBrowser_Click(object sender, EventArgs e)
        {
            await InjectFingerprintIntoBrowser(_fingerprint);
        }

        private void btnRefreshFingerprint_Click(object sender, EventArgs e)
        {
            _fingerprint = FakeProfileFactory.Generate();
            _fingerprint.UserAgent = Generator.GenerateRandomUserAgent(_fingerprint.BrowserTypeType);
            DisplayFingerprint();
        }

        private void InitializeComponent()
        {
            this.txtFingerprint = new System.Windows.Forms.TextBox();
            this.btnLoadBrowser = new System.Windows.Forms.Button();
            this.btnRefreshFingerprint = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtFingerprint
            // 
            this.txtFingerprint.Location = new System.Drawing.Point(12, 12);
            this.txtFingerprint.Multiline = true;
            this.txtFingerprint.Name = "txtFingerprint";
            this.txtFingerprint.ReadOnly = true;
            this.txtFingerprint.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtFingerprint.Size = new System.Drawing.Size(260, 200); // Adjust size as needed
            this.txtFingerprint.TabIndex = 0;
            // 
            // btnLoadBrowser
            // 
            this.btnLoadBrowser.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnLoadBrowser.Location = new System.Drawing.Point(40, 220);
            this.btnLoadBrowser.Name = "btnLoadBrowser";
            this.btnLoadBrowser.Size = new System.Drawing.Size(200, 23);
            this.btnLoadBrowser.TabIndex = 1;
            this.btnLoadBrowser.Text = "Load Browser";
            this.btnLoadBrowser.UseVisualStyleBackColor = true;
            this.btnLoadBrowser.Click += new System.EventHandler(this.btnLoadBrowser_Click);
            // 
            // btnRefreshFingerprint
            // 
            this.btnRefreshFingerprint.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnRefreshFingerprint.Location = new System.Drawing.Point(40, 250);
            this.btnRefreshFingerprint.Name = "btnRefreshFingerprint";
            this.btnRefreshFingerprint.Size = new System.Drawing.Size(200, 23);
            this.btnRefreshFingerprint.TabIndex = 2;
            this.btnRefreshFingerprint.Text = "Refresh Fingerprint";
            this.btnRefreshFingerprint.UseVisualStyleBackColor = true;
            this.btnRefreshFingerprint.Click += new System.EventHandler(this.btnRefreshFingerprint_Click);
            // 
            // FingerprintPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 311); // Adjust size as needed
            this.Controls.Add(this.btnRefreshFingerprint);
            this.Controls.Add(this.btnLoadBrowser);
            this.Controls.Add(this.txtFingerprint);
            this.Name = "FingerprintPage";
            this.Text = "Fingerprint Page";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.TextBox txtFingerprint;
        private System.Windows.Forms.Button btnLoadBrowser;
        private System.Windows.Forms.Button btnRefreshFingerprint;
    }
}
