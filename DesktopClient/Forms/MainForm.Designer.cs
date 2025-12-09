namespace ImageAnnotationApp.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        // 左侧导航控件（Designer 声明）
        private System.Windows.Forms.Panel panelNav;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelNav;
        private System.Windows.Forms.Button navBtnProjects;
        private System.Windows.Forms.Button navBtnAdminProjects;
        private System.Windows.Forms.Button navBtnAdminQueues;
        private System.Windows.Forms.Button navBtnAdminUsers;
        private System.Windows.Forms.Button navBtnAdminExport;
        private System.Windows.Forms.Panel panelAccount; // 底部账户面板
        private System.Windows.Forms.Button navBtnAccount;

        // 新增：管理员视图切换按钮（显示在底部账户上方）
        private System.Windows.Forms.Button navBtnSwitchView;

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
            this.panelAccount = new System.Windows.Forms.Panel();
            this.navBtnAccount = new System.Windows.Forms.Button();

            // 新增切换视图按钮（在 panelAccount 顶部）
            this.navBtnSwitchView = new System.Windows.Forms.Button();

            this.statusStrip.SuspendLayout();
            this.panelMain.SuspendLayout();
            this.panelWelcome.SuspendLayout();
            this.panelNav.SuspendLayout();
            this.flowLayoutPanelNav.SuspendLayout();
            this.panelAccount.SuspendLayout();
            this.SuspendLayout();

            //
            // statusStrip
            //
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus,
            this.lblUser});
            this.statusStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1000, 22);
            this.statusStrip.TabIndex = 1;
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
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.TabIndex = 2;
            //
            // panelWelcome
            //
            this.panelWelcome.Controls.Add(this.btnAdminEntry);
            this.panelWelcome.Controls.Add(this.btnUserEntry);
            this.panelWelcome.Controls.Add(this.lblWelcome);
            this.panelWelcome.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelWelcome.Name = "panelWelcome";
            this.panelWelcome.TabIndex = 1;
            //
            // btnAdminEntry
            //
            this.btnAdminEntry.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnAdminEntry.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnAdminEntry.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdminEntry.FlatAppearance.BorderSize = 0;
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
            this.btnUserEntry.FlatAppearance.BorderSize = 0;
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
            // panelNav (左侧导航面板)
            //
            this.panelNav.BackColor = System.Drawing.Color.White;
            // 顺序：flowLayoutPanelNav (Fill) + panelAccount (Bottom)
            this.panelNav.Controls.Add(this.flowLayoutPanelNav);
            this.panelNav.Controls.Add(this.panelAccount);
            this.panelNav.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelNav.Name = "panelNav";
            this.panelNav.Padding = new System.Windows.Forms.Padding(8);
            this.panelNav.Size = new System.Drawing.Size(220, 0); // 高度由 Dock 与父容器决定
            this.panelNav.TabIndex = 10;
            //
            // flowLayoutPanelNav
            //
            this.flowLayoutPanelNav.AutoScroll = true;
            this.flowLayoutPanelNav.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelNav.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelNav.WrapContents = false;
            this.flowLayoutPanelNav.Name = "flowLayoutPanelNav";
            this.flowLayoutPanelNav.TabIndex = 0;
            //
            // navBtnProjects
            //
            this.navBtnProjects.BackColor = System.Drawing.Color.White;
            this.navBtnProjects.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.navBtnProjects.ForeColor = System.Drawing.Color.Black;
            this.navBtnProjects.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
            this.navBtnProjects.Name = "navBtnProjects";
            this.navBtnProjects.Size = new System.Drawing.Size(190, 40);
            this.navBtnProjects.TabIndex = 0;
            this.navBtnProjects.Text = "项目列表";
            this.navBtnProjects.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.navBtnProjects.UseVisualStyleBackColor = false;
            this.navBtnProjects.Click += new System.EventHandler(this.menuProjects_Click);
            //
            // navBtnAdminProjects
            //
            this.navBtnAdminProjects.BackColor = System.Drawing.Color.White;
            this.navBtnAdminProjects.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.navBtnAdminProjects.ForeColor = System.Drawing.Color.Black;
            this.navBtnAdminProjects.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
            this.navBtnAdminProjects.Name = "navBtnAdminProjects";
            this.navBtnAdminProjects.Size = new System.Drawing.Size(190, 36);
            this.navBtnAdminProjects.TabIndex = 1;
            this.navBtnAdminProjects.Text = "项目管理";
            this.navBtnAdminProjects.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.navBtnAdminProjects.UseVisualStyleBackColor = false;
            this.navBtnAdminProjects.Visible = false;
            this.navBtnAdminProjects.Click += new System.EventHandler(this.menuAdminProjects_Click);
            //
            // navBtnAdminQueues
            //
            this.navBtnAdminQueues.BackColor = System.Drawing.Color.White;
            this.navBtnAdminQueues.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.navBtnAdminQueues.ForeColor = System.Drawing.Color.Black;
            this.navBtnAdminQueues.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
            this.navBtnAdminQueues.Name = "navBtnAdminQueues";
            this.navBtnAdminQueues.Size = new System.Drawing.Size(190, 36);
            this.navBtnAdminQueues.TabIndex = 2;
            this.navBtnAdminQueues.Text = "队列管理";
            this.navBtnAdminQueues.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.navBtnAdminQueues.UseVisualStyleBackColor = false;
            this.navBtnAdminQueues.Visible = false;
            this.navBtnAdminQueues.Click += new System.EventHandler(this.menuAdminQueues_Click);
            //
            // navBtnAdminUsers
            //
            this.navBtnAdminUsers.BackColor = System.Drawing.Color.White;
            this.navBtnAdminUsers.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.navBtnAdminUsers.ForeColor = System.Drawing.Color.Black;
            this.navBtnAdminUsers.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
            this.navBtnAdminUsers.Name = "navBtnAdminUsers";
            this.navBtnAdminUsers.Size = new System.Drawing.Size(190, 36);
            this.navBtnAdminUsers.TabIndex = 3;
            this.navBtnAdminUsers.Text = "用户管理";
            this.navBtnAdminUsers.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.navBtnAdminUsers.UseVisualStyleBackColor = false;
            this.navBtnAdminUsers.Visible = false;
            this.navBtnAdminUsers.Click += new System.EventHandler(this.menuAdminUsers_Click);
            //
            // navBtnAdminExport
            //
            this.navBtnAdminExport.BackColor = System.Drawing.Color.White;
            this.navBtnAdminExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.navBtnAdminExport.ForeColor = System.Drawing.Color.Black;
            this.navBtnAdminExport.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
            this.navBtnAdminExport.Name = "navBtnAdminExport";
            this.navBtnAdminExport.Size = new System.Drawing.Size(190, 36);
            this.navBtnAdminExport.TabIndex = 4;
            this.navBtnAdminExport.Text = "数据导出";
            this.navBtnAdminExport.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.navBtnAdminExport.UseVisualStyleBackColor = false;
            this.navBtnAdminExport.Visible = false;
            this.navBtnAdminExport.Click += new System.EventHandler(this.menuAdminExport_Click);
            //
            // panelAccount (底部账户区)
            //
            this.panelAccount.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelAccount.Name = "panelAccount";
            this.panelAccount.Size = new System.Drawing.Size(204, 72); // 增高以容纳切换按钮 + 账户按钮
            this.panelAccount.TabIndex = 11;
            //
            // navBtnSwitchView (管理员切换按钮)
            //
            this.navBtnSwitchView.BackColor = System.Drawing.Color.White;
            this.navBtnSwitchView.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.navBtnSwitchView.ForeColor = System.Drawing.Color.Black;
            this.navBtnSwitchView.Margin = new System.Windows.Forms.Padding(3, 2, 3, 4);
            this.navBtnSwitchView.Name = "navBtnSwitchView";
            this.navBtnSwitchView.Size = new System.Drawing.Size(204, 36);
            this.navBtnSwitchView.TabIndex = 99;
            this.navBtnSwitchView.Text = "切换视图";
            this.navBtnSwitchView.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.navBtnSwitchView.UseVisualStyleBackColor = false;
            this.navBtnSwitchView.Visible = false; // 默认不显示，登录后根据权限控制
            this.navBtnSwitchView.Click += new System.EventHandler(this.NavBtnSwitchView_Click);

            //
            // navBtnAccount (与导航按钮样式一致，较矮)
            //
            this.navBtnAccount.BackColor = System.Drawing.Color.White;
            this.navBtnAccount.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.navBtnAccount.ForeColor = System.Drawing.Color.Black;
            this.navBtnAccount.Margin = new System.Windows.Forms.Padding(3, 2, 3, 8);
            this.navBtnAccount.Name = "navBtnAccount";
            this.navBtnAccount.Size = new System.Drawing.Size(204, 36);
            this.navBtnAccount.TabIndex = 0;
            this.navBtnAccount.Text = "账户";
            this.navBtnAccount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.navBtnAccount.UseVisualStyleBackColor = false;

            // 把导航按钮加到 flowLayoutPanel
            this.flowLayoutPanelNav.Controls.Add(this.navBtnProjects);
            this.flowLayoutPanelNav.Controls.Add(this.navBtnAdminProjects);
            this.flowLayoutPanelNav.Controls.Add(this.navBtnAdminQueues);
            this.flowLayoutPanelNav.Controls.Add(this.navBtnAdminUsers);
            this.flowLayoutPanelNav.Controls.Add(this.navBtnAdminExport);

            // 把 switch + account 放入 panelAccount（switch 在上，account 在下）
            this.panelAccount.Controls.Add(this.navBtnAccount);
            this.panelAccount.Controls.Add(this.navBtnSwitchView);

            //
            // MainForm - Controls 顺序：panelNav (Left), panelMain (Fill), statusStrip (Bottom)
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 750);

            // 清理旧的 Controls 添加，按正确顺序添加避免遮盖
            this.Controls.Clear();
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.panelNav);
            this.Controls.Add(this.statusStrip);

            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "图片标注系统";
            this.Load += new System.EventHandler(this.MainForm_Load);

            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.panelMain.ResumeLayout(false);
            this.panelWelcome.ResumeLayout(false);
            this.panelNav.ResumeLayout(false);
            this.flowLayoutPanelNav.ResumeLayout(false);
            this.panelAccount.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

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
