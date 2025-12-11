using ImageAnnotationApp.Services;
using ImageAnnotationApp.Models;

namespace ImageAnnotationApp.Forms
{
    public partial class LoginForm : Form
    {
        private readonly AuthService _authService;

        public LoginForm()
        {
            InitializeComponent();
            _authService = AuthService.Instance;
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("请输入用户名", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("请输入密码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            btnLogin.Enabled = false;
            btnLogin.Text = "登录中...";

            try
            {
                var loginDto = new LoginDto
                {
                    Username = txtUsername.Text.Trim(),
                    Password = txtPassword.Text
                };

                var response = await _authService.LoginAsync(loginDto);

                MessageBox.Show($"登录成功！欢迎回来，{response.Username}！", "成功",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 打开主窗体
                var mainForm = new MainForm();
                this.Hide();
                mainForm.FormClosed += (s, args) => this.Close();
                mainForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnLogin.Enabled = true;
                btnLogin.Text = "登录";
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            var registerForm = new RegisterForm();
            registerForm.ShowDialog();
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin_Click(sender, e);
            }
        }
    }
}
