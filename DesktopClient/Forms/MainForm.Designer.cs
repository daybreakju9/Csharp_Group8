namespace ImageAnnotationApp.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        // 新增：左侧导航控件（在 Designer 中声明，以便不再依赖运行时创建）
        private System.Windows.Forms.Panel panelNav;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelNav;
        private System.Windows.Forms.Button navBtnProjects;
        private System.Windows.Forms.Button navBtnAdminProjects;
        private System.Windows.Forms.Button navBtnAdminQueues;
        private System.Windows.Forms.Button navBtnAdminUsers;
        private System.Windows.Forms.Button navBtnAdminExport;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.menuUser = new System.Windows.Forms.ToolStripMenuItem();
            this.menuProjects = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAdmin = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAdminProjects = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAdminQueues = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAdminUsers = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAdminExport = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAccount = new System.Windows.Forms.ToolStripMenuItem();
            this.menuLogout = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblUser = new System.Windows.Forms.ToolStripStatusLabel();
            this.panelMain = new System.Windows.Forms.Panel();
            this.panelWelcome = new System.Windows.Forms.Panel();
            this.btnAdminEntry = new System.Windows.Forms.Button();
            this.btnUserEntry = new System.Windows.Forms.Button();
            this.lblWelcome = new System.Windows.Forms.Label();

            // 新增导航控件初始化
            this.panelNav = new System.Windows.Forms.Panel();
            this.flowLayoutPanelNav = new System.Windows.Forms.FlowLayoutPanel();
            this.navBtnProjects = new System.Windows.Forms.Button();
            this.navBtnAdminProjects = new System.Windows.Forms.Button();
            this.navBtnAdminQueues = new System.Windows.Forms.Button();
            this.navBtnAdminUsers = new System.Windows.Forms.Button();
            this.navBtnAdminExport = new System.Windows.Forms.Button();

            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.panelMain.SuspendLayout();
            this.panelWelcome.SuspendLayout();
            this.panelNav.SuspendLayout();
            this.flowLayoutPanelNav.SuspendLayout();
            this.SuspendLayout();
            //
            // menuStrip
            //
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuUser,
            this.menuAdmin,
            this.menuAccount});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(1000, 25);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip1";
            //
            // menuUser
            //
            this.menuUser.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuProjects});
            this.menuUser.Name = "menuUser";
            this.menuUser.Size = new System.Drawing.Size(68, 21);
            this.menuUser.Text = "用户功能";
            //
            // menuProjects
            //
            this.menuProjects.Name = "menuProjects";
            this.menuProjects.Size = new System.Drawing.Size(124, 22);
            this.menuProjects.Text = "项目列表";
            this.menuProjects.Click += new System.EventHandler(this.menuProjects_Click);
            //
            // menuAdmin
            //
            this.menuAdmin.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuAdminProjects,
            this.menuAdminQueues,
            this.menuAdminUsers,
            this.menuAdminExport});
            this.menuAdmin.Name = "menuAdmin";
            this.menuAdmin.Size = new System.Drawing.Size(80, 21);
            this.menuAdmin.Text = "管理员功能";
            this.menuAdmin.Visible = false;
            //
            // menuAdminProjects
            //
            this.menuAdminProjects.Name = "menuAdminProjects";
            this.menuAdminProjects.Size = new System.Drawing.Size(124, 22);
            this.menuAdminProjects.Text = "项目管理";
            this.menuAdminProjects.Click += new System.EventHandler(this.menuAdminProjects_Click);
            //
            // menuAdminQueues
            //
            this.menuAdminQueues.Name = "menuAdminQueues";
            this.menuAdminQueues.Size = new System.Drawing.Size(124, 22);
            this.menuAdminQueues.Text = "队列管理";
            this.menuAdminQueues.Click += new System.EventHandler(this.menuAdminQueues_Click);
            //
            // menuAdminUsers
            //
            this.menuAdminUsers.Name = "menuAdminUsers";
            this.menuAdminUsers.Size = new System.Drawing.Size(124, 22);
            this.menuAdminUsers.Text = "用户管理";
            this.menuAdminUsers.Click += new System.EventHandler(this.menuAdminUsers_Click);
            //
            // menuAdminExport
            //
            this.menuAdminExport.Name = "menuAdminExport";
            this.menuAdminExport.Size = new System.Drawing.Size(124, 22);
            this.menuAdminExport.Text = "数据导出";
            this.menuAdminExport.Click += new System.EventHandler(this.menuAdminExport_Click);
            //
            // menuAccount
            //
            this.menuAccount.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.menuAccount.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuLogout});
            this.menuAccount.Name = "menuAccount";
            this.menuAccount.Size = new System.Drawing.Size(44, 21);
            this.menuAccount.Text = "账户";
            //
            // menuLogout
            //
            this.menuLogout.Name = "menuLogout";
            this.menuLogout.Size = new System.Drawing.Size(124, 22);
            this.menuLogout.Text = "退出登录";
            this.menuLogout.Click += new System.EventHandler(this.menuLogout_Click);
            //
            // statusStrip
            //
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus,
            this.lblUser});
            this.statusStrip.Location = new System.Drawing.Point(0, 728);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1000, 22);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip1";
            //
            // lblStatus
            //
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(32, 17);
            this.lblStatus.Text = "就绪";
            //
            // lblUser
            //
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(953, 17);
            this.lblUser.Spring = true;
            this.lblUser.Text = "用户";
            this.lblUser.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // panelMain
            //
            this.panelMain.Controls.Add(this.panelWelcome);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 25);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(1000, 703);
            this.panelMain.TabIndex = 2;
            //
            // panelWelcome
            //
            this.panelWelcome.Controls.Add(this.btnAdminEntry);
            this.panelWelcome.Controls.Add(this.btnUserEntry);
            this.panelWelcome.Controls.Add(this.lblWelcome);
            this.panelWelcome.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelWelcome.Location = new System.Drawing.Point(0, 0);
            this.panelWelcome.Name = "panelWelcome";
            this.panelWelcome.Size = new System.Drawing.Size(1000, 703);
            this.panelWelcome.TabIndex = 1;
            //
            // btnAdminEntry
            //
            this.btnAdminEntry.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnAdminEntry.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnAdminEntry.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdminEntry.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnAdminEntry.ForeColor = System.Drawing.Color.White;
            this.btnAdminEntry.Location = new System.Drawing.Point(520, 390);
            this.btnAdminEntry.Name = "btnAdminEntry";
            this.btnAdminEntry.Size = new System.Drawing.Size(200, 60);
            this.btnAdminEntry.TabIndex = 2;
            this.btnAdminEntry.Text = "进入管理员功能";
            this.btnAdminEntry.UseVisualStyleBackColor = false;
            this.btnAdminEntry.Visible = false;
            this.btnAdminEntry.Click += new System.EventHandler(this.btnAdminEntry_Click);
            //
            // btnUserEntry
            //
            this.btnUserEntry.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnUserEntry.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnUserEntry.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUserEntry.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnUserEntry.ForeColor = System.Drawing.Color.White;
            this.btnUserEntry.Location = new System.Drawing.Point(280, 390);
            this.btnUserEntry.Name = "btnUserEntry";
            this.btnUserEntry.Size = new System.Drawing.Size(200, 60);
            this.btnUserEntry.TabIndex = 1;
            this.btnUserEntry.Text = "进入用户功能";
            this.btnUserEntry.UseVisualStyleBackColor = false;
            this.btnUserEntry.Click += new System.EventHandler(this.btnUserEntry_Click);
            //
            // lblWelcome
            //
            this.lblWelcome.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblWelcome.Font = new System.Drawing.Font("Microsoft YaHei UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblWelcome.Location = new System.Drawing.Point(0, 0);
            this.lblWelcome.Name = "lblWelcome";
            this.lblWelcome.Size = new System.Drawing.Size(1000, 300);
            this.lblWelcome.TabIndex = 0;
            this.lblWelcome.Text = "欢迎使用图片标注系统";
            this.lblWelcome.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // panelNav (新增左侧导航面板)
            //
            this.panelNav.BackColor = System.Drawing.Color.White;
            this.panelNav.Controls.Add(this.flowLayoutPanelNav);
            this.panelNav.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelNav.Location = new System.Drawing.Point(0, 25);
            this.panelNav.Name = "panelNav";
            this.panelNav.Padding = new System.Windows.Forms.Padding(8);
            this.panelNav.Size = new System.Drawing.Size(220, 703);
            this.panelNav.TabIndex = 10;
            //
            // flowLayoutPanelNav
            //
            this.flowLayoutPanelNav.AutoScroll = true;
            this.flowLayoutPanelNav.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelNav.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelNav.WrapContents = false;
            this.flowLayoutPanelNav.Location = new System.Drawing.Point(8, 8);
            this.flowLayoutPanelNav.Name = "flowLayoutPanelNav";
            this.flowLayoutPanelNav.Size = new System.Drawing.Size(204, 687);
            this.flowLayoutPanelNav.TabIndex = 0;
            //
            // navBtnProjects
            //
            this.navBtnProjects.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.navBtnProjects.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.navBtnProjects.ForeColor = System.Drawing.Color.White;
            this.navBtnProjects.Location = new System.Drawing.Point(3, 3);
            this.navBtnProjects.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
            this.navBtnProjects.Name = "navBtnProjects";
            this.navBtnProjects.Size = new System.Drawing.Size(190, 40);
            this.navBtnProjects.TabIndex = 0;
            this.navBtnProjects.Text = "项目列表";
            this.navBtnProjects.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.navBtnProjects.UseVisualStyleBackColor = false;
            this.navBtnProjects.Click += new System.EventHandler(this.menuProjects_Click);
            //
            // navBtnAdminProjects
            //
            this.navBtnAdminProjects.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.navBtnAdminProjects.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.navBtnAdminProjects.ForeColor = System.Drawing.Color.White;
            this.navBtnAdminProjects.Location = new System.Drawing.Point(3, 54);
            this.navBtnAdminProjects.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
            this.navBtnAdminProjects.Name = "navBtnAdminProjects";
            this.navBtnAdminProjects.Size = new System.Drawing.Size(190, 36);
            this.navBtnAdminProjects.TabIndex = 1;
            this.navBtnAdminProjects.Text = "项目管理";
            this.navBtnAdminProjects.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.navBtnAdminProjects.UseVisualStyleBackColor = false;
            this.navBtnAdminProjects.Visible = false;
            this.navBtnAdminProjects.Click += new System.EventHandler(this.menuAdminProjects_Click);
            //
            // navBtnAdminQueues
            //
            this.navBtnAdminQueues.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.navBtnAdminQueues.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.navBtnAdminQueues.ForeColor = System.Drawing.Color.White;
            this.navBtnAdminQueues.Location = new System.Drawing.Point(3, 98);
            this.navBtnAdminQueues.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
            this.navBtnAdminQueues.Name = "navBtnAdminQueues";
            this.navBtnAdminQueues.Size = new System.Drawing.Size(190, 36);
            this.navBtnAdminQueues.TabIndex = 2;
            this.navBtnAdminQueues.Text = "队列管理";
            this.navBtnAdminQueues.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.navBtnAdminQueues.UseVisualStyleBackColor = false;
            this.navBtnAdminQueues.Visible = false;
            this.navBtnAdminQueues.Click += new System.EventHandler(this.menuAdminQueues_Click);
            //
            // navBtnAdminUsers
            //
            this.navBtnAdminUsers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.navBtnAdminUsers.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.navBtnAdminUsers.ForeColor = System.Drawing.Color.White;
            this.navBtnAdminUsers.Location = new System.Drawing.Point(3, 142);
            this.navBtnAdminUsers.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
            this.navBtnAdminUsers.Name = "navBtnAdminUsers";
            this.navBtnAdminUsers.Size = new System.Drawing.Size(190, 36);
            this.navBtnAdminUsers.TabIndex = 3;
            this.navBtnAdminUsers.Text = "用户管理";
            this.navBtnAdminUsers.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.navBtnAdminUsers.UseVisualStyleBackColor = false;
            this.navBtnAdminUsers.Visible = false;
            this.navBtnAdminUsers.Click += new System.EventHandler(this.menuAdminUsers_Click);
            //
            // navBtnAdminExport
            //
            this.navBtnAdminExport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.navBtnAdminExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.navBtnAdminExport.ForeColor = System.Drawing.Color.White;
            this.navBtnAdminExport.Location = new System.Drawing.Point(3, 186);
            this.navBtnAdminExport.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
            this.navBtnAdminExport.Name = "navBtnAdminExport";
            this.navBtnAdminExport.Size = new System.Drawing.Size(190, 36);
            this.navBtnAdminExport.TabIndex = 4;
            this.navBtnAdminExport.Text = "数据导出";
            this.navBtnAdminExport.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.navBtnAdminExport.UseVisualStyleBackColor = false;
            this.navBtnAdminExport.Visible = false;
            this.navBtnAdminExport.Click += new System.EventHandler(this.menuAdminExport_Click);

            // 把导航按钮加到 flowLayoutPanel
            this.flowLayoutPanelNav.Controls.Add(this.navBtnProjects);
            this.flowLayoutPanelNav.Controls.Add(this.navBtnAdminProjects);
            this.flowLayoutPanelNav.Controls.Add(this.navBtnAdminQueues);
            this.flowLayoutPanelNav.Controls.Add(this.navBtnAdminUsers);
            this.flowLayoutPanelNav.Controls.Add(this.navBtnAdminExport);

            //
            // MainForm - Controls 顺序：menuStrip, statusStrip, panelNav, panelMain
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 750);

            // 先清理旧的 Controls 添加，按正确顺序添加避免遮盖
            this.Controls.Clear();
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.panelNav);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);

            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "图片标注系统";
            this.Load += new System.EventHandler(this.MainForm_Load);

            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.panelMain.ResumeLayout(false);
            this.panelWelcome.ResumeLayout(false);
            this.panelNav.ResumeLayout(false);
            this.flowLayoutPanelNav.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem menuUser;
        private System.Windows.Forms.ToolStripMenuItem menuProjects;
        private System.Windows.Forms.ToolStripMenuItem menuAdmin;
        private System.Windows.Forms.ToolStripMenuItem menuAdminProjects;
        private System.Windows.Forms.ToolStripMenuItem menuAdminQueues;
        private System.Windows.Forms.ToolStripMenuItem menuAdminUsers;
        private System.Windows.Forms.ToolStripMenuItem menuAdminExport;
        private System.Windows.Forms.ToolStripMenuItem menuAccount;
        private System.Windows.Forms.ToolStripMenuItem menuLogout;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblUser;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Panel panelWelcome;
        private System.Windows.Forms.Button btnAdminEntry;
        private System.Windows.Forms.Button btnUserEntry;
        private System.Windows.Forms.Label lblWelcome;
    }
}