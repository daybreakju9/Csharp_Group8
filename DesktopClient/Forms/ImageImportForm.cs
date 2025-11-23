using ImageAnnotationApp.Services;
using ImageAnnotationApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace ImageAnnotationApp.Forms
{
    public partial class ImageImportForm : Form
    {
        private readonly ImageService _imageService;
        private readonly QueueService _queueService;
        private readonly Models.Queue _queue;
        private TabControl tabControl = null!;
        private ListView listViewFolders = null!;
        private ListView listViewPreview = null!;
        private ListView listViewUploaded = null!;
        private ToolStripLabel lblUploadedCount = null!;
        private Panel progressPanel = null!;
        private ProgressBar progressBar = null!;
        private Label lblProgress = null!;
        private Button btnImport = null!;
        private Button btnCancel = null!;
        private Dictionary<string, FolderData> _folders = new();
        private bool _isUploading = false;
        private List<Models.Image> _uploadedImages = new();

        private class FolderData
        {
            public string Name { get; set; } = string.Empty;
            public List<string> FilePaths { get; set; } = new();
        }

        public ImageImportForm(Models.Queue queue)
        {
            _imageService = new ImageService();
            _queueService = new QueueService();
            _queue = queue;
            InitializeCustomComponents();
            LoadExistingImagesAsync();
        }

        private void InitializeCustomComponents()
        {
            this.Text = $"批量导入图片 - {_queue.Name}";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterParent;

            // 队列信息面板
            var infoPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblInfo = new Label
            {
                Text = $"队列: {_queue.Name} | 所属项目: {_queue.ProjectName} | 对比图片数: {_queue.ImageCount} 张 | 当前总图片数: {_queue.TotalImages}",
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblTip = new Label
            {
                Text = "提示: 需要上传 " + _queue.ImageCount + " 个文件夹，每个文件夹包含相同文件名的图片。系统会自动按文件名分组。",
                Dock = DockStyle.Bottom,
                Height = 30,
                ForeColor = Color.Blue,
                Font = new Font("Microsoft YaHei", 9F)
            };

            infoPanel.Controls.Add(lblInfo);
            infoPanel.Controls.Add(lblTip);

            // 标签页控件
            tabControl = new TabControl { Dock = DockStyle.Fill };

            // 文件夹管理标签页
            var tabFolders = new TabPage("文件夹管理");
            var panelFolders = new Panel { Dock = DockStyle.Fill };

            var toolStripFolders = new ToolStrip();
            var btnAddFolder = new ToolStripButton("添加文件夹");
            var btnRemoveFolder = new ToolStripButton("删除文件夹");
            var btnSelectFiles = new ToolStripButton("选择文件");
            var btnClearFiles = new ToolStripButton("清空文件");

            btnAddFolder.Click += BtnAddFolder_Click;
            btnRemoveFolder.Click += BtnRemoveFolder_Click;
            btnSelectFiles.Click += BtnSelectFiles_Click;
            btnClearFiles.Click += BtnClearFiles_Click;

            toolStripFolders.Items.Add(btnAddFolder);
            toolStripFolders.Items.Add(btnRemoveFolder);
            toolStripFolders.Items.Add(new ToolStripSeparator());
            toolStripFolders.Items.Add(btnSelectFiles);
            toolStripFolders.Items.Add(btnClearFiles);

            listViewFolders = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            listViewFolders.Columns.Add("文件夹名称", 200);
            listViewFolders.Columns.Add("文件数量", 100);
            listViewFolders.Columns.Add("文件列表", 600);

            panelFolders.Controls.Add(listViewFolders);
            panelFolders.Controls.Add(toolStripFolders);
            toolStripFolders.Dock = DockStyle.Top;

            tabFolders.Controls.Add(panelFolders);

            // 导入预览标签页
            var tabPreview = new TabPage("导入预览");
            listViewPreview = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            listViewPreview.Columns.Add("图片文件名", 300);
            foreach (var folder in _folders.Keys)
            {
                listViewPreview.Columns.Add(folder, 150);
            }
            listViewPreview.Columns.Add("状态", 100);

            tabPreview.Controls.Add(listViewPreview);

            // 已上传图片标签页
            var tabUploaded = new TabPage("已上传图片");
            var panelUploaded = new Panel { Dock = DockStyle.Fill };

            var toolStripUploaded = new ToolStrip();
            var btnRefreshUploaded = new ToolStripButton("刷新");
            lblUploadedCount = new ToolStripLabel("已上传: 0 个图片组");
            btnRefreshUploaded.Click += async (s, e) => await LoadExistingImagesAsync();
            toolStripUploaded.Items.Add(btnRefreshUploaded);
            toolStripUploaded.Items.Add(new ToolStripSeparator());
            toolStripUploaded.Items.Add(lblUploadedCount);

            listViewUploaded = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            listViewUploaded.Columns.Add("图片组", 300);
            listViewUploaded.Columns.Add("文件夹", 150);
            listViewUploaded.Columns.Add("文件名", 250);
            listViewUploaded.Columns.Add("图片ID", 80);

            panelUploaded.Controls.Add(listViewUploaded);
            panelUploaded.Controls.Add(toolStripUploaded);
            toolStripUploaded.Dock = DockStyle.Top;

            tabUploaded.Controls.Add(panelUploaded);

            tabControl.TabPages.Add(tabFolders);
            tabControl.TabPages.Add(tabPreview);
            tabControl.TabPages.Add(tabUploaded);

            // 进度条
            progressPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(10),
                Visible = false
            };

            progressBar = new ProgressBar
            {
                Dock = DockStyle.Top,
                Height = 25,
                Style = ProgressBarStyle.Continuous
            };

            lblProgress = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                Text = "准备就绪",
                TextAlign = ContentAlignment.MiddleLeft
            };

            progressPanel.Controls.Add(progressBar);
            progressPanel.Controls.Add(lblProgress);

            // 按钮面板
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(10)
            };

            btnImport = new Button
            {
                Text = "开始导入",
                Size = new Size(120, 35),
                Location = new Point(800, 7),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnImport.Click += async (s, e) => await BtnImport_ClickAsync();

            btnCancel = new Button
            {
                Text = "取消",
                Size = new Size(120, 35),
                Location = new Point(930, 7),
                DialogResult = DialogResult.Cancel
            };

            buttonPanel.Controls.Add(btnImport);
            buttonPanel.Controls.Add(btnCancel);

            this.Controls.Add(tabControl);
            this.Controls.Add(infoPanel);
            this.Controls.Add(progressPanel);
            this.Controls.Add(buttonPanel);

            UpdateFoldersList();
            UpdatePreview();
        }

        private void BtnAddFolder_Click(object? sender, EventArgs e)
        {
            if (_folders.Count >= _queue.ImageCount)
            {
                MessageBox.Show($"最多只能添加 {_queue.ImageCount} 个文件夹", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var folderName = PromptInput("文件夹名称:", "添加文件夹", $"folder_{_folders.Count + 1}");
            if (string.IsNullOrWhiteSpace(folderName))
                return;

            if (_folders.ContainsKey(folderName))
            {
                MessageBox.Show("文件夹名称已存在", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _folders[folderName] = new FolderData { Name = folderName };
            UpdateFoldersList();
            UpdatePreview();
        }

        private void BtnRemoveFolder_Click(object? sender, EventArgs e)
        {
            if (listViewFolders.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选择要删除的文件夹", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var folderName = listViewFolders.SelectedItems[0].Text;
            if (_folders.Remove(folderName))
            {
                UpdateFoldersList();
                UpdatePreview();
            }
        }

        private void BtnSelectFiles_Click(object? sender, EventArgs e)
        {
            if (listViewFolders.SelectedItems.Count == 0)
            {
                MessageBox.Show("请先选择文件夹", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var folderName = listViewFolders.SelectedItems[0].Text;
            if (!_folders.ContainsKey(folderName))
                return;

            using var dialog = new OpenFileDialog
            {
                Filter = "图片文件|*.jpg;*.jpeg;*.png;*.gif;*.webp|所有文件|*.*",
                Multiselect = true,
                Title = $"为文件夹 '{folderName}' 选择图片"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _folders[folderName].FilePaths = dialog.FileNames.ToList();
                UpdateFoldersList();
                UpdatePreview();
            }
        }

        private void BtnClearFiles_Click(object? sender, EventArgs e)
        {
            if (listViewFolders.SelectedItems.Count == 0)
            {
                MessageBox.Show("请先选择文件夹", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var folderName = listViewFolders.SelectedItems[0].Text;
            if (_folders.ContainsKey(folderName))
            {
                _folders[folderName].FilePaths.Clear();
                UpdateFoldersList();
                UpdatePreview();
            }
        }

        private void UpdateFoldersList()
        {
            listViewFolders.Items.Clear();
            foreach (var folder in _folders.Values)
            {
                var item = new ListViewItem(folder.Name);
                item.SubItems.Add(folder.FilePaths.Count.ToString());
                var fileNames = string.Join("; ", folder.FilePaths.Select(Path.GetFileName).Take(10));
                if (folder.FilePaths.Count > 10)
                    fileNames += $" ... (共 {folder.FilePaths.Count} 个文件)";
                item.SubItems.Add(fileNames);
                item.Tag = folder;
                listViewFolders.Items.Add(item);
            }
        }

        private void UpdatePreview()
        {
            // 获取所有文件名
            var allFileNames = new HashSet<string>();
            foreach (var folder in _folders.Values)
            {
                foreach (var filePath in folder.FilePaths)
                {
                    allFileNames.Add(Path.GetFileName(filePath));
                }
            }

            // 更新预览列表
            listViewPreview.Items.Clear();
            listViewPreview.Columns.Clear();
            listViewPreview.Columns.Add("图片文件名", 300);

            foreach (var folderName in _folders.Keys)
            {
                listViewPreview.Columns.Add(folderName, 150);
            }
            listViewPreview.Columns.Add("状态", 100);

            foreach (var fileName in allFileNames.OrderBy(f => f))
            {
                var item = new ListViewItem(fileName);
                bool isComplete = true;

                foreach (var folderName in _folders.Keys)
                {
                    var hasFile = _folders[folderName].FilePaths.Any(f => Path.GetFileName(f) == fileName);
                    item.SubItems.Add(hasFile ? "✓" : "✗");
                    if (!hasFile) isComplete = false;
                }

                item.SubItems.Add(isComplete ? "完整" : "不完整");
                item.ForeColor = isComplete ? Color.Black : Color.Orange;
                listViewPreview.Items.Add(item);
            }
        }

        private async Task BtnImport_ClickAsync()
        {
            if (_folders.Count == 0)
            {
                MessageBox.Show("请至少添加一个文件夹", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_folders.Count < _queue.ImageCount)
            {
                var result = MessageBox.Show(
                    $"当前只有 {_folders.Count} 个文件夹，但队列设置需要 {_queue.ImageCount} 个文件夹。是否继续？",
                    "提示",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                    return;
            }

            // 检查是否有不完整的组
            var incompleteGroups = listViewPreview.Items.Cast<ListViewItem>()
                .Count(item => item.SubItems[item.SubItems.Count - 1].Text == "不完整");
            if (incompleteGroups > 0)
            {
                var result = MessageBox.Show(
                    $"有 {incompleteGroups} 个图片组不完整（未在所有文件夹中找到相同文件名）。是否继续导入？",
                    "警告",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (result != DialogResult.Yes)
                    return;
            }

            _isUploading = true;
            btnImport.Enabled = false;
            btnCancel.Enabled = false;
            progressPanel.Visible = true;
            progressBar.Value = 0;
            progressBar.Visible = true;
            lblProgress.Visible = true;

            try
            {
                // 准备上传数据
                var folderFiles = new Dictionary<string, List<(string fileName, byte[] data)>>();
                int totalFiles = 0;

                foreach (var folder in _folders.Values)
                {
                    var files = new List<(string fileName, byte[] data)>();
                    foreach (var filePath in folder.FilePaths)
                    {
                        try
                        {
                            var fileData = await File.ReadAllBytesAsync(filePath);
                            files.Add((Path.GetFileName(filePath), fileData));
                            totalFiles++;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"读取文件失败: {filePath}\n{ex.Message}", "错误",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    if (files.Count > 0)
                    {
                        folderFiles[folder.Name] = files;
                    }
                }

                if (folderFiles.Count == 0)
                {
                    MessageBox.Show("没有可上传的文件", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 上传图片
                var progress = new Progress<ParallelUploadProgress>(p =>
                {
                    progressBar.Value = (int)p.Percentage;
                    lblProgress.Text = $"上传中: {p.CompletedFiles} / {p.TotalFiles} ({p.Percentage:F1}%) - {p.CurrentFile}";
                });

                var result = await _imageService.UploadImagesParallelAsync(
                    _queue.Id,
                    folderFiles,
                    maxConcurrency: 5,
                    progress);

                if (result.SuccessCount > 0)
                {
                    MessageBox.Show(
                        $"导入完成！\n成功: {result.SuccessCount} 个文件\n失败: {result.FailureCount} 个文件",
                        "导入完成",
                        MessageBoxButtons.OK,
                        result.FailureCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                    if (result.Errors.Count > 0)
                    {
                        var errorMsg = string.Join("\n", result.Errors.Take(10));
                        if (result.Errors.Count > 10)
                            errorMsg += $"\n... 还有 {result.Errors.Count - 10} 个错误";
                        MessageBox.Show($"错误详情:\n{errorMsg}", "上传错误",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    // 刷新队列信息
                    await LoadExistingImagesAsync();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("所有文件上传失败", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导入失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isUploading = false;
                btnImport.Enabled = true;
                btnCancel.Enabled = true;
                progressPanel.Visible = false;
                progressBar.Visible = false;
                lblProgress.Visible = false;
            }
        }

        private async Task LoadExistingImagesAsync()
        {
            try
            {
                var images = await _imageService.GetQueueImagesAsync(_queue.Id);
                _uploadedImages = images;

                if (images.Count > 0)
                {
                    // 按文件夹分组
                    var folderMap = images.GroupBy(img => img.FolderName)
                        .ToDictionary(g => g.Key, g => g.Select(img => img.FileName).Distinct().ToList());

                    // 添加到现有文件夹或创建新文件夹
                    foreach (var kvp in folderMap)
                    {
                        if (!_folders.ContainsKey(kvp.Key))
                        {
                            _folders[kvp.Key] = new FolderData { Name = kvp.Key };
                        }
                        // 注意：这里不覆盖现有文件，只是显示已存在的文件
                    }

                    UpdateFoldersList();
                    UpdatePreview();
                    UpdateUploadedList();
                }
                else
                {
                    UpdateUploadedList();
                }
            }
            catch (Exception ex)
            {
                // 静默失败，不影响导入流程
                System.Diagnostics.Debug.WriteLine($"加载已存在图片失败: {ex.Message}");
                MessageBox.Show($"加载已上传图片失败: {ex.Message}", "警告",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateUploadedList()
        {
            listViewUploaded.Items.Clear();

            if (_uploadedImages.Count == 0)
            {
                lblUploadedCount.Text = "已上传: 0 个图片组";
                return;
            }

            // 按图片组分组
            var imageGroups = _uploadedImages.GroupBy(img => img.ImageGroup).OrderBy(g => g.Key);
            int groupCount = imageGroups.Count();

            foreach (var group in imageGroups)
            {
                bool isFirstInGroup = true;
                foreach (var image in group.OrderBy(i => i.FolderName))
                {
                    var item = new ListViewItem(isFirstInGroup ? group.Key : "");
                    item.SubItems.Add(image.FolderName);
                    item.SubItems.Add(image.FileName);
                    item.SubItems.Add(image.Id.ToString());
                    item.Tag = image;
                    listViewUploaded.Items.Add(item);
                    isFirstInGroup = false;
                }
            }

            lblUploadedCount.Text = $"已上传: {groupCount} 个图片组, 共 {_uploadedImages.Count} 张图片";
        }

        private string? PromptInput(string prompt, string title, string defaultValue = "")
        {
            var form = new Form
            {
                Text = title,
                Size = new Size(400, 150),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var label = new Label { Text = prompt, Location = new Point(10, 20), AutoSize = true };
            var textBox = new TextBox { Location = new Point(10, 50), Size = new Size(360, 23), Text = defaultValue };
            var btnOk = new Button { Text = "确定", DialogResult = DialogResult.OK, Location = new Point(200, 80), Size = new Size(80, 30) };
            var btnCancel = new Button { Text = "取消", DialogResult = DialogResult.Cancel, Location = new Point(290, 80), Size = new Size(80, 30) };

            form.Controls.AddRange(new Control[] { label, textBox, btnOk, btnCancel });
            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;

            return form.ShowDialog() == DialogResult.OK ? textBox.Text : null;
        }
    }
}

