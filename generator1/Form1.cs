using System;
using System.Text;
using System.Windows.Forms;
using generator1.Core;

namespace generator1
{
    public partial class Generator : Form
    {
        public Generator()
        {
            InitializeComponent();
        }

        private void btnGenerateFingerprint_Click(object sender, EventArgs e)
        {
            var fingerprint = FakeProfileFactory.Generate();
            fingerprint.UserAgent = GenerateRandomUserAgent(fingerprint.BrowserTypeType);
            var fingerprintPage = new FingerprintPage(fingerprint);
            fingerprintPage.Show();
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

        public static string GenerateRandomUserAgent(EBrowserType browserType)
        {
            var userAgents = new System.Collections.Generic.List<string>();

            switch (browserType)
            {
                case EBrowserType.Chrome:
                    userAgents = new System.Collections.Generic.List<string>
                    {
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.5938.122 Safari/537.36",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.5845.180 Safari/537.36",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.5790.170 Safari/537.36"
                    };
                    break;
                case EBrowserType.Firefox:
                    userAgents = new System.Collections.Generic.List<string>
                    {
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:117.0) Gecko/20100101 Firefox/117.0",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:116.0) Gecko/20100101 Firefox/116.0",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:115.0) Gecko/20100101 Firefox/115.0"
                    };
                    break;
                case EBrowserType.Webkit:
                    userAgents = new System.Collections.Generic.List<string>
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
