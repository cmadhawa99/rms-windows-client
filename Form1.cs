using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;

namespace LetterPortal
{
    public partial class Form1 : Form
    {
        private static readonly HttpClient Client = new()
        {
            Timeout = TimeSpan.FromSeconds(2)
        };

        private WebView2 _webView;
        private string _serverUrl;

        private readonly string _configFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LetterPortal");

        private string ConfigFilePath => Path.Combine(_configFolder, "server.txt");
        
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

            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            _webView = new WebView2
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(_webView);
            
            var menu = new MenuStrip();
            var settingsMenu = new ToolStripMenuItem("Settings");
            var changeServerItem = new ToolStripMenuItem("Change Server Address");
            changeServerItem.Click += (s, e) => ChangeServer();
            settingsMenu.DropDownItems.Add(changeServerItem);
            menu.Items.Add(settingsMenu);
            this.MainMenuStrip = menu;
            this.Controls.Add(menu);
        }

        private void ChangeServer()
        {
            var setupForm = new ServerSetupForm(_serverUrl);
            if (setupForm.ShowDialog() == DialogResult.OK)
            {
                _serverUrl = setupForm.ServerUrl;
                SaveServerUrl(_serverUrl);
                MessageBox.Show("The server address has been saved. Please close the application and reopen it.", "Saved");
            }
        }

        private string LoadSavedServerUrl()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string saved = File.ReadAllText(ConfigFilePath).Trim();
                    if (!string.IsNullOrWhiteSpace(saved))
                        return saved;
                }
            }
            catch { }
            return null;
        }

        private void SaveServerUrl(string url)
        {
            try
            {
                if (!Directory.Exists(_configFolder))
                    Directory.CreateDirectory(_configFolder);
                File.WriteAllText(ConfigFilePath, url);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save the server address: " + ex.Message, "Save Error");
            }
        }

        private async void InitializeApp()
        {

            _serverUrl = LoadSavedServerUrl();


            if (string.IsNullOrWhiteSpace(_serverUrl))
            {
                var setupForm = new ServerSetupForm();
                if (setupForm.ShowDialog() != DialogResult.OK)
                {
                    Application.Exit();
                    return;
                }
                _serverUrl = setupForm.ServerUrl;
                SaveServerUrl(_serverUrl);
            }


            if (!await IsServerRunning(_serverUrl))
            {
                var result = MessageBox.Show(
                    $"Could not connect to the Server at:\n{_serverUrl}\n\nIs the server address incorrect? Do you want to change it?",
                    "Connection Failed", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                if (result == DialogResult.Yes)
                {
                    var setupForm = new ServerSetupForm(_serverUrl);
                    if (setupForm.ShowDialog() == DialogResult.OK)
                    {
                        _serverUrl = setupForm.ServerUrl;
                        SaveServerUrl(_serverUrl);
                        InitializeApp(); 
                        return;
                    }
                }

                Application.Exit();
                return;
            }

            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "LetterPortal", "WebView2Data");

            var env = await CoreWebView2Environment.CreateAsync(null, appDataPath);
            await _webView.EnsureCoreWebView2Async(env);
            _webView.CoreWebView2.Navigate(_serverUrl);
        }

        private async Task<bool> IsServerRunning(string url)
        {
            int maxRetries = 5;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var response = await Client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                    await Task.Delay(1000);
                }
            }
            return false;
        }
    }
}