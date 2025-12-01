using ImageAnnotationApp.Services;
using ImageAnnotationApp.Models;
using ImageAnnotationApp.Helpers;

namespace ImageAnnotationApp.Forms
{
    public partial class QueueListForm : BaseForm
    {
        private readonly QueueService _queueService;
        private readonly int _projectId;
        private readonly string _projectName;
        private ListView listView = null!;
        private ToolStripButton btnRefresh = null!;
        private ToolStripButton btnBack = null!;

        public QueueListForm(int projectId, string projectName)
        {
            _queueService = new QueueService();
            _projectId = projectId;
            _projectName = projectName;
            InitializeCustomComponents();
            _ = LoadQueuesAsync();
        }

        private void InitializeCustomComponents()
        {
            this.Text = UIConstants.FormatWindowTitle("队列列表", _projectName);
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
            listView.Columns.Add("队列名称", 200);
            listView.Columns.Add("对比图片数", 100);
            listView.Columns.Add("总图片组", 100);
            listView.Columns.Add("创建时间", 150);
            listView.Columns.Add("", 150);

            // 工具栏
            var toolStrip = new ToolStrip();
            btnBack = new ToolStripButton("返回");
            btnBack.Click += BtnBack_Click;
            btnRefresh = new ToolStripButton("刷新");
            btnRefresh.Click += async (s, e) => await LoadQueuesAsync();
            toolStrip.Items.Add(btnBack);
            toolStrip.Items.Add(btnRefresh);

            this.Controls.Add(listView);
            this.Controls.Add(toolStrip);
            toolStrip.Dock = DockStyle.Top;

            listView.MouseDoubleClick += ListView_MouseDoubleClick;
            listView.DoubleClick += (s, e) => ListView_MouseDoubleClick(s, new MouseEventArgs(MouseButtons.Left, 2, 0, 0, 0));
        }

        private async Task LoadQueuesAsync()
        {
            try
            {
                UpdateStatus(UIConstants.StatusMessages.Loading);
                btnRefresh.Enabled = false;
                listView.Items.Clear();

                var queues = await _queueService.GetAllAsync(_projectId);

                foreach (var queue in queues)
                {
                    var item = new ListViewItem(queue.Id.ToString());
                    item.SubItems.Add(queue.Name);
                    item.SubItems.Add(queue.ImageCount.ToString());
                    item.SubItems.Add(queue.TotalImages.ToString());
                    item.SubItems.Add(queue.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
                    item.SubItems.Add("双击开始选择");
                    item.Tag = queue;
                    listView.Items.Add(item);
                }
                UpdateStatus(UIConstants.StatusMessages.Ready);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载队列列表失败: {ex.Message}", UIConstants.MessageTitles.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus(UIConstants.Messages.LoadFailed);
            }
            finally
            {
                btnRefresh.Enabled = true;
            }
        }

        private void ListView_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            try
            {
                if (listView.SelectedItems.Count == 0)
                {
                    return;
                }

                var queue = listView.SelectedItems[0].Tag as Models.Queue;
                if (queue == null)
                {
                    MessageBox.Show("无法获取队列信息", UIConstants.MessageTitles.Error,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                NavigateTo(new ImageSelectionForm(queue.Id, queue.Name));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开图片选择界面失败: {ex.Message}", UIConstants.MessageTitles.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBack_Click(object? sender, EventArgs e)
        {
            NavigateBack();
        }
    }
}
