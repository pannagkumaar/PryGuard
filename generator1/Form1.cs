using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using generator1.Core;
using Microsoft.Playwright;

namespace generator1
{
    public partial class Generator : Form
    {
        private IPlaywright _playwright;
        private IBrowser _browser;

        public Generator()
        {
            InitializeComponent();
        }

        private async void btnGenerateFingerprint_Click(object sender, EventArgs e)
        {
            var fingerprint = FakeProfileFactory.Generate();
            fingerprint.UserAgent = GenerateRandomUserAgent(fingerprint.BrowserTypeType);
            lblFingerprint.Text = $"Generated Fingerprint: {fingerprint}";
            await InjectFingerprintIntoBrowser(fingerprint);
        }

        private void btnGenerateRandomEmail_Click(object sender, EventArgs e)
        {
            string randomEmail = GenerateRandomEmail();
            lblRandomEmail.Text = $"Generated Random Email: {randomEmail}";
        }

        private static string GenerateRandomEmail()
        {
            string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            StringBuilder localPart = new StringBuilder();
            for (int i = 0; i < 10; i++)
            {
                localPart.Append(chars[random.Next(chars.Length)]);
            }

            string hashedLocalPart = GetMd5Hash(localPart.ToString());
            string domain = "demo.com";
            return $"{hashedLocalPart}@{domain}";
        }

        private static string GetMd5Hash(string input)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString().ToLower();
            }
        }

        private async Task InjectFingerprintIntoBrowser(FakeProfile fingerprint)
        {
            _playwright = await Playwright.CreateAsync();
            switch (fingerprint.BrowserTypeType)
            {
                case EBrowserType.Chrome:
                    _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Headless = false,
                        ExecutablePath = @"C:\Users\panna\AppData\Local\ms-playwright\chromium-1124\chrome-win\chrome.exe"
                    });
                    break;
                case EBrowserType.Firefox:
                    _browser = await _playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Headless = false,
                        ExecutablePath = @"C:\Users\panna\AppData\Local\ms-playwright\firefox-1454\firefox\firefox.exe"
                    });
                    break;
                case EBrowserType.Webkit:
                    _browser = await _playwright.Webkit.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Headless = false,
                        ExecutablePath = @"C:\Users\panna\AppData\Local\ms-playwright\webkit-2035\Playwright.exe"
                    });
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

            await page.GotoAsync("https://www.whatismybrowser.com/");

            // Keep the browser open
            await Task.Delay(-1);
        }

        private string GenerateRandomUserAgent(EBrowserType browserType)
        {
            var userAgents = new List<string>();

            switch (browserType)
            {
                case EBrowserType.Chrome:
                    userAgents = new List<string>
                    {
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.5938.122 Safari/537.36",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.5845.180 Safari/537.36",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.5790.170 Safari/537.36"
                    };
                    break;
                case EBrowserType.Firefox:
                    userAgents = new List<string>
                    {
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:117.0) Gecko/20100101 Firefox/117.0",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:116.0) Gecko/20100101 Firefox/116.0",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:115.0) Gecko/20100101 Firefox/115.0"
                    };
                    break;
                case EBrowserType.Webkit:
                    userAgents = new List<string>
                    {
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.1 Safari/605.1.15",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.1.2 Safari/605.1.15",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.1.2 Safari/605.1.15"
                    };
                    break;
            }

            Random random = new Random();
            int index = random.Next(userAgents.Count);
            return userAgents[index];
        }
    }
}
