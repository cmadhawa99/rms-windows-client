using System;
using System.Net.Http;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using System.IO;

namespace LetterPortal
{
    public partial class Form1 : Form
    {
        private WebView2 webView;
        private const string ServerUrl = "http://DESKTOP-NHOO4UE:8000";

        public Form1()
        {
            InitializeComponent();
            SetupWindow();
            InitializeApp();
        }

        private void SetupWindow()
        {
            this.Text = "Letter Management Portal";
            this.Width = 1200;
            this.Height = 800;
            this.StartPosition = FormStartPosition.CenterScreen;
            
            this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            
            webView = new WebView2
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(webView);
        }

        private async void InitializeApp()
        {
            if (!await IsServerRunning(ServerUrl))
            {
                MessageBox.Show(
                    $"Could not connect to the Server at:\n{ServerUrl}\n\nPlease ensure the main server is turned on and try again.", 
                    "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }
            
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LetterPortal");
            var env = await CoreWebView2Environment.CreateAsync(null, appDataPath);
            await webView.EnsureCoreWebView2Async(env);
            webView.CoreWebView2.Navigate(ServerUrl);
        }

        private async System.Threading.Tasks.Task<bool> IsServerRunning(string url)
        {
            using var client = new HttpClient();
            
            client.Timeout = TimeSpan.FromSeconds(2); 
    
            int maxRetries = 5; 

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var response = await client.GetAsync(url);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        return true; 
                    }
                }
                catch (Exception)
                {

                    await System.Threading.Tasks.Task.Delay(1000); 
                }
            }
            MessageBox.Show("Could not reach the server after 5 attempts. Please check if the port is blocked.", "Connection Timeout");
            return false;
        }
    }
}