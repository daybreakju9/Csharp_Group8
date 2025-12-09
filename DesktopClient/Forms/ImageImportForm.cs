using ImageAnnotationApp.Services;
using ImageAnnotationApp.Models;
using ImageAnnotationApp.Controls;
using ImageAnnotationApp.Helpers;
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
        private int _folderListSortColumn = 0;
        private SortOrder _folderListSortOrder = SortOrder.Ascending;

        private class FolderData
        {
            public string Name { get; set; } = string.Empty;
            public List<string> FilePaths { get; set; } = new();
            public List<string> UploadedFileNames { get; set; } = new();
        }

        public ImageImportForm(Models.Queue queue)
        {
            _imageService = new ImageService();
            _queueService = new QueueService();
            _queue = queue;
            InitializeCustomComponents();
            _ = LoadExistingImagesAsync();
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
                Text = $"队列: {_queue.Name} | 所属项目: {_queue.ProjectName} | 对比图片数: {_queue.ComparisonCount} 张 | 当前总图片数: {_queue.TotalImageCount}",
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblTip = new Label
            {
                Text = "提示: 需要上传 " + _queue.ComparisonCount + " 个文件夹，每个文件夹包含相同文件名的图片。系统会自动按文件名分组。",
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
            var btnAddFromLocalFolder = new ToolStripButton("从本地文件夹添加");
            var btnRenameFolder = new ToolStripButton("重命名文件夹");
            var btnRemoveFolder = new ToolStripButton("删除文件夹");
            var btnSelectFiles = new ToolStripButton("选择文件");
            var btnClearFiles = new ToolStripButton("清空文件");

            btnAddFolder.Click += BtnAddFolder_Click;
            btnAddFromLocalFolder.Click += BtnAddFromLocalFolder_Click;
            btnRenameFolder.Click += BtnRenameFolder_Click;
            btnRemoveFolder.Click += BtnRemoveFolder_Click;
            btnSelectFiles.Click += BtnSelectFiles_Click;
            btnClearFiles.Click += BtnClearFiles_Click;

            toolStripFolders.Items.Add(btnAddFolder);
            toolStripFolders.Items.Add(btnAddFromLocalFolder);
            toolStripFolders.Items.Add(new ToolStripSeparator());
            toolStripFolders.Items.Add(btnRenameFolder);
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
            listViewFolders.ColumnClick += ListViewFolders_ColumnClick;

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
            if (_folders.Count >= _queue.ComparisonCount)
            {
                MessageBox.Show($"最多只能添加 {_queue.ComparisonCount} 个文件夹", "提示",
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

        private void BtnAddFromLocalFolder_Click(object? sender, EventArgs e)
        {
            if (_folders.Count >= _queue.ComparisonCount)
            {
                MessageBox.Show($"最多只能添加 {_queue.ComparisonCount} 个文件夹", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dialog = new FolderBrowserDialog
            {
                Description = "选择包含图片的文件夹",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var selectedPath = dialog.SelectedPath;
                var folderName = Path.GetFileName(selectedPath);

                // 如果文件夹名称为空（比如选择了驱动器根目录），使用完整路径的最后部分
                if (string.IsNullOrWhiteSpace(folderName))
                {
                    folderName = selectedPath.Replace(":", "").Replace("\\", "_");
                }

                // 检查名称是否已存在
                if (_folders.ContainsKey(folderName))
                {
                    var newName = PromptInput("文件夹名称已存在，请输入新名称:", "重命名", folderName);
                    if (string.IsNullOrWhiteSpace(newName))
                        return;

                    if (_folders.ContainsKey(newName))
                    {
                        MessageBox.Show("文件夹名称仍然存在", "提示",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    folderName = newName;
                }

                // 获取文件夹内的所有图片文件
                var imageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"
                };

                var allFiles = Directory.GetFiles(selectedPath);
                var imageFiles = new List<string>();

                foreach (var file in allFiles)
                {
                    var ext = Path.GetExtension(file);
                    if (imageExtensions.Contains(ext))
                    {
                        imageFiles.Add(file);
                    }
                }

                if (imageFiles.Count == 0)
                {
                    MessageBox.Show("该文件夹中没有找到图片文件", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 添加文件夹和文件
                _folders[folderName] = new FolderData
                {
                    Name = folderName,
                    FilePaths = new List<string>(imageFiles),
                    UploadedFileNames = new List<string>()
                };

                UpdateFoldersList();
                UpdatePreview();

                MessageBox.Show($"成功添加文件夹：{folderName}\n包含 {imageFiles.Count} 个图片文件", "成功",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnRenameFolder_Click(object? sender, EventArgs e)
        {
            if (listViewFolders.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选择要重命名的文件夹", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var oldFolderName = listViewFolders.SelectedItems[0].Text;
            if (!_folders.ContainsKey(oldFolderName))
                return;

            // 检查是否有已上传的文件
            if (_folders[oldFolderName].UploadedFileNames.Count > 0)
            {
                MessageBox.Show(
                    "该文件夹包含已上传的文件，无法重命名。\n\n" +
                    "已上传的文件在服务器中保留原文件夹名称。\n" +
                    "如需更改，请删除此文件夹后重新创建。",
                    "无法重命名",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var newFolderName = PromptInput("新文件夹名称:", "重命名文件夹", oldFolderName);
            if (string.IsNullOrWhiteSpace(newFolderName))
                return;

            if (newFolderName == oldFolderName)
                return;

            if (_folders.ContainsKey(newFolderName))
            {
                MessageBox.Show("文件夹名称已存在", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 保存旧文件夹的数据
            var folderData = _folders[oldFolderName];
            folderData.Name = newFolderName;

            // 删除旧键，添加新键
            _folders.Remove(oldFolderName);
            _folders[newFolderName] = folderData;

            UpdateFoldersList();
            UpdatePreview();

            MessageBox.Show($"文件夹已重命名：{oldFolderName} → {newFolderName}", "成功",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
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

                // 计算总文件数（本地文件 + 已上传文件）
                var totalCount = folder.FilePaths.Count + folder.UploadedFileNames.Count;
                item.SubItems.Add(totalCount.ToString());

                // 显示文件列表
                var fileList = new List<string>();

                // 添加已上传的文件（带标记）
                foreach (var fileName in folder.UploadedFileNames.Take(5))
                {
                    fileList.Add($"{fileName} (已上传)");
                }

                // 添加本地文件
                foreach (var filePath in folder.FilePaths
                             .Where(f => !string.IsNullOrWhiteSpace(f))
                             .Select(f => Path.GetFileName(f))
                             .Where(f => !string.IsNullOrWhiteSpace(f))
                             .Take(10 - folder.UploadedFileNames.Take(5).Count()))
                {
                    fileList.Add(filePath!);
                }

                var fileNames = string.Join("; ", fileList);
                if (totalCount > 10)
                    fileNames += $" ... (共 {totalCount} 个文件: {folder.UploadedFileNames.Count} 已上传, {folder.FilePaths.Count} 待上传)";
                else if (folder.UploadedFileNames.Count > 0)
                    fileNames += $" ({folder.UploadedFileNames.Count} 已上传, {folder.FilePaths.Count} 待上传)";

                item.SubItems.Add(fileNames);
                item.Tag = folder;

                // 如果有已上传的文件，改变颜色
                if (folder.UploadedFileNames.Count > 0)
                {
                    item.ForeColor = Color.DarkGreen;
                }

                listViewFolders.Items.Add(item);
            }
        }

        private void UpdatePreview()
        {
            // 获取所有文件名（包括已上传的和本地的）
            var allFileNames = new HashSet<string>();
            foreach (var folder in _folders.Values)
            {
                // 添加本地文件
                foreach (var filePath in folder.FilePaths)
                {
                    allFileNames.Add(Path.GetFileName(filePath));
                }
                // 添加已上传的文件
                foreach (var fileName in folder.UploadedFileNames)
                {
                    allFileNames.Add(fileName);
                }
            }

            // 更新预览列表
            listViewPreview.Items.Clear();
            listViewPreview.Columns.Clear();
            listViewPreview.Columns.Add("图片文件名", 300);

            // 使用文件夹的 Name 属性而不是字典的 Key，确保显示最新的名称
            var folderList = _folders.Values.OrderBy(f => f.Name).ToList();
            foreach (var folder in folderList)
            {
                listViewPreview.Columns.Add(folder.Name, 150);
            }
            listViewPreview.Columns.Add("状态", 100);

            foreach (var fileName in allFileNames.OrderBy(f => f))
            {
                var item = new ListViewItem(fileName);
                bool isComplete = true;
                bool allUploaded = true;

                // 按照列的顺序遍历文件夹
                foreach (var folder in folderList)
                {
                    var hasLocal = folder.FilePaths.Any(f => Path.GetFileName(f) == fileName);
                    var hasUploaded = folder.UploadedFileNames.Contains(fileName);

                    if (hasUploaded)
                    {
                        item.SubItems.Add("✓ (已上传)");
                    }
                    else if (hasLocal)
                    {
                        item.SubItems.Add("✓ (待上传)");
                        allUploaded = false;
                    }
                    else
                    {
                        item.SubItems.Add("✗");
                        isComplete = false;
                        allUploaded = false;
                    }

                    if (!hasLocal && !hasUploaded) isComplete = false;
                }

                // 设置状态
                if (allUploaded)
                {
                    item.SubItems.Add("已上传");
                    item.ForeColor = Color.DarkGreen;
                }
                else if (isComplete)
                {
                    item.SubItems.Add("完整");
                    item.ForeColor = Color.Black;
                }
                else
                {
                    item.SubItems.Add("不完整");
                    item.ForeColor = Color.Orange;
                }

                listViewPreview.Items.Add(item);
            }
        }

        private async Task BtnImport_ClickAsync()
        {
            if (_isUploading)
                return;

            if (_folders.Count == 0)
            {
                MessageBox.Show("请至少添加一个文件夹", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_folders.Count < _queue.ComparisonCount)
            {
                var result = MessageBox.Show(
                    $"当前只有 {_folders.Count} 个文件夹，但队列设置需要 {_queue.ComparisonCount} 个文件夹。是否继续？",
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
                // 准备上传数据（传递文件路径，由服务内部处理读取）
                var folderFiles = new Dictionary<string, List<string>>();

                foreach (var folder in _folders.Values)
                {
                    var files = new List<string>();
                    foreach (var filePath in folder.FilePaths)
                    {
                        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                        {
                            MessageBox.Show($"文件不存在: {filePath}", "警告",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            continue;
                        }

                        files.Add(filePath);
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
                    var errorMsg = "所有文件上传失败";
                    if (result.Errors.Count > 0)
                    {
                        errorMsg += "\n\n错误详情:\n" + string.Join("\n", result.Errors.Take(10));
                        if (result.Errors.Count > 10)
                            errorMsg += $"\n... 还有 {result.Errors.Count - 10} 个错误";
                    }
                    MessageBox.Show(errorMsg, "错误",
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
                        // 填充已上传的文件列表
                        _folders[kvp.Key].UploadedFileNames = kvp.Value;
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
            var imageGroups = _uploadedImages.GroupBy(img => img.ImageGroupId).OrderBy(g => g.Key);
            int groupCount = imageGroups.Count();

            foreach (var group in imageGroups)
            {
                bool isFirstInGroup = true;
                foreach (var image in group.OrderBy(i => i.FolderName))
                {
                    var item = new ListViewItem(isFirstInGroup ? group.Key.ToString() : "");
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

        private void ListViewFolders_ColumnClick(object? sender, ColumnClickEventArgs e)
        {
            // 如果点击的是同一列，则切换排序方向
            if (e.Column == _folderListSortColumn)
            {
                _folderListSortOrder = _folderListSortOrder == SortOrder.Ascending
                    ? SortOrder.Descending
                    : SortOrder.Ascending;
            }
            else
            {
                // 点击不同列，默认升序
                _folderListSortColumn = e.Column;
                _folderListSortOrder = SortOrder.Ascending;
            }

            // 应用排序
            listViewFolders.ListViewItemSorter = new ListViewItemComparer(_folderListSortColumn, _folderListSortOrder);
            listViewFolders.Sort();
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

    // ListView列排序比较器
    public class ListViewItemComparer : System.Collections.IComparer
    {
        private readonly int _column;
        private readonly SortOrder _sortOrder;

        public ListViewItemComparer(int column, SortOrder sortOrder)
        {
            _column = column;
            _sortOrder = sortOrder;
        }

        public int Compare(object? x, object? y)
        {
            if (x is not ListViewItem itemX || y is not ListViewItem itemY)
                return 0;

            string textX = _column < itemX.SubItems.Count ? itemX.SubItems[_column].Text : "";
            string textY = _column < itemY.SubItems.Count ? itemY.SubItems[_column].Text : "";

            int result;

            // 第二列（文件数量）按数字排序
            if (_column == 1)
            {
                if (int.TryParse(textX, out int numX) && int.TryParse(textY, out int numY))
                {
                    result = numX.CompareTo(numY);
                }
                else
                {
                    result = string.Compare(textX, textY, StringComparison.OrdinalIgnoreCase);
                }
            }
            else
            {
                // 其他列按字符串排序
                result = string.Compare(textX, textY, StringComparison.OrdinalIgnoreCase);
            }

            // 根据排序方向返回结果
            return _sortOrder == SortOrder.Descending ? -result : result;
        }
    }
}
