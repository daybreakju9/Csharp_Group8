using System.Drawing;
using System.Linq;
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

            // 初始不显示侧边导航（符合需求2）
            if (panelNav != null)
                panelNav.Visible = false;

            // 响应尺寸变化以便居中欢迎页按钮和调整侧栏按钮宽度
            this.Resize += MainForm_Resize;
            panelMain.SizeChanged += (s, e) => CenterWelcomeButtons();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // 初始化导航管理器（panelMain 已在 Designer 中定义）
            _navigationManager = new NavigationManager(this, panelMain, lblStatus);

            // 默认隐藏侧边导航（不在登录后立刻显示）
            if (panelNav != null)
                panelNav.Visible = false;

            // 设置侧边栏按钮外观（字体加粗、颜色更柔和，文字居中）
            UpdateNavButtonsAppearance();

            // 设置用户信息
            if (_authService?.CurrentUser != null)
            {
                lblUser.Text = $"当前用户: {_authService.CurrentUser.Username} ({GetRoleDisplayName(_authService.CurrentUser.Role)})";

                // 游客可以进入项目，但不能执行管理/新建操作（需求1）
                btnUserEntry.Enabled = true;

                // 管理员专属入口可见性
                btnAdminEntry.Visible = _authService.IsAdmin;
                menuAdmin.Visible = _authService.IsAdmin;

                // 如果是管理员，不显示“用户功能”的“项目列表”在侧栏（需求1）
                if (_authService.IsAdmin)
                {
                    if (navBtnProjects != null) navBtnProjects.Visible = false;

                    // 管理员时显示侧边管理按钮（当侧栏显示时）
                    if (navBtnAdminProjects != null) navBtnAdminProjects.Visible = true;
                    if (navBtnAdminQueues != null) navBtnAdminQueues.Visible = true;
                    if (navBtnAdminUsers != null) navBtnAdminUsers.Visible = true;
                    if (navBtnAdminExport != null) navBtnAdminExport.Visible = true;
                }
                else
                {
                    // 非管理员确保项目按钮可见
                    if (navBtnProjects != null) navBtnProjects.Visible = true;
                }
            }

            // 初次布局：居中欢迎页按钮
            CenterWelcomeButtons();
        }

        // ========== 导航控制（当用户点击进入功能时显示侧栏） ==========
        private void ShowNav()
        {
            if (panelNav != null && !panelNav.Visible)
            {
                panelNav.Visible = true;
                // 调整按钮宽度以匹配侧栏
                UpdateNavButtonWidths();

                // 自动设定当前激活项（如果没有）为第一个可见按钮
                var first = flowLayoutPanelNav?.Controls.OfType<Button>().FirstOrDefault(b => b.Visible);
                if (first != null)
                    SetActiveNav(first);
            }
        }

        private void HideNav()
        {
            if (panelNav != null && panelNav.Visible)
            {
                panelNav.Visible = false;
                // 居中欢迎区按钮
                CenterWelcomeButtons();
            }
        }

        // 当用户点击“进入用户功能”时，显示导航并导航到项目列表
        private void btnUserEntry_Click(object sender, EventArgs e)
        {
            // 游客也允许进入项目（需求1）
            ShowNav();
            // 等待一次布局调整，然后导航
            Application.DoEvents();
            // 设置激活状态
            if (navBtnProjects != null && navBtnProjects.Visible)
                SetActiveNav(navBtnProjects);

            menuProjects_Click(sender, e);
        }

        // 管理入口：显示侧栏并展示管理员快捷菜单（或直接导航）
        private void btnAdminEntry_Click(object sender, EventArgs e)
        {
            if (!_authService.IsAdmin)
            {
                // 非管理员不显示管理入口（防御性检查）
                return;
            }

            ShowNav();

            // 优先将第一个管理员按钮设为激活（视觉反馈）
            if (navBtnAdminProjects != null && navBtnAdminProjects.Visible)
                SetActiveNav(navBtnAdminProjects);

            // 如果有快捷菜单项则显示菜单，否则直接打开管理页
            if (_adminShortcutMenu.Items.Count == 0)
            {
                menuAdminProjects_Click(sender, e);
                return;
            }

            var location = new Point(0, btnAdminEntry.Height);
            _adminShortcutMenu.Show(btnAdminEntry, location);
        }

        // 菜单导航：点击项目菜单也应显示侧栏并设置激活项
        private void menuProjects_Click(object sender, EventArgs e)
        {
            ShowNav();
            if (navBtnProjects != null && navBtnProjects.Visible) SetActiveNav(navBtnProjects);
            _navigationManager.NavigateToRoot(new ProjectListForm());
        }

        private void menuAdminProjects_Click(object sender, EventArgs e)
        {
            ShowNav();
            if (navBtnAdminProjects != null && navBtnAdminProjects.Visible) SetActiveNav(navBtnAdminProjects);
            _navigationManager.NavigateToRoot(new ProjectManagementForm());
        }

        private void menuAdminQueues_Click(object sender, EventArgs e)
        {
            ShowNav();
            if (navBtnAdminQueues != null && navBtnAdminQueues.Visible) SetActiveNav(navBtnAdminQueues);
            _navigationManager.NavigateToRoot(new QueueManagementForm());
        }

        private void menuAdminUsers_Click(object sender, EventArgs e)
        {
            ShowNav();
            if (navBtnAdminUsers != null && navBtnAdminUsers.Visible) SetActiveNav(navBtnAdminUsers);
            _navigationManager.NavigateToRoot(new UserManagementForm());
        }

        private void menuAdminExport_Click(object sender, EventArgs e)
        {
            ShowNav();
            if (navBtnAdminExport != null && navBtnAdminExport.Visible) SetActiveNav(navBtnAdminExport);
            _navigationManager.NavigateToRoot(new DataExportForm());
        }

        // ========== 布局与外观调整 ==========
        private void MainForm_Resize(object? sender, EventArgs e)
        {
            // 当窗体变化时，调整侧栏按钮宽度与欢迎页按钮居中
            UpdateNavButtonWidths();
            CenterWelcomeButtons();
        }

        private void UpdateNavButtonsAppearance()
        {
            // 统一字体：加粗并增大一、两个字号
            var navFont = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);

            // 非选中默认样式：白底黑字
            Color defaultBack = Color.White;
            Color defaultFore = Color.Black;

            // 激活时样式（用 UIConstants 主色的稍淡颜色）
            Color selectedBack = Color.FromArgb(64, 158, 255);
            Color selectedFore = Color.White;

            if (navBtnProjects != null)
            {
                navBtnProjects.Font = navFont;
                navBtnProjects.TextAlign = ContentAlignment.MiddleCenter;
                navBtnProjects.BackColor = defaultBack;
                navBtnProjects.ForeColor = defaultFore;
                navBtnProjects.FlatStyle = FlatStyle.Flat;
            }

            var adminButtons = new[] { navBtnAdminProjects, navBtnAdminQueues, navBtnAdminUsers, navBtnAdminExport };
            foreach (var b in adminButtons)
            {
                if (b != null)
                {
                    b.Font = navFont;
                    b.TextAlign = ContentAlignment.MiddleCenter;
                    b.BackColor = defaultBack;
                    b.ForeColor = defaultFore;
                    b.FlatStyle = FlatStyle.Flat;
                }
            }

            // 保证首次宽度正确
            UpdateNavButtonWidths();
        }

        private void UpdateNavButtonWidths()
        {
            if (panelNav == null || flowLayoutPanelNav == null) return;

            int innerWidth = panelNav.ClientSize.Width - panelNav.Padding.Left - panelNav.Padding.Right;
            // flowLayoutPanelNav 里每个按钮宽度设为 innerWidth - margin
            foreach (Control c in flowLayoutPanelNav.Controls)
            {
                if (c is Button btn)
                {
                    btn.Width = Math.Max(80, innerWidth - btn.Margin.Left - btn.Margin.Right);
                }
            }
        }

        // 设置侧栏激活按钮视觉状态（需求2）
        private void SetActiveNav(Button active)
        {
            if (flowLayoutPanelNav == null) return;

            Color defaultBack = Color.White;
            Color defaultFore = Color.Black;
            Color selectedBack = Color.FromArgb(64, 158, 255);
            Color selectedFore = Color.White;

            foreach (Control c in flowLayoutPanelNav.Controls)
            {
                if (c is Button b)
                {
                    // 重置到默认状态
                    b.BackColor = defaultBack;
                    b.ForeColor = defaultFore;
                }
            }

            // 应用激活样式
            active.BackColor = selectedBack;
            active.ForeColor = selectedFore;
            active.Font = new Font(active.Font.FontFamily, active.Font.Size, FontStyle.Bold);
        }

        // 居中欢迎页按钮（当侧栏隐藏时按钮应居中）
        private void CenterWelcomeButtons()
        {
            if (panelWelcome == null) return;

            // 如果侧栏可见，则保留 Designer 布局（不做改变）
            if (panelNav != null && panelNav.Visible)
            {
                return;
            }

            // 侧栏隐藏：计算居中布局
            var visibleButtons = new List<Button>();
            if (btnUserEntry != null && btnUserEntry.Visible) visibleButtons.Add(btnUserEntry);
            if (btnAdminEntry != null && btnAdminEntry.Visible) visibleButtons.Add(btnAdminEntry);

            int spacing = UIConstants.Spacing.Large;
            int totalWidth = visibleButtons.Sum(b => b.Width) + Math.Max(0, (visibleButtons.Count - 1) * spacing);
            int startX = Math.Max(0, (panelWelcome.ClientSize.Width - totalWidth) / 2);

            int y = Math.Max(UIConstants.Spacing.Large, btnUserEntry?.Location.Y ?? 40);
            foreach (var btn in visibleButtons)
            {
                btn.Location = new Point(startX, y);
                startX += btn.Width + spacing;
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
