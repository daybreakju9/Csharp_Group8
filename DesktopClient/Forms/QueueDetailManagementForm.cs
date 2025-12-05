using ImageAnnotationApp.Models;
using ImageAnnotationApp.Services;
using ImageAnnotationApp.Helpers;

namespace ImageAnnotationApp.Forms
{
    /// <summary>
    /// 队列图片管理表单 - 重构版
    /// 支持分页、搜索、图片组管理
    /// </summary>
    public partial class QueueDetailManagementForm : Form
    {
        private readonly Models.Queue _queue;
        private readonly QueueService _queueService;
        private readonly ImageService _imageService;

        // UI控件
        private Label lblQueueInfo = null!;
        private TextBox txtSearch = null!;
        private ComboBox cmbViewMode = null!;
        private ListView listViewGroups = null!;
        private ListView listViewImages = null!;
        private Label lblPageInfo = null!;
        private Button btnPrevPage = null!;
        private Button btnNextPage = null!;
        private Label lblStats = null!;
        private Panel panelGroupView = null!;
        private Panel panelImageView = null!;

        // 分页和数据
        private int _currentPage = 1;
        private const int PageSize = 50;
        private int _totalPages = 0;
        private int _totalCount = 0;
        private string _searchTerm = "";
        private ViewMode _viewMode = ViewMode.Groups;

        private enum ViewMode
        {
            Groups,   // 图片组视图
            Images    // 图片列表视图
        }

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
            this.Text = $"图片管理 - {_queue.Name}";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterParent;

            // ========== 顶部信息面板 ==========
            var infoPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(15),
                BackColor = Color.FromArgb(245, 245, 245)
            };

            lblQueueInfo = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular),
                Text = "加载中...",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft
            };

            infoPanel.Controls.Add(lblQueueInfo);

            // ========== 工具栏 ==========
            var toolStrip = new ToolStrip
            {
                Dock = DockStyle.Top,
                Padding = new Padding(10, 5, 10, 5),
                GripStyle = ToolStripGripStyle.Hidden
            };

            var btnImport = new ToolStripButton
            {
                Text = "导入图片",
                Image = null,
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Microsoft YaHei UI", 9F)
            };
            btnImport.Click += BtnImport_Click;

            var btnRefresh = new ToolStripButton
            {
                Text = "刷新",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Microsoft YaHei UI", 9F)
            };
            btnRefresh.Click += async (s, e) => await LoadDataAsync();

            var btnDeleteSelected = new ToolStripButton
            {
                Text = "批量删除",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Microsoft YaHei UI", 9F),
                ForeColor = Color.Red
            };
            btnDeleteSelected.Click += async (s, e) => await BtnDeleteSelected_ClickAsync();

            var btnBack = new ToolStripButton
            {
                Text = "返回",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Microsoft YaHei UI", 9F)
            };
            btnBack.Click += (s, e) => this.Close();

            toolStrip.Items.Add(btnImport);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(btnRefresh);
            toolStrip.Items.Add(btnDeleteSelected);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(btnBack);

            // ========== 搜索和视图控制面板 ==========
            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(15, 10, 15, 10)
            };

            var lblSearch = new Label
            {
                Text = "搜索:",
                Location = new Point(15, 15),
                AutoSize = true,
                Font = new Font("Microsoft YaHei UI", 9F)
            };

            txtSearch = new TextBox
            {
                Location = new Point(60, 12),
                Size = new Size(300, 25),
                Font = new Font("Microsoft YaHei UI", 9F)
            };
            txtSearch.KeyPress += TxtSearch_KeyPress;

            var btnSearch = new Button
            {
                Text = "搜索",
                Location = new Point(370, 11),
                Size = new Size(70, 27),
                Font = new Font("Microsoft YaHei UI", 9F)
            };
            btnSearch.Click += async (s, e) => await PerformSearchAsync();

            var lblViewMode = new Label
            {
                Text = "视图模式:",
                Location = new Point(460, 15),
                AutoSize = true,
                Font = new Font("Microsoft YaHei UI", 9F)
            };

            cmbViewMode = new ComboBox
            {
                Location = new Point(540, 12),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Microsoft YaHei UI", 9F)
            };
            cmbViewMode.Items.AddRange(new object[] { "图片组视图", "图片列表视图" });
            cmbViewMode.SelectedIndex = 0;
            cmbViewMode.SelectedIndexChanged += CmbViewMode_SelectedIndexChanged;

            searchPanel.Controls.AddRange(new Control[]
            {
                lblSearch, txtSearch, btnSearch, lblViewMode, cmbViewMode
            });

            // ========== 主内容区 ==========
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // 图片组视图面板
            panelGroupView = CreateGroupViewPanel();
            panelGroupView.Visible = true;

            // 图片列表视图面板
            panelImageView = CreateImageViewPanel();
            panelImageView.Visible = false;

            mainPanel.Controls.Add(panelGroupView);
            mainPanel.Controls.Add(panelImageView);

            // ========== 底部面板 ==========
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                Padding = new Padding(15)
            };

            // 分页控件
            btnPrevPage = new Button
            {
                Text = "上一页",
                Location = new Point(15, 15),
                Size = new Size(80, 30),
                Enabled = false
            };
            btnPrevPage.Click += async (s, e) => await NavigatePageAsync(-1);

            lblPageInfo = new Label
            {
                Text = "第 1 页，共 1 页",
                Location = new Point(105, 20),
                Size = new Size(200, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft YaHei UI", 9F)
            };

            btnNextPage = new Button
            {
                Text = "下一页",
                Location = new Point(310, 15),
                Size = new Size(80, 30),
                Enabled = false
            };
            btnNextPage.Click += async (s, e) => await NavigatePageAsync(1);

            // 统计信息
            lblStats = new Label
            {
                Text = "统计信息：0 个图片组，0 张图片",
                Location = new Point(410, 20),
                AutoSize = true,
                Font = new Font("Microsoft YaHei UI", 9F),
                ForeColor = Color.Gray
            };

            bottomPanel.Controls.AddRange(new Control[]
            {
                btnPrevPage, lblPageInfo, btnNextPage, lblStats
            });

            // ========== 添加所有控件到窗体 ==========
            this.Controls.Add(mainPanel);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(searchPanel);
            this.Controls.Add(toolStrip);
            this.Controls.Add(infoPanel);
        }

        private Panel CreateGroupViewPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill
            };

            listViewGroups = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                CheckBoxes = true,
                Font = new Font("Microsoft YaHei UI", 9F)
            };

            listViewGroups.Columns.Add("图片组名称", 300);
            listViewGroups.Columns.Add("图片数量", 100);
            listViewGroups.Columns.Add("已完成", 80);
            listViewGroups.Columns.Add("顺序", 80);
            listViewGroups.Columns.Add("创建时间", 150);
            listViewGroups.Columns.Add("图片组ID", 100);

            listViewGroups.DoubleClick += ListViewGroups_DoubleClick;

            panel.Controls.Add(listViewGroups);
            return panel;
        }

        private Panel CreateImageViewPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill
            };

            listViewImages = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                CheckBoxes = true,
                Font = new Font("Microsoft YaHei UI", 9F)
            };

            listViewImages.Columns.Add("图片ID", 80);
            listViewImages.Columns.Add("图片组ID", 100);
            listViewImages.Columns.Add("文件夹", 150);
            listViewImages.Columns.Add("文件名", 300);
            listViewImages.Columns.Add("顺序", 80);
            listViewImages.Columns.Add("尺寸", 100);
            listViewImages.Columns.Add("大小", 100);
            listViewImages.Columns.Add("创建时间", 150);

            panel.Controls.Add(listViewImages);
            return panel;
        }

        private async Task LoadQueueInfoAsync()
        {
            try
            {
                var queue = await _queueService.GetByIdAsync(_queue.Id);
                if (queue != null)
                {
                    lblQueueInfo.Text =
                        $"队列名称: {queue.Name}  |  " +
                        $"对比图片数: {queue.ComparisonCount}  |  " +
                        $"图片组数: {queue.GroupCount}  |  " +
                        $"总图片数: {queue.TotalImageCount}  |  " +
                        $"状态: {queue.Status}  |  " +
                        $"创建时间: {queue.CreatedAt:yyyy-MM-dd HH:mm}";

                    UpdateStatsLabel(queue.GroupCount, queue.TotalImageCount);
                }

                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载队列信息失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                if (_viewMode == ViewMode.Groups)
                {
                    await LoadImageGroupsAsync();
                }
                else
                {
                    await LoadImagesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载数据失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadImageGroupsAsync()
        {
            try
            {
                listViewGroups.Items.Clear();
                this.Cursor = Cursors.WaitCursor;

                var result = await _imageService.GetImageGroupsPagedAsync(
                    _queue.Id,
                    _currentPage,
                    PageSize,
                    string.IsNullOrWhiteSpace(_searchTerm) ? null : _searchTerm
                );

                _totalPages = result.TotalPages;
                _totalCount = result.TotalCount;

                foreach (var group in result.Items)
                {
                    var item = new ListViewItem(group.GroupName);
                    item.SubItems.Add(group.ImageCount.ToString());
                    item.SubItems.Add(group.IsCompleted ? "是" : "否");
                    item.SubItems.Add(group.DisplayOrder.ToString());
                    item.SubItems.Add(group.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
                    item.SubItems.Add(group.Id.ToString());
                    item.Tag = group;
                    listViewGroups.Items.Add(item);
                }

                UpdatePaginationControls();
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private async Task LoadImagesAsync()
        {
            try
            {
                listViewImages.Items.Clear();
                this.Cursor = Cursors.WaitCursor;

                var result = await _imageService.GetQueueImagesPagedAsync(
                    _queue.Id,
                    _currentPage,
                    PageSize,
                    string.IsNullOrWhiteSpace(_searchTerm) ? null : _searchTerm,
                    null
                );

                _totalPages = result.TotalPages;
                _totalCount = result.TotalCount;

                foreach (var image in result.Items)
                {
                    var item = new ListViewItem(image.Id.ToString());
                    item.SubItems.Add(image.ImageGroupId.ToString());
                    item.SubItems.Add(image.FolderName);
                    item.SubItems.Add(image.FileName);
                    item.SubItems.Add(image.DisplayOrder.ToString());

                    string dimensions = (image.Width.HasValue && image.Height.HasValue)
                        ? $"{image.Width} × {image.Height}"
                        : "未知";
                    item.SubItems.Add(dimensions);

                    string fileSize = FormatFileSize(image.FileSize);
                    item.SubItems.Add(fileSize);

                    item.SubItems.Add(image.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
                    item.Tag = image;
                    listViewImages.Items.Add(item);
                }

                UpdatePaginationControls();
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void UpdatePaginationControls()
        {
            lblPageInfo.Text = $"第 {_currentPage} 页，共 {_totalPages} 页  (总计 {_totalCount} 条)";
            btnPrevPage.Enabled = _currentPage > 1;
            btnNextPage.Enabled = _currentPage < _totalPages;
        }

        private void UpdateStatsLabel(int groupCount, int imageCount)
        {
            lblStats.Text = $"统计信息：{groupCount} 个图片组，{imageCount} 张图片";
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";
            else if (bytes < 1024 * 1024)
                return $"{bytes / 1024.0:F1} KB";
            else if (bytes < 1024 * 1024 * 1024)
                return $"{bytes / (1024.0 * 1024):F1} MB";
            else
                return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
        }

        private async Task NavigatePageAsync(int direction)
        {
            int newPage = _currentPage + direction;
            if (newPage < 1 || newPage > _totalPages)
                return;

            _currentPage = newPage;
            await LoadDataAsync();
        }

        private async Task PerformSearchAsync()
        {
            _searchTerm = txtSearch.Text.Trim();
            _currentPage = 1; // 重置到第一页
            await LoadDataAsync();
        }

        private void TxtSearch_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                _ = PerformSearchAsync();
            }
        }

        private async void CmbViewMode_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _viewMode = cmbViewMode.SelectedIndex == 0 ? ViewMode.Groups : ViewMode.Images;
            panelGroupView.Visible = (_viewMode == ViewMode.Groups);
            panelImageView.Visible = (_viewMode == ViewMode.Images);

            _currentPage = 1;
            _searchTerm = "";
            txtSearch.Text = "";

            await LoadDataAsync();
        }

        private void ListViewGroups_DoubleClick(object? sender, EventArgs e)
        {
            if (listViewGroups.SelectedItems.Count == 0)
                return;

            var group = listViewGroups.SelectedItems[0].Tag as ImageGroup;
            if (group == null)
                return;

            ShowGroupDetailsDialog(group);
        }

        private void ShowGroupDetailsDialog(ImageGroup group)
        {
            var dialog = new Form
            {
                Text = $"图片组详情 - {group.GroupName}",
                Size = new Size(800, 600),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.Sizable
            };

            var listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Microsoft YaHei UI", 9F)
            };

            listView.Columns.Add("文件夹", 150);
            listView.Columns.Add("文件名", 250);
            listView.Columns.Add("尺寸", 100);
            listView.Columns.Add("大小", 100);
            listView.Columns.Add("顺序", 80);

            foreach (var image in group.Images)
            {
                var item = new ListViewItem(image.FolderName);
                item.SubItems.Add(image.FileName);

                string dimensions = (image.Width.HasValue && image.Height.HasValue)
                    ? $"{image.Width} × {image.Height}"
                    : "未知";
                item.SubItems.Add(dimensions);

                item.SubItems.Add(FormatFileSize(image.FileSize));
                item.SubItems.Add(image.DisplayOrder.ToString());
                listView.Items.Add(item);
            }

            var btnClose = new Button
            {
                Text = "关闭",
                Dock = DockStyle.Bottom,
                Height = 40,
                DialogResult = DialogResult.OK
            };

            dialog.Controls.Add(listView);
            dialog.Controls.Add(btnClose);
            dialog.ShowDialog();
        }

        private void BtnImport_Click(object? sender, EventArgs e)
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
            if (_viewMode == ViewMode.Groups)
            {
                await DeleteSelectedGroupsAsync();
            }
            else
            {
                await DeleteSelectedImagesAsync();
            }
        }

        private async Task DeleteSelectedGroupsAsync()
        {
            var checkedItems = listViewGroups.CheckedItems;
            if (checkedItems.Count == 0)
            {
                MessageBox.Show("请选择要删除的图片组", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 计算总图片数
            int totalImages = 0;
            var imageIds = new List<int>();

            foreach (ListViewItem item in checkedItems)
            {
                var group = item.Tag as ImageGroup;
                if (group != null)
                {
                    totalImages += group.Images.Count;
                    imageIds.AddRange(group.Images.Select(img => img.Id));
                }
            }

            var result = MessageBox.Show(
                $"确定要删除选中的 {checkedItems.Count} 个图片组吗？\n" +
                $"这将删除共 {totalImages} 张图片！\n\n" +
                $"此操作不可撤销！",
                "确认删除",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            await DeleteImagesById(imageIds, totalImages);
        }

        private async Task DeleteSelectedImagesAsync()
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

            if (result != DialogResult.Yes)
                return;

            var imageIds = checkedItems.Cast<ListViewItem>()
                .Where(item => item.Tag is Models.Image)
                .Select(item => ((Models.Image)item.Tag!).Id)
                .ToList();

            await DeleteImagesById(imageIds, imageIds.Count);
        }

        private async Task DeleteImagesById(List<int> imageIds, int expectedCount)
        {
            if (imageIds.Count == 0)
                return;

            try
            {
                this.Cursor = Cursors.WaitCursor;

                // 使用批量删除API
                int deletedCount = await _imageService.DeleteBatchAsync(imageIds);

                MessageBox.Show(
                    $"删除完成！\n成功删除 {deletedCount} 张图片",
                    "删除结果",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // 重新加载数据
                await LoadQueueInfoAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
    }
}
