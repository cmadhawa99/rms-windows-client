using System;
using System.Windows.Forms;

namespace LetterPortal
{
    public class ServerSetupForm : Form
    {
        private TextBox _txtUrl;
        private Button _btnOk;
        public string ServerUrl { get; private set; }

        public ServerSetupForm(string existingUrl = "")
        {
            this.Text = "Server Setup";
            this.Width = 450;
            this.Height = 180;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lbl = new Label
            {
                Text = "Enter the server address...",
                Left = 20,
                Top = 15,
                Width = 400,
                Height = 40
            };

            _txtUrl = new TextBox
            {
                Left = 20,
                Top = 60,
                Width = 390,
                Text = existingUrl
            };

            _btnOk = new Button
            {
                Text = "Connect",
                Left = 20,
                Top = 95,
                Width = 100,
                Height = 30
            };
            _btnOk.Click += BtnOk_Click;

            this.Controls.Add(lbl);
            this.Controls.Add(_txtUrl);
            this.Controls.Add(_btnOk);
            this.AcceptButton = _btnOk;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            string input = _txtUrl.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("Please Eeter the server address.", "Required Field");
                return;
            }


            if (!input.StartsWith("http://") && !input.StartsWith("https://"))
            {
                input = "https://" + input;
            }

            ServerUrl = input.TrimEnd('/');
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}