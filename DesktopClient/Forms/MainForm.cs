using System;
using System.Collections.Generic;
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

        // 新增：表示当前左侧导航处于管理员视图（true）或用户视图（false）
        private bool _isAdminView = false;

        public NavigationManager Navigation => _navigationManager;

        public MainForm()
        {
            InitializeComponent();

            _authService = AuthService.Instance;
            _adminShortcutMenu = BuildAdminShortcutMenu();
            // Designer 中存在 btnAdminEntry，绑定上下文菜单
            if (btnAdminEntry != null)
                btnAdminEntry.ContextMenuStrip = _adminShortcutMenu;

            // 绑定左侧账户按钮行为（替代顶部账户）
            if (navBtnAccount != null)
            {
                navBtnAccount.Click += NavBtnAccount_Click;
            }

            // 初始不显示侧边导航（符合需求）
            if (panelNav != null)
                panelNav.Visible = false;

            // 响应尺寸变化以便居中欢迎页按钮和调整侧栏按钮宽度
            this.Resize += MainForm_Resize;
            if (panelMain != null)
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

                // 游客可以进入项目，但不能执行管理/新建操作（仍在各页面里限制）
                if (btnUserEntry != null) btnUserEntry.Enabled = true;

                // 管理员专属入口可见性（由侧栏按钮控制）
                if (btnAdminEntry != null) btnAdminEntry.Visible = _authService.IsAdmin;

                // 默认视图：普通用户视图（即使是管理员也先不切换到 admin view）
                _isAdminView = false;

                // 控制左侧切换按钮，仅管理员可见
                if (navBtnSwitchView != null)
                {
                    navBtnSwitchView.Visible = _authService.IsAdmin;
                    UpdateSwitchButtonText();
                }

                // 根据是否为管理员，设置初始侧栏项可见性（管理员初始以用户视图展示）
                UpdateNavForAdminView(_isAdminView);
            }

            // 初次布局：居中欢迎页按钮
            CenterWelcomeButtons();

            System.Diagnostics.Debug.WriteLine($"DEBUG: _authService.IsAdmin = {_authService?.IsAdmin}");
            if (navBtnSwitchView == null)
            {
                System.Diagnostics.Debug.WriteLine("DEBUG: navBtnSwitchView == null (Designer 未初始化此控件)");
            }
            else
            {
                // 强制设置停靠以防被覆盖/位置不当，并确保显示性同步
                navBtnSwitchView.Dock = DockStyle.Top;
                navBtnAccount.Dock = DockStyle.Bottom;
                navBtnSwitchView.Visible = _authService?.IsAdmin ?? false;
                navBtnSwitchView.BringToFront();
                System.Diagnostics.Debug.WriteLine($"DEBUG: navBtnSwitchView.Visible = {navBtnSwitchView.Visible}, Location={navBtnSwitchView.Location}, Size={navBtnSwitchView.Size}");
            }
        }

        // ========== Navigation show/hide + handlers ==========

        private void ShowNav()
        {
            if (panelNav != null && !panelNav.Visible)
            {
                panelNav.Visible = true;
                UpdateNavButtonWidths();

                // 自动设定当前激活项（如果没有）为第一个可见按钮
                var first = flowLayoutPanelNav?.Controls.OfType<Button>().FirstOrDefault(b => b.Visible);
                if (first != null)
                    SetActiveNav(first);
            }

            // 每次显示侧边导航时根据当前模式刷新按钮可见性
            UpdateNavForAdminView(_isAdminView);
        }

        private void HideNav()
        {
            if (panelNav != null && panelNav.Visible)
            {
                panelNav.Visible = false;
                CenterWelcomeButtons();
            }
        }

        private void btnUserEntry_Click(object? sender, EventArgs e)
        {
            // 游客也允许进入项目（需求1）
            // 进入用户功能时切换为用户视图（便于管理员调试）
            _isAdminView = false;
            UpdateNavForAdminView(_isAdminView);
            UpdateSwitchButtonText();

            ShowNav();
            Application.DoEvents();

            if (navBtnProjects != null && navBtnProjects.Visible)
                SetActiveNav(navBtnProjects);

            menuProjects_Click(sender, e);
        }

        private void btnAdminEntry_Click(object? sender, EventArgs e)
        {
            if (!_authService.IsAdmin) return;

            // 进入管理员功能时切换为管理员视图
            _isAdminView = true;
            UpdateNavForAdminView(_isAdminView);
            UpdateSwitchButtonText();

            // Show left navigation and mark admin "Project Management" as active,
            // then navigate directly to the ProjectManagementForm.
            ShowNav();
            if (navBtnAdminProjects != null && navBtnAdminProjects.Visible)
                SetActiveNav(navBtnAdminProjects);

            menuAdminProjects_Click(sender, e);
        }

        // 新增：左侧切换按钮事件（管理员可见）
        private void NavBtnSwitchView_Click(object? sender, EventArgs e)
        {
            if (!_authService.IsAdmin) return;

            // 切换模式
            _isAdminView = !_isAdminView;
            UpdateNavForAdminView(_isAdminView);
            UpdateSwitchButtonText();

            // 确保侧栏显示并更新激活项
            ShowNav();
            Application.DoEvents();

            // 确保 NavigationManager 已初始化
            if (_navigationManager == null)
            {
                System.Diagnostics.Debug.WriteLine("DEBUG: NavigationManager is null when switching views");
                return;
            }

            // 在 UI 线程执行导航，避免跨线程问题
            void DoNavigate()
            {
                if (_isAdminView)
                {
                    if (navBtnAdminProjects != null && navBtnAdminProjects.Visible)
                        SetActiveNav(navBtnAdminProjects);

                    // 直接导航到管理员的默认页面
                    _navigationManager.NavigateToRoot(new ProjectManagementForm());
                }
                else
                {
                    if (navBtnProjects != null && navBtnProjects.Visible)
                        SetActiveNav(navBtnProjects);

                    // 导航到用户默认页面（与游客/用户入口相同）
                    _navigationManager.NavigateToRoot(new ProjectListForm());
                }
            }

            if (this.InvokeRequired)
                this.Invoke((Action)DoNavigate);
            else
                DoNavigate();
        }

        private void menuProjects_Click(object? sender, EventArgs e)
        {
            // 切换为用户视图（当管理员通过侧边点击用户项时也要切换）
            _isAdminView = false;
            UpdateNavForAdminView(_isAdminView);
            UpdateSwitchButtonText();

            ShowNav();
            if (navBtnProjects != null && navBtnProjects.Visible) SetActiveNav(navBtnProjects);
            _navigationManager.NavigateToRoot(new ProjectListForm());
        }

        private void menuAdminProjects_Click(object? sender, EventArgs e)
        {
            // 切换为管理员视图
            _isAdminView = true;
            UpdateNavForAdminView(_isAdminView);
            UpdateSwitchButtonText();

            ShowNav();
            if (navBtnAdminProjects != null && navBtnAdminProjects.Visible) SetActiveNav(navBtnAdminProjects);
            _navigationManager.NavigateToRoot(new ProjectManagementForm());
        }

        private void menuAdminQueues_Click(object? sender, EventArgs e)
        {
            _isAdminView = true;
            UpdateNavForAdminView(_isAdminView);
            UpdateSwitchButtonText();

            ShowNav();
            if (navBtnAdminQueues != null && navBtnAdminQueues.Visible) SetActiveNav(navBtnAdminQueues);
            _navigationManager.NavigateToRoot(new QueueManagementForm());
        }

        private void menuAdminUsers_Click(object? sender, EventArgs e)
        {
            _isAdminView = true;
            UpdateNavForAdminView(_isAdminView);
            UpdateSwitchButtonText();

            ShowNav();
            if (navBtnAdminUsers != null && navBtnAdminUsers.Visible) SetActiveNav(navBtnAdminUsers);
            _navigationManager.NavigateToRoot(new UserManagementForm());
        }

        private void menuAdminExport_Click(object? sender, EventArgs e)
        {
            _isAdminView = true;
            UpdateNavForAdminView(_isAdminView);
            UpdateSwitchButtonText();

            ShowNav();
            if (navBtnAdminExport != null && navBtnAdminExport.Visible) SetActiveNav(navBtnAdminExport);
            _navigationManager.NavigateToRoot(new DataExportForm());
        }

        // ========== Layout / Styling helpers ==========

        private void MainForm_Resize(object? sender, EventArgs e)
        {
            UpdateNavButtonWidths();
            CenterWelcomeButtons();
        }

        private void UpdateNavButtonsAppearance()
        {
            var navFont = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
            Color defaultBack = Color.White;
            Color defaultFore = Color.Black;

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

            if (navBtnAccount != null)
            {
                navBtnAccount.Font = navFont;
                navBtnAccount.TextAlign = ContentAlignment.MiddleCenter;
                navBtnAccount.BackColor = defaultBack;
                navBtnAccount.ForeColor = defaultFore;
                navBtnAccount.FlatStyle = FlatStyle.Flat;
            }

            if (navBtnSwitchView != null)
            {
                navBtnSwitchView.Font = navFont;
                navBtnSwitchView.TextAlign = ContentAlignment.MiddleCenter;
                navBtnSwitchView.BackColor = defaultBack;
                navBtnSwitchView.ForeColor = defaultFore;
                navBtnSwitchView.FlatStyle = FlatStyle.Flat;
            }

            UpdateNavButtonWidths();
        }

        private void UpdateNavButtonWidths()
        {
            if (panelNav == null || flowLayoutPanelNav == null) return;

            int innerWidth = panelNav.ClientSize.Width - panelNav.Padding.Left - panelNav.Padding.Right;
            foreach (Control c in flowLayoutPanelNav.Controls)
            {
                if (c is Button btn)
                {
                    btn.Width = Math.Max(80, innerWidth - btn.Margin.Left - btn.Margin.Right);
                }
            }

            if (navBtnAccount != null && panelAccount != null)
            {
                navBtnAccount.Width = panelAccount.ClientSize.Width - navBtnAccount.Margin.Left - navBtnAccount.Margin.Right;
                navBtnSwitchView.Width = navBtnAccount.Width;
            }
        }

        // 新增：根据当前 _isAdminView 控制侧边栏显示的项（管理员可以在两种视图间切换）
        private void UpdateNavForAdminView(bool isAdminView)
        {
            if (_authService == null) return;

            // 只有管理员可以切换到管理员视图
            if (!_authService.IsAdmin)
            {
                // 非管理员只显示用户项
                if (navBtnProjects != null) navBtnProjects.Visible = true;
                var adminButtons = new[] { navBtnAdminProjects, navBtnAdminQueues, navBtnAdminUsers, navBtnAdminExport };
                foreach (var b in adminButtons) if (b != null) b.Visible = false;
                if (navBtnSwitchView != null) navBtnSwitchView.Visible = false;
                return;
            }

            // 管理员可见切换按钮
            if (navBtnSwitchView != null) navBtnSwitchView.Visible = true;

            if (isAdminView)
            {
                if (navBtnProjects != null) navBtnProjects.Visible = false;
                if (navBtnAdminProjects != null) navBtnAdminProjects.Visible = true;
                if (navBtnAdminQueues != null) navBtnAdminQueues.Visible = true;
                if (navBtnAdminUsers != null) navBtnAdminUsers.Visible = true;
                if (navBtnAdminExport != null) navBtnAdminExport.Visible = true;
            }
            else
            {
                if (navBtnProjects != null) navBtnProjects.Visible = true;
                if (navBtnAdminProjects != null) navBtnAdminProjects.Visible = false;
                if (navBtnAdminQueues != null) navBtnAdminQueues.Visible = false;
                if (navBtnAdminUsers != null) navBtnAdminUsers.Visible = false;
                if (navBtnAdminExport != null) navBtnAdminExport.Visible = false;
            }

            UpdateNavButtonWidths();
        }

        // 新增：更新切换按钮文本
        private void UpdateSwitchButtonText()
        {
            if (navBtnSwitchView == null) return;
            navBtnSwitchView.Text = _isAdminView ? "进入用户功能" : "进入管理员功能";
        }

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
                    b.BackColor = defaultBack;
                    b.ForeColor = defaultFore;
                }
            }

            active.BackColor = selectedBack;
            active.ForeColor = selectedFore;
            active.Font = new Font(active.Font.FontFamily, active.Font.Size, FontStyle.Bold);
        }

        private void CenterWelcomeButtons()
        {
            if (panelWelcome == null) return;

            if (panelNav != null && panelNav.Visible)
                return;

            var visibleButtons = new List<Button>();
            if (btnUserEntry != null && btnUserEntry.Visible) visibleButtons.Add(btnUserEntry);
            if (btnAdminEntry != null && btnAdminEntry.Visible) visibleButtons.Add(btnAdminEntry);

            int spacing = UIConstants.Spacing.Large * 2;
            int totalWidth = visibleButtons.Sum(b => b.Width) + Math.Max(0, (visibleButtons.Count - 1) * spacing);
            int startX = Math.Max(0, (panelWelcome.ClientSize.Width - totalWidth) / 2);

            int y = Math.Max(UIConstants.Spacing.Large, btnUserEntry?.Location.Y ?? 40);
            foreach (var btn in visibleButtons)
            {
                btn.Location = new Point(startX, y);
                startX += btn.Width + spacing;
            }
        }

        // ========== Account / Admin menu builders ==========

        private ContextMenuStrip BuildAdminShortcutMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("项目管理", null, (s, e) => menuAdminProjects_Click(s, e));
            menu.Items.Add("队列管理", null, (s, e) => menuAdminQueues_Click(s, e));
            menu.Items.Add("用户管理", null, (s, e) => menuAdminUsers_Click(s, e));
            menu.Items.Add("数据导出", null, (s, e) => menuAdminExport_Click(s, e));
            return menu;
        }

        private void NavBtnAccount_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "确定要退出登录并关闭程序吗？",
                "确认",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try { _authService.Logout(); } catch { }
                Application.Exit();
            }
        }

        // ========== Utilities ==========

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
            NavBtnAccount_Click(sender, e);
        }

        private void LoadForm(Form form)
        {
            _navigationManager?.NavigateTo(form, addToStack: false);
        }
    }
}
