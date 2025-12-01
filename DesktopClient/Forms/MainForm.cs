using System.Drawing;
using System.Windows.Forms;
using ImageAnnotationApp.Services;
using ImageAnnotationApp.Helpers;

namespace ImageAnnotationApp.Forms
{
    public partial class MainForm : Form
    {
        private readonly AuthService _authService;
        private readonly ContextMenuStrip _adminShortcutMenu;
        private NavigationManager _navigationManager = null!;

        public NavigationManager Navigation => _navigationManager;

        public MainForm()
        {
            InitializeComponent();
            _authService = AuthService.Instance;
            _adminShortcutMenu = BuildAdminShortcutMenu();
            btnAdminEntry.ContextMenuStrip = _adminShortcutMenu;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // 初始化导航管理器
            _navigationManager = new NavigationManager(this, panelMain, lblStatus);

            // 设置用户信息
            if (_authService.CurrentUser != null)
            {
                lblUser.Text = $"当前用户: {_authService.CurrentUser.Username} ({GetRoleDisplayName(_authService.CurrentUser.Role)})";

                btnUserEntry.Enabled = _authService.CurrentUser.Role != "Guest";
                btnAdminEntry.Visible = _authService.IsAdmin;

                // 根据角色显示菜单
                if (_authService.IsAdmin)
                {
                    menuAdmin.Visible = true;
                    btnAdminEntry.Visible = true;
                }
                else if (_authService.CurrentUser.Role == "Guest")
                {
                    menuUser.Enabled = false;
                    btnUserEntry.Enabled = false;
                    MessageBox.Show(
                        "您当前是游客身份，功能受限。\n请等待管理员审核后方可使用完整功能。",
                        "提示",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    btnAdminEntry.Visible = false;
                }
            }
        }

        private string GetRoleDisplayName(string role)
        {
            return role switch
            {
                "Admin" => "管理员",
                "User" => "普通用户",
                "Guest" => "游客（待审核）",
                _ => role
            };
        }

        private void menuProjects_Click(object sender, EventArgs e)
        {
            _navigationManager.NavigateToRoot(new ProjectListForm());
        }

        private void menuAdminProjects_Click(object sender, EventArgs e)
        {
            _navigationManager.NavigateToRoot(new ProjectManagementForm());
        }

        private void menuAdminQueues_Click(object sender, EventArgs e)
        {
            _navigationManager.NavigateToRoot(new QueueManagementForm());
        }

        private void menuAdminUsers_Click(object sender, EventArgs e)
        {
            _navigationManager.NavigateToRoot(new UserManagementForm());
        }

        private void menuAdminExport_Click(object sender, EventArgs e)
        {
            _navigationManager.NavigateToRoot(new DataExportForm());
        }

        private void btnUserEntry_Click(object sender, EventArgs e)
        {
            menuProjects_Click(sender, e);
        }

        private void btnAdminEntry_Click(object sender, EventArgs e)
        {
            if (_adminShortcutMenu.Items.Count == 0)
            {
                menuAdminProjects_Click(sender, e);
                return;
            }

            var location = new Point(0, btnAdminEntry.Height);
            _adminShortcutMenu.Show(btnAdminEntry, location);
        }

        private void menuLogout_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "确定要退出登录吗？",
                "确认",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _authService.Logout();

                // 返回登录界面
                var loginForm = new LoginForm();
                loginForm.Show();
                this.Close();
            }
        }

        private void LoadForm(Form form)
        {
            // 已废弃，请使用 NavigationManager.NavigateTo 或 NavigateToRoot
            _navigationManager?.NavigateTo(form, addToStack: false);
        }

        private ContextMenuStrip BuildAdminShortcutMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("项目管理", null, (s, e) => menuAdminProjects_Click(s, e));
            menu.Items.Add("队列管理", null, (s, e) => menuAdminQueues_Click(s, e));
            menu.Items.Add("用户管理", null, (s, e) => menuAdminUsers_Click(s, e));
            menu.Items.Add("数据导出", null, (s, e) => menuAdminExport_Click(s, e));
            return menu;
        }
    }
}
