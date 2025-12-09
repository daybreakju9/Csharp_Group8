using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageAnnotationApp.Services;
using ImageAnnotationApp.Models;
using ImageAnnotationApp.Helpers;

namespace ImageAnnotationApp.Forms
{
    public partial class DataExportForm : BaseForm
    {
        private readonly ExportService _exportService;
        private readonly QueueService _queueService;
        private ComboBox cmbQueueFilter = null!;
        private ComboBox cmbFormat = null!;
        private ComboBox cmbProgressQueue = null!;
        private ComboBox cmbProgressFormat = null!;
        private Button btnExportSelections = null!;
        private Button btnExportProgress = null!;
        private ListView listViewQueues = null!;
        private List<Models.Queue> _allQueues = new();

        public DataExportForm()
        {
            _exportService = new ExportService();
            _queueService = new QueueService();
            InitializeCustomComponents();
            _ = LoadQueuesAsync();
        }

        private void InitializeCustomComponents()
        {
            this.Text = UIConstants.FormatWindowTitle("数据导出");
            this.Size = UIConstants.WindowSizes.Large;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = UIConstants.Fonts.Normal;
            this.BackColor = UIConstants.Colors.Background;

            // 导出选择记录面板
            var panelSelections = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                Padding = new Padding(UIConstants.Spacing.Medium),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = this.BackColor
            };

            var lblSelectionsTitle = new Label
            {
                Text = "导出选择记录",
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font(UIConstants.Fonts.Normal.FontFamily, 10F, FontStyle.Bold)
            };

            var lblQueue = new Label
            {
                Text = "选择队列:",
                Location = new Point(10, 40),
                AutoSize = true
            };

            cmbQueueFilter = new ComboBox
            {
                Location = new Point(100, 37),
                Size = new Size(300, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var lblFormat = new Label
            {
                Text = "导出格式:",
                Location = new Point(420, 40),
                AutoSize = true
            };

            cmbFormat = new ComboBox
            {
                Location = new Point(500, 37),
                Size = new Size(100, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFormat.Items.AddRange(new object[] { "CSV", "JSON" });
            cmbFormat.SelectedIndex = 0;

            btnExportSelections = new Button
            {
                Text = "导出选择记录",
                Location = new Point(620, 35),
                Size = UIConstants.ButtonSizes.Large,
                Font = UIConstants.Fonts.Normal,
                TextAlign = ContentAlignment.MiddleCenter
            };
            ApplyGhostButtonStyle(btnExportSelections);
            btnExportSelections.Click += async (s, e) => await ExportSelectionsAsync();

            var lblSelectionsTip = new Label
            {
                Text = "导出指定队列中所有用户的图片选择记录",
                Location = new Point(10, 75),
                AutoSize = true,
                ForeColor = Color.Gray,
                Font = UIConstants.Fonts.Small
            };

            panelSelections.Controls.AddRange(new Control[]
            {
                lblSelectionsTitle, lblQueue, cmbQueueFilter,
                lblFormat, cmbFormat, btnExportSelections, lblSelectionsTip
            });

            // 导出进度数据面板
            var panelProgress = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                Padding = new Padding(UIConstants.Spacing.Medium),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = this.BackColor
            };

            var lblProgressTitle = new Label
            {
                Text = "导出进度数据",
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font(UIConstants.Fonts.Normal.FontFamily, 10F, FontStyle.Bold)
            };

            var lblProgressQueue = new Label
            {
                Text = "选择队列:",
                Location = new Point(10, 40),
                AutoSize = true
            };

            cmbProgressQueue = new ComboBox
            {
                Location = new Point(100, 37),
                Size = new Size(300, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var lblProgressFormat = new Label
            {
                Text = "导出格式:",
                Location = new Point(420, 40),
                AutoSize = true
            };

            cmbProgressFormat = new ComboBox
            {
                Location = new Point(500, 37),
                Size = new Size(100, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbProgressFormat.Items.AddRange(new object[] { "CSV", "JSON" });
            cmbProgressFormat.SelectedIndex = 0;

            btnExportProgress = new Button
            {
                Text = "导出进度数据",
                Location = new Point(620, 35),
                Size = UIConstants.ButtonSizes.Large,
                Font = UIConstants.Fonts.Normal,
                TextAlign = ContentAlignment.MiddleCenter
            };
            ApplyGhostButtonStyle(btnExportProgress, Color.FromArgb(46, 204, 113));
            btnExportProgress.Click += async (s, e) =>
            {
                int? queueId = null;
                if (cmbProgressQueue.SelectedIndex > 0 && _allQueues.Count > 0)
                {
                    queueId = _allQueues[cmbProgressQueue.SelectedIndex - 1].Id;
                }
                await ExportProgressAsync(queueId);
            };

            var lblProgressTip = new Label
            {
                Text = "导出所有用户在各队列中的完成进度（不选择队列则导出所有队列的进度）",
                Location = new Point(10, 75),
                AutoSize = true,
                ForeColor = Color.Gray,
                Font = UIConstants.Fonts.Small
            };

            panelProgress.Controls.AddRange(new Control[]
            {
                lblProgressTitle, lblProgressQueue, cmbProgressQueue,
                lblProgressFormat, cmbProgressFormat, btnExportProgress, lblProgressTip
            });

            // 队列统计表格
            var panelStats = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(UIConstants.Spacing.Medium),
                BackColor = this.BackColor
            };

            var lblStatsTitle = new Label
            {
                Text = "数据统计",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font(UIConstants.Fonts.Normal.FontFamily, 10F, FontStyle.Bold)
            };

            var topPanelStats = new Panel
            {
                Dock = DockStyle.Top,
                Height = UIConstants.ButtonSizes.Medium.Height + UIConstants.Spacing.Large * 2,
                Padding = new Padding(UIConstants.Spacing.Medium),
                BackColor = this.BackColor
            };
            var flowStats = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = false,
                AutoScroll = false,
                BackColor = this.BackColor
            };
            var btnRefresh = UIConstants.CreateButton("刷新", UIConstants.ButtonSizes.Medium, UIConstants.Colors.PrimaryButton, null);
            btnRefresh.Font = UIConstants.Fonts.Normal;
            btnRefresh.Margin = new Padding(0, 0, UIConstants.Spacing.Medium, 0);
            ApplyGhostButtonStyle(btnRefresh);
            btnRefresh.Click += async (s, e) => await LoadQueuesAsync();
            flowStats.Controls.Add(btnRefresh);
            topPanelStats.Controls.Add(flowStats);

            listViewQueues = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = UIConstants.Fonts.Normal
            };
            listViewQueues.Columns.Add("所属项目", 150);
            listViewQueues.Columns.Add("队列名称", 200);
            listViewQueues.Columns.Add("总图片数", 100);
            listViewQueues.Columns.Add("图片组数", 100);

            panelStats.Controls.Add(listViewQueues);
            panelStats.Controls.Add(topPanelStats);
            panelStats.Controls.Add(lblStatsTitle);

            this.Controls.Add(panelStats);
            this.Controls.Add(panelProgress);
            this.Controls.Add(panelSelections);
        }

        private async Task LoadQueuesAsync()
        {
            try
            {
                _allQueues = await _queueService.GetAllAsync();

                // 更新选择记录导出下拉框
                cmbQueueFilter.Items.Clear();
                foreach (var queue in _allQueues)
                {
                    cmbQueueFilter.Items.Add($"{queue.ProjectName} - {queue.Name}");
                }
                if (cmbQueueFilter.Items.Count > 0)
                    cmbQueueFilter.SelectedIndex = 0;

                // 更新进度数据导出下拉框
                cmbProgressQueue.Items.Clear();
                cmbProgressQueue.Items.Add("所有队列");
                foreach (var queue in _allQueues)
                {
                    cmbProgressQueue.Items.Add($"{queue.ProjectName} - {queue.Name}");
                }
                cmbProgressQueue.SelectedIndex = 0;

                // 更新队列统计表格
                listViewQueues.Items.Clear();
                foreach (var queue in _allQueues)
                {
                    var item = new ListViewItem(queue.ProjectName);
                    item.SubItems.Add(queue.Name);
                    item.SubItems.Add(queue.TotalImageCount.ToString());
                    item.SubItems.Add(queue.GroupCount.ToString());
                    item.Tag = queue;
                    listViewQueues.Items.Add(item);
                }

                // 绑定/替换双击处理器
                listViewQueues.DoubleClick -= ListViewQueues_DoubleClickHandler;
                listViewQueues.DoubleClick += ListViewQueues_DoubleClickHandler;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载队列列表失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyGhostButtonStyle(Button btn, Color? hoverColor = null)
        {
            if (btn == null) return;

            var normalColor = UIConstants.Colors.Background;
            var hover = hoverColor ?? Color.FromArgb(64, 158, 255);
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

        private async void ListViewQueues_DoubleClickHandler(object? s, EventArgs e)
        {
            if (listViewQueues.SelectedItems.Count > 0)
            {
                var queue = listViewQueues.SelectedItems[0].Tag as Models.Queue;
                if (queue != null)
                {
                    var choice = ShowQuickExportChoice(queue);
                    if (choice == QuickExportChoice.Selections)
                        await QuickExportSelectionsAsync(queue.Id);
                    else if (choice == QuickExportChoice.Progress)
                        await QuickExportProgressAsync(queue.Id);
                }
            }
        }

        private enum QuickExportChoice
        {
            None,
            Selections,
            Progress
        }

        private QuickExportChoice ShowQuickExportChoice(Models.Queue queue)
        {
            var choice = QuickExportChoice.None;

            using var dialog = new Form
            {
                Text = "快速导出",
                Size = new Size(400, 200),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = UIConstants.Colors.Background
            };

            var lbl = new Label
            {
                Text = $"选择操作：\n队列：{queue.ProjectName} - {queue.Name}",
                AutoSize = true,
                Location = new Point(20, 20),
                Font = UIConstants.Fonts.Normal
            };

            var btnSelections = UIConstants.CreateButton("导出选择记录", UIConstants.ButtonSizes.Large, UIConstants.Colors.PrimaryButton, null);
            btnSelections.Font = UIConstants.Fonts.Normal;
            btnSelections.Location = new Point(30, 100);
            ApplyGhostButtonStyle(btnSelections);
            btnSelections.Click += (s, e) =>
            {
                choice = QuickExportChoice.Selections;
                dialog.DialogResult = DialogResult.OK;
                dialog.Close();
            };

            var btnProgress = UIConstants.CreateButton("导出进度数据", UIConstants.ButtonSizes.Large, UIConstants.Colors.PrimaryButton, null);
            btnProgress.Font = UIConstants.Fonts.Normal;
            btnProgress.Location = new Point(200, 100);
            ApplyGhostButtonStyle(btnProgress);
            btnProgress.Click += (s, e) =>
            {
                choice = QuickExportChoice.Progress;
                dialog.DialogResult = DialogResult.OK;
                dialog.Close();
            };

            dialog.Controls.Add(lbl);
            dialog.Controls.Add(btnSelections);
            dialog.Controls.Add(btnProgress);

            dialog.ShowDialog(this);
            return choice;
        }

        private async Task ExportSelectionsAsync()
        {
            if (cmbQueueFilter.SelectedIndex < 0 || _allQueues.Count == 0)
            {
                MessageBox.Show("请选择要导出的队列", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnExportSelections.Enabled = false;
                btnExportSelections.Text = "导出中...";
                UpdateStatus("正在导出选择记录...");

                var queue = _allQueues[cmbQueueFilter.SelectedIndex];
                var format = cmbFormat.SelectedItem?.ToString()?.ToLower() ?? "csv";

                var data = await _exportService.ExportSelectionsAsync(queue.Id, format);

                var fileName = $"selections_{queue.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.{format}";
                _exportService.SaveToFile(data, fileName);

                UpdateStatus("导出完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出选择记录失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("导出失败");
            }
            finally
            {
                btnExportSelections.Enabled = true;
                btnExportSelections.Text = "导出选择记录";
            }
        }

        private async Task ExportProgressAsync(int? queueId)
        {
            try
            {
                btnExportProgress.Enabled = false;
                btnExportProgress.Text = "导出中...";
                UpdateStatus("正在导出进度数据...");

                var format = cmbProgressFormat.SelectedItem?.ToString()?.ToLower() ?? "csv";

                var data = await _exportService.ExportProgressAsync(queueId, format);

                var fileName = $"progress_{(queueId.HasValue ? queueId.Value.ToString() : "all")}_{DateTime.Now:yyyyMMdd_HHmmss}.{format}";
                _exportService.SaveToFile(data, fileName);

                UpdateStatus("导出完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出进度数据失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("导出失败");
            }
            finally
            {
                btnExportProgress.Enabled = true;
                btnExportProgress.Text = "导出进度数据";
            }
        }

        private async Task QuickExportSelectionsAsync(int queueId)
        {
            try
            {
                var data = await _exportService.ExportSelectionsAsync(queueId, "csv");
                var fileName = $"selections_{queueId}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                _exportService.SaveToFile(data, fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出选择记录失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task QuickExportProgressAsync(int queueId)
        {
            try
            {
                var data = await _exportService.ExportProgressAsync(queueId, "csv");
                var fileName = $"progress_{queueId}_{DateTime.Now:yyyyMMdd_HHmms}.csv";
                _exportService.SaveToFile(data, fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出进度数据失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
