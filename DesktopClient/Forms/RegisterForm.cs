using ImageAnnotationApp.Services;
using ImageAnnotationApp.Models;

namespace ImageAnnotationApp.Forms
{
    public partial class RegisterForm : Form
    {
        private readonly AuthService _authService;

        public RegisterForm()
        {
            InitializeComponent();
            _authService = AuthService.Instance;
        }

        private async void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();
            string confirm = txtConfirmPassword.Text.Trim();

            // --- 1. 基础空值校验 ---
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("请输入用户名", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("请输入密码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            if (password != confirm)
            {
                MessageBox.Show("两次输入的密码不一致", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmPassword.Focus();
                return;
            }

            // --- 2. 专业密码强度校验 ---
            if (password.Length < 6)
            {
                MessageBox.Show("密码长度至少需要 6 位。", "弱密码警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit))
            {
                MessageBox.Show("密码必须同时包含字母和数字，安全性更高。",
                    "密码强度不足", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // --- 3. 禁用按钮 ---
            btnRegister.Enabled = false;
            btnRegister.Text = "注册中...";

            try
            {
                var registerDto = new RegisterDto
                {
                    Username = username,
                    Password = password
                };

                var response = await _authService.RegisterAsync(registerDto);

                MessageBox.Show(
                    $"注册成功！您的账号: {response.Username}\n" +
                    $"角色: {(response.Role == "Guest" ? "游客（待审核）" : response.Role)}\n" +
                    "请等待管理员审核后方可使用完整功能。",
                    "成功",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"注册失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnRegister.Enabled = true;
                btnRegister.Text = "注册";
            }
        }


        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
