using ImageAnnotationApp.Services;
using ImageAnnotationApp.Models;
using ImageAnnotationApp.Controls;
using ImageAnnotationApp.Helpers;

namespace ImageAnnotationApp.Forms
{
    public partial class ImageSelectionForm : BaseForm
    {
        private readonly ImageService _imageService;
        private readonly SelectionService _selectionService;
        private readonly ImageCacheService _cacheService;
        private readonly int _queueId;
        private readonly string _queueName;
        private ImageGroup? _currentGroup;
        private int _selectedImageId = -1;
        private Panel panelImages = null!;
        private ProgressBar progressBar = null!;
        private Label lblProgress = null!;
        private Button btnSubmit = null!;
        private ToolStripButton btnBack = null!;
        private CheckBox chkAutoSubmit = null!;

        public ImageSelectionForm(int queueId, string queueName)
        {
            _imageService = new ImageService();
            _selectionService = new SelectionService();
            _cacheService = ImageCacheService.Instance;
            _queueId = queueId;
            _queueName = queueName;
            InitializeCustomComponents();
            _ = LoadNextGroupAsync();
        }

        private void InitializeCustomComponents()
        {
            this.Text = UIConstants.FormatWindowTitle("图片选择", _queueName);
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterParent;

            // 监听窗口大小变化
            this.Resize += ImageSelectionForm_Resize;
            this.SizeChanged += ImageSelectionForm_SizeChanged;
            this.FormClosing += ImageSelectionForm_FormClosing;

            // 顶部工具栏
            var toolStrip = new ToolStrip();
            btnBack = new ToolStripButton("返回");
            btnBack.Click += BtnBack_Click;
            toolStrip.Items.Add(btnBack);

            // 进度条
            progressBar = new ProgressBar
            {
                Dock = DockStyle.Top,
                Height = 25,
                Style = ProgressBarStyle.Continuous
            };

            lblProgress = new Label
            {
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 30,
                Text = "加载中..."
            };

            // 图片显示面板
            panelImages = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            panelImages.DoubleBuffered(true);

            // 底部控制面板
            var panelBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50
            };

            chkAutoSubmit = new CheckBox
            {
                Text = "自动提交",
                Location = new Point(10, 15),
                AutoSize = true
            };

            btnSubmit = new Button
            {
                Text = "提交选择",
                Location = new Point(150, 10),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSubmit.Click += async (s, e) => await SubmitSelectionAsync();

            panelBottom.Controls.Add(chkAutoSubmit);
            panelBottom.Controls.Add(btnSubmit);

            this.Controls.Add(panelImages);
            this.Controls.Add(panelBottom);
            this.Controls.Add(lblProgress);
            this.Controls.Add(progressBar);
            this.Controls.Add(toolStrip);
        }

        private bool _isResizing = false;
        private System.Windows.Forms.Timer? _resizeTimer;

        private void ImageSelectionForm_Resize(object? sender, EventArgs e)
        {
            // 防止频繁调用
            if (_resizeTimer == null)
            {
                _resizeTimer = new System.Windows.Forms.Timer();
                _resizeTimer.Interval = 300; 
                _resizeTimer.Tick += (s, args) =>
                {
                    _resizeTimer?.Stop();
                    _isResizing = false;
                    if (_currentGroup != null && _currentGroup.Images.Count > 0)
                    {
                        RedrawImages();
                    }
                };
            }

            if (!_isResizing)
            {
                _isResizing = true;
                _resizeTimer.Start();
            }
            else
            {
                _resizeTimer.Stop();
                _resizeTimer.Start();
            }
        }

        private void ImageSelectionForm_SizeChanged(object? sender, EventArgs e)
        {
            
        }

        private void ImageSelectionForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // 清理定时器资源
            if (_resizeTimer != null)
            {
                _resizeTimer.Stop();
                _resizeTimer.Dispose();
                _resizeTimer = null;
            }
        }

        private void RedrawImages()
        {
            if (_currentGroup == null || _currentGroup.Images.Count == 0)
                return;

            panelImages.SuspendLayout();

            try
            {
                int imageCount = _currentGroup.Images.Count;

                int panelWidth = panelImages.ClientSize.Width - 20;
                int panelHeight = panelImages.ClientSize.Height - 20;

                // 确保最小尺寸
                if (panelWidth < 100 || panelHeight < 100)
                    return;

                int labelHeight = 20;
                int spacing = 10;

                // 计算最优的行列布局
                int bestCols = 1;
                int bestImageSize = 0;

                // 尝试不同的列数，找到能让图片最大的配置
                for (int testCols = 1; testCols <= imageCount; testCols++)
                {
                    int testRows = (int)Math.Ceiling((double)imageCount / testCols);

                    // 计算在这个布局下图片能有多大
                    int maxWidth = (panelWidth - spacing) / testCols - spacing;
                    int maxHeight = (panelHeight - spacing) / testRows - labelHeight - spacing;

                    // 保持1:1比例
                    int testImageSize = Math.Min(maxWidth, maxHeight);

                    // 如果这个配置能让图片更大，则采用
                    if (testImageSize > bestImageSize)
                    {
                        bestImageSize = testImageSize;
                        bestCols = testCols;
                    }
                }

                int cols = bestCols;
                int rows = (int)Math.Ceiling((double)imageCount / cols);
                int imageSize = bestImageSize;

                // 确保最小尺寸
                imageSize = Math.Max(100, imageSize);

                // 计算总尺寸
                int totalWidth = cols * (imageSize + spacing) + spacing;
                int totalHeight = rows * (imageSize + labelHeight + spacing) + spacing;

                // 居中显示
                int startX = Math.Max(0, (panelWidth - totalWidth + spacing * 2) / 2);
                int startY = spacing;

                // 重新定位和调整大小现有的图片面板
                for (int i = 0; i < panelImages.Controls.Count && i < _currentGroup.Images.Count; i++)
                {
                    if (panelImages.Controls[i] is Panel imagePanel)
                    {
                        int row = i / cols;
                        int col = i % cols;

                        int x = startX + col * (imageSize + spacing);
                        int y = startY + row * (imageSize + labelHeight + spacing);

                        imagePanel.Size = new Size(imageSize, imageSize + labelHeight);
                        imagePanel.Location = new Point(x, y);

                        // 调整内部 PictureBox 和 Label 大小
                        foreach (Control control in imagePanel.Controls)
                        {
                            if (control is PictureBox pictureBox)
                            {
                                pictureBox.Size = new Size(imageSize, imageSize);
                                pictureBox.Location = new Point(0, 0);
                            }
                            else if (control is Label label)
                            {
                                label.Size = new Size(imageSize, labelHeight);
                                label.Location = new Point(0, imageSize);
                            }
                        }
                    }
                }
            }
            finally
            {
                panelImages.ResumeLayout(true);
            }
        }

        private async Task LoadNextGroupAsync()
        {
            try
            {
                panelImages.Controls.Clear();
                lblProgress.Text = "加载中...";
                UpdateStatus("正在加载图片组...");

                // 获取进度
                var progress = await _selectionService.GetProgressAsync(_queueId);
                if (progress != null)
                {
                    progressBar.Maximum = progress.TotalGroups;
                    progressBar.Value = progress.CompletedGroups;
                    lblProgress.Text = $"进度: {progress.CompletedGroups}/{progress.TotalGroups} ({progress.ProgressPercentage:F1}%)";
                }

                // 获取下一组图片
                _currentGroup = await _imageService.GetNextGroupAsync(_queueId);

                if (_currentGroup == null || _currentGroup.Images.Count == 0)
                {
                    MessageBox.Show("恭喜！您已完成所有图片标注！", "完成",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    BtnBack_Click(null, EventArgs.Empty);
                    return;
                }

                UpdateStatus($"正在显示图片组: {_currentGroup.GroupName}");

                // 预加载图片到缓存（后台任务）
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var imagePaths = _currentGroup.Images.Select(img => img.FilePath).ToArray();
                        await _cacheService.PreloadImagesAsync(imagePaths, _imageService);
                        System.Diagnostics.Debug.WriteLine($"预加载完成，共 {imagePaths.Length} 张图片");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"预加载失败: {ex.Message}");
                    }
                });

                // 显示图片
                DisplayImages();

                UpdateStatus($"已加载图片组: {_currentGroup.GroupName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载图片失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("加载失败");
            }
        }

        private void DisplayImages()
        {
            if (_currentGroup == null) return;

            panelImages.Controls.Clear();
            _selectedImageId = -1;

            int imageCount = _currentGroup.Images.Count;
            if (imageCount == 0) return;

            int panelWidth = panelImages.ClientSize.Width - 20;
            int panelHeight = panelImages.ClientSize.Height - 20;

            int labelHeight = 20; // 标签高度
            int spacing = 10; // 图片间距

            // 计算最优的行列布局，使图片尽可能大
            int bestCols = 1;
            int bestImageSize = 0;

            // 尝试不同的列数，找到能让图片最大的配置
            for (int testCols = 1; testCols <= imageCount; testCols++)
            {
                int testRows = (int)Math.Ceiling((double)imageCount / testCols);

                // 计算在这个布局下图片能有多大
                int maxWidth = (panelWidth - spacing) / testCols - spacing;
                int maxHeight = (panelHeight - spacing) / testRows - labelHeight - spacing;

                // 保持1:1比例
                int testImageSize = Math.Min(maxWidth, maxHeight);

                // 如果这个配置能让图片更大，则采用
                if (testImageSize > bestImageSize)
                {
                    bestImageSize = testImageSize;
                    bestCols = testCols;
                }
            }

            int cols = bestCols;
            int rows = (int)Math.Ceiling((double)imageCount / cols);
            int imageSize = bestImageSize;

            // 确保最小尺寸
            imageSize = Math.Max(100, imageSize);

            // 计算总尺寸
            int totalWidth = cols * (imageSize + spacing) + spacing;
            int totalHeight = rows * (imageSize + labelHeight + spacing) + spacing;

            // 居中显示
            int startX = Math.Max(0, (panelWidth - totalWidth + spacing * 2) / 2);
            int startY = spacing;

            for (int i = 0; i < _currentGroup.Images.Count; i++)
            {
                var image = _currentGroup.Images[i];
                int row = i / cols;
                int col = i % cols;

                int x = startX + col * (imageSize + spacing);
                int y = startY + row * (imageSize + labelHeight + spacing);

                // 创建图片容器面板
                var imagePanel = new Panel
                {
                    Size = new Size(imageSize, imageSize + labelHeight),
                    Location = new Point(x, y),
                    BorderStyle = BorderStyle.None
                };

                var pictureBox = new OptimizedPictureBox
                {
                    Size = new Size(imageSize, imageSize),
                    Location = new Point(0, 0),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BorderStyle = BorderStyle.FixedSingle,
                    Tag = image,
                    Cursor = Cursors.Hand
                };

                pictureBox.Click += PictureBox_Click;

                // 创建标签显示文件夹名
                var lblFolderName = new Label
                {
                    Text = image.FolderName,
                    Size = new Size(imageSize, labelHeight),
                    Location = new Point(0, imageSize),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Microsoft YaHei UI", 9F),
                    ForeColor = Color.DarkGray
                };

                imagePanel.Controls.Add(pictureBox);
                imagePanel.Controls.Add(lblFolderName);

                // 异步加载图片
                LoadImageAsync(pictureBox, image.FilePath);

                panelImages.Controls.Add(imagePanel);
            }
        }

        private async void LoadImageAsync(PictureBox pictureBox, string imagePath)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"开始加载图片: {imagePath}");

                // 先检查缓存
                var cachedData = _cacheService.GetImageData(imagePath);
                byte[]? imageData;

                if (cachedData != null)
                {
                    System.Diagnostics.Debug.WriteLine($"从缓存加载图片: {imagePath}");
                    imageData = cachedData;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"从服务器下载图片: {imagePath}");
                    imageData = await _imageService.GetImageDataAsync(imagePath);

                    if (imageData == null || imageData.Length == 0)
                    {
                        pictureBox.BackColor = Color.LightGray;
                        System.Diagnostics.Debug.WriteLine($"图片数据为空: {imagePath}");
                        return;
                    }

                    // 添加到缓存
                    _cacheService.AddImageData(imagePath, imageData);
                    System.Diagnostics.Debug.WriteLine($"图片已添加到缓存: {imagePath}, 大小: {imageData.Length} 字节");
                }

                // 释放旧的图片资源
                if (pictureBox.Image != null)
                {
                    var oldImage = pictureBox.Image;
                    pictureBox.Image = null;
                    oldImage.Dispose();
                }

                // 创建图片副本以避免GDI+错误
                // 不直接使用MemoryStream，而是创建Bitmap副本
                using (var ms = new MemoryStream(imageData))
                {
                    using (var tempImage = System.Drawing.Image.FromStream(ms))
                    {
                        // 创建新的Bitmap作为副本
                        var bitmap = new Bitmap(tempImage);
                        pictureBox.Image = bitmap;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"图片加载成功: {imagePath}");
            }
            catch (Exception ex)
            {
                // 加载失败时显示占位符和错误信息
                pictureBox.BackColor = Color.LightGray;
                var errorMsg = ex.Message.Length > 50 ? ex.Message.Substring(0, 50) + "..." : ex.Message;
                System.Diagnostics.Debug.WriteLine($"图片加载失败: {imagePath}, 错误: {ex.Message}\n堆栈: {ex.StackTrace}");
            }
        }

        private async void PictureBox_Click(object? sender, EventArgs e)
        {
            if (sender is not PictureBox pictureBox || pictureBox.Tag is not Models.Image image)
                return;

            // 清除之前的所有选择（确保只选择一张图片）
            foreach (Control control in panelImages.Controls)
            {
                if (control is Panel imagePanel)
                {
                    // 查找 Panel 中的 PictureBox
                    foreach (Control child in imagePanel.Controls)
                    {
                        if (child is PictureBox pb)
                        {
                            pb.BorderStyle = BorderStyle.FixedSingle;
                            pb.BackColor = Color.Transparent;
                        }
                    }
                    imagePanel.BackColor = Color.Transparent;
                }
            }

            // 标记当前选择
            pictureBox.BorderStyle = BorderStyle.Fixed3D;
            pictureBox.BackColor = Color.LightGreen;
            
            if (pictureBox.Parent is Panel parentPanel)
            {
                parentPanel.BackColor = Color.LightBlue;
            }
            
            _selectedImageId = image.Id;

            // 自动提交
            if (chkAutoSubmit.Checked)
            {
                await Task.Delay(100); 
                await SubmitSelectionAsync();
            }
        }

        private async Task SubmitSelectionAsync()
        {
            if (_selectedImageId == -1)
            {
                MessageBox.Show("请选择一张图片", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_currentGroup == null) return;

            try
            {
                btnSubmit.Enabled = false;
                btnBack.Enabled = false;
                btnSubmit.Text = "提交中...";
                UpdateStatus("正在提交选择...");

                var dto = new CreateSelectionDto
                {
                    QueueId = _queueId,
                    ImageGroupId = _currentGroup.Id,
                    SelectedImageId = _selectedImageId
                };

                await _selectionService.CreateAsync(dto);

                UpdateStatus("提交成功，正在加载下一组...");

                // 加载下一组
                await LoadNextGroupAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"提交失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("提交失败");
            }
            finally
            {
                btnSubmit.Enabled = true;
                btnBack.Enabled = true;
                btnSubmit.Text = "提交选择";
            }
        }

        private void BtnBack_Click(object? sender, EventArgs e)
        {
            // 使用NavigationManager返回上一层
            NavigateBack();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 清理定时器资源
                if (_resizeTimer != null)
                {
                    _resizeTimer.Stop();
                    _resizeTimer.Dispose();
                    _resizeTimer = null;
                }

                // 释放所有PictureBox中的图片资源
                foreach (Control control in panelImages.Controls)
                {
                    if (control is Panel imagePanel)
                    {
                        foreach (Control child in imagePanel.Controls)
                        {
                            if (child is PictureBox pb && pb.Image != null)
                            {
                                pb.Image.Dispose();
                                pb.Image = null;
                            }
                        }
                    }
                }
            }

            base.Dispose(disposing);
        }
    }
}
