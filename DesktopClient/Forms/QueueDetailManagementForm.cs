using ImageAnnotationApp.Models;
using ImageAnnotationApp.Services;

namespace ImageAnnotationApp.Forms
{
    public partial class QueueDetailManagementForm : Form
    {
        private readonly Models.Queue _queue;
        private readonly QueueService _queueService;
        private readonly ImageService _imageService;
        private TabControl tabControl = null!;
        private Label lblQueueInfo = null!;
        private ListView listViewImages = null!;
        private List<Models.Image> _images = new();

        public QueueDetailManagementForm(Models.Queue queue)
        {
            _queue = queue;
            _queueService = new QueueService();
            _imageService = new ImageService();
            InitializeCustomComponents();
            _ = LoadQueueInfoAsync();
        }

        private void InitializeCustomComponents()
        {
            this.Text = $"队列管理 - {_queue.Name}";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterParent;

            // 顶部信息面板
            var infoPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(10)
            };

            lblQueueInfo = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Microsoft YaHei UI", 10F),
                Text = "加载中..."
            };

            infoPanel.Controls.Add(lblQueueInfo);

            // 工具栏
            var toolStrip = new ToolStrip();
            var btnImportImages = new ToolStripButton("导入图片");
            var btnRefresh = new ToolStripButton("刷新");
            var btnDeleteSelected = new ToolStripButton("删除选中图片");
            var btnBack = new ToolStripButton("返回");

            btnImportImages.Click += BtnImportImages_Click;
            btnRefresh.Click += async (s, e) => await LoadImagesAsync();
            btnDeleteSelected.Click += async (s, e) => await BtnDeleteSelected_ClickAsync();
            btnBack.Click += BtnBack_Click;

            toolStrip.Items.Add(btnImportImages);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(btnRefresh);
            toolStrip.Items.Add(btnDeleteSelected);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(btnBack);

            // 标签页控件
            tabControl = new TabControl { Dock = DockStyle.Fill };

            // 图片列表标签页
            var tabImages = new TabPage("图片列表");
            listViewImages = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                CheckBoxes = true
            };
            listViewImages.Columns.Add("ID", 60);
            listViewImages.Columns.Add("图片组", 200);
            listViewImages.Columns.Add("文件夹", 150);
            listViewImages.Columns.Add("文件名", 250);
            listViewImages.Columns.Add("顺序", 60);

            tabImages.Controls.Add(listViewImages);
            tabControl.TabPages.Add(tabImages);

            // 统计标签页
            var tabStats = new TabPage("统计信息");
            var lblStats = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Microsoft YaHei UI", 10F),
                Padding = new Padding(20)
            };
            tabStats.Controls.Add(lblStats);
            tabControl.TabPages.Add(tabStats);

            this.Controls.Add(tabControl);
            this.Controls.Add(toolStrip);
            this.Controls.Add(infoPanel);
        }

        private async Task LoadQueueInfoAsync()
        {
            try
            {
                var queue = await _queueService.GetByIdAsync(_queue.Id);
                if (queue != null)
                {
                    lblQueueInfo.Text = $"队列名称: {queue.Name}\n" +
                                       $"对比图片数: {queue.ImageCount}    总图片数: {queue.TotalImages}    " +
                                       $"创建时间: {queue.CreatedAt:yyyy-MM-dd HH:mm}";
                }

                await LoadImagesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载队列信息失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadImagesAsync()
        {
            try
            {
                listViewImages.Items.Clear();
                _images = await _imageService.GetQueueImagesAsync(_queue.Id);

                foreach (var image in _images)
                {
                    var item = new ListViewItem(image.Id.ToString());
                    item.SubItems.Add(image.ImageGroup);
                    item.SubItems.Add(image.FolderName);
                    item.SubItems.Add(image.FileName);
                    item.SubItems.Add(image.Order.ToString());
                    item.Tag = image;
                    listViewImages.Items.Add(item);
                }

                // 更新统计信息
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载图片列表失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateStatistics()
        {
            if (tabControl.TabPages.Count < 2) return;

            var statsTab = tabControl.TabPages[1];
            var lblStats = statsTab.Controls[0] as Label;
            if (lblStats == null) return;

            var groupCount = _images.Select(i => i.ImageGroup).Distinct().Count();
            var folderCount = _images.Select(i => i.FolderName).Distinct().Count();

            var folderStats = _images.GroupBy(i => i.FolderName)
                .Select(g => $"  - {g.Key}: {g.Count()} 张")
                .ToList();

            lblStats.Text = $"总计:\n" +
                           $"  图片总数: {_images.Count} 张\n" +
                           $"  图片组数: {groupCount} 个\n" +
                           $"  文件夹数: {folderCount} 个\n\n" +
                           $"各文件夹图片数:\n" +
                           string.Join("\n", folderStats);
        }

        private void BtnImportImages_Click(object? sender, EventArgs e)
        {
            try
            {
                var importForm = new ImageImportForm(_queue);
                if (importForm.ShowDialog() == DialogResult.OK)
                {
                    _ = LoadQueueInfoAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开导入界面失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task BtnDeleteSelected_ClickAsync()
        {
            var checkedItems = listViewImages.CheckedItems;
            if (checkedItems.Count == 0)
            {
                MessageBox.Show("请选择要删除的图片", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"确定要删除选中的 {checkedItems.Count} 张图片吗？\n\n此操作不可撤销！",
                "确认删除",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            try
            {
                int successCount = 0;
                int failCount = 0;
                var errors = new List<string>();

                foreach (ListViewItem item in checkedItems)
                {
                    var image = item.Tag as Models.Image;
                    if (image == null) continue;

                    try
                    {
                        await _imageService.DeleteAsync(image.Id);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        errors.Add($"{image.FileName}: {ex.Message}");
                    }
                }

                string message = $"删除完成\n成功: {successCount} 张\n失败: {failCount} 张";
                if (errors.Count > 0)
                {
                    message += "\n\n错误详情:\n" + string.Join("\n", errors.Take(5));
                    if (errors.Count > 5)
                        message += $"\n... 还有 {errors.Count - 5} 个错误";
                }

                MessageBox.Show(message, "删除结果",
                    MessageBoxButtons.OK,
                    failCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                await LoadQueueInfoAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBack_Click(object? sender, EventArgs e)
        {
            this.Close();
        }
    }
}
