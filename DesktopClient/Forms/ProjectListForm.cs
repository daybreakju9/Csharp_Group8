using System.Drawing;
using System.Windows.Forms;
using ImageAnnotationApp.Services;
using ImageAnnotationApp.Models;
using ImageAnnotationApp.Helpers;

namespace ImageAnnotationApp.Forms
{
    public partial class ProjectListForm : BaseForm
    {
        private readonly ProjectService _projectService;
        private readonly AuthService _authService;
        private ListView listView = null!;
        private Button btnRefresh = null!;

        public ProjectListForm()
        {
            _projectService = new ProjectService();
            _authService = AuthService.Instance;
            InitializeCustomComponents();
            _ = LoadProjectsAsync();
        }

        private void InitializeCustomComponents()
        {
            this.Text = UIConstants.FormatWindowTitle("项目列表");
            this.Size = UIConstants.WindowSizes.Medium;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = UIConstants.Fonts.Normal;
            this.BackColor = UIConstants.Colors.Background;

            // ListView
            listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = UIConstants.Fonts.Normal
            };
            listView.Columns.Add("ID", 50);
            listView.Columns.Add("项目名称", 200);
            listView.Columns.Add("描述", 300);
            listView.Columns.Add("队列数", 80);
            listView.Columns.Add("创建者", 120);
            listView.Columns.Add("创建时间", 150);
            listView.DoubleClick += ListView_DoubleClick;

            // 顶部按钮区（与管理界面风格一致）
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = UIConstants.ButtonSizes.Medium.Height + UIConstants.Spacing.Large * 2,
                Padding = new Padding(UIConstants.Spacing.Medium),
                BackColor = this.BackColor
            };
            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = false,
                AutoScroll = false,
                BackColor = this.BackColor
            };

            btnRefresh = UIConstants.CreateButton("刷新", UIConstants.ButtonSizes.Medium, UIConstants.Colors.PrimaryButton, null);
            btnRefresh.Font = UIConstants.Fonts.Normal;
            btnRefresh.Margin = new Padding(0, 0, UIConstants.Spacing.Medium, 0);
            ApplyGhostButtonStyle(btnRefresh);
            btnRefresh.Click += async (s, e) => await LoadProjectsAsync();

            flow.Controls.Add(btnRefresh);
            topPanel.Controls.Add(flow);

            this.Controls.Add(listView);
            this.Controls.Add(topPanel);
        }

        private async Task LoadProjectsAsync()
        {
            try
            {
                UpdateStatus(UIConstants.StatusMessages.Loading);
                btnRefresh.Enabled = false;
                listView.Items.Clear();

                var projects = await _projectService.GetAllAsync();

                foreach (var project in projects)
                {
                    var item = new ListViewItem(project.Id.ToString());
                    item.SubItems.Add(project.Name);
                    item.SubItems.Add(project.Description ?? "");
                    item.SubItems.Add(project.QueueCount.ToString());
                    item.SubItems.Add(project.CreatedByUsername);
                    item.SubItems.Add(project.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
                    item.Tag = project;
                    listView.Items.Add(item);
                }
                UpdateStatus(UIConstants.StatusMessages.Ready);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载项目列表失败: {ex.Message}", UIConstants.MessageTitles.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus(UIConstants.Messages.LoadFailed);
            }
            finally
            {
                btnRefresh.Enabled = true;
            }
        }

        private void ListView_DoubleClick(object? sender, EventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                var project = listView.SelectedItems[0].Tag as Project;
                if (project != null)
                {
                    if (_authService.CurrentUser?.Role == "Guest")
                    {
                        MessageBox.Show("游客用户无法进入项目，请等待管理员审核。", UIConstants.MessageTitles.Warning,
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    NavigateTo(new QueueListForm(project.Id, project.Name));
                }
            }
        }

        // 与管理界面保持一致的“幽灵”按钮样式
        private void ApplyGhostButtonStyle(Button btn)
        {
            if (btn == null) return;

            var normalColor = UIConstants.Colors.Background;
            var hover = Color.FromArgb(64, 158, 255);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.LightGray;
            btn.BackColor = normalColor;
            btn.ForeColor = UIConstants.Colors.TextPrimary;
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.MouseDownBackColor = hover;
            btn.FlatAppearance.MouseOverBackColor = hover;
            btn.MouseEnter += (s, e) => btn.BackColor = hover;
            btn.MouseLeave += (s, e) => btn.BackColor = normalColor;
        }
    }
}
