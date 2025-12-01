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
        private ToolStripButton btnRefresh = null!;

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

            // ListView
            listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            listView.Columns.Add("ID", 50);
            listView.Columns.Add("项目名称", 200);
            listView.Columns.Add("描述", 300);
            listView.Columns.Add("队列数", 80);
            listView.Columns.Add("创建者", 120);
            listView.Columns.Add("创建时间", 150);
            listView.DoubleClick += ListView_DoubleClick;

            // 工具栏
            var toolStrip = new ToolStrip();
            btnRefresh = new ToolStripButton("刷新");
            btnRefresh.Click += async (s, e) => await LoadProjectsAsync();
            toolStrip.Items.Add(btnRefresh);

            this.Controls.Add(listView);
            this.Controls.Add(toolStrip);
            toolStrip.Dock = DockStyle.Top;
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
    }
}
