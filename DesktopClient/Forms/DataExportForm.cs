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

            // 导出选择记录面板
            var panelSelections = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                Padding = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblSelectionsTitle = new Label
            {
                Text = "导出选择记录",
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font("Microsoft YaHei", 10F, FontStyle.Bold)
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
                Size = new Size(150, 30),
                BackColor = UIConstants.Colors.PrimaryButton,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnExportSelections.Click += async (s, e) => await ExportSelectionsAsync();

            var lblSelectionsTip = new Label
            {
                Text = "导出指定队列中所有用户的图片选择记录",
                Location = new Point(10, 75),
                AutoSize = true,
                ForeColor = Color.Gray,
                Font = new Font("Microsoft YaHei", 9F)
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
                Padding = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblProgressTitle = new Label
            {
                Text = "导出进度数据",
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font("Microsoft YaHei", 10F, FontStyle.Bold)
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
                Size = new Size(150, 30),
                BackColor = UIConstants.Colors.SuccessButton,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
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
                Font = new Font("Microsoft YaHei", 9F)
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
                Padding = new Padding(10)
            };

            var lblStatsTitle = new Label
            {
                Text = "数据统计",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Microsoft YaHei", 10F, FontStyle.Bold)
            };

            var toolStrip = new ToolStrip { Dock = DockStyle.Top };
            var btnRefresh = new ToolStripButton("刷新");
            btnRefresh.Click += async (s, e) => await LoadQueuesAsync();
            toolStrip.Items.Add(btnRefresh);

            listViewQueues = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            listViewQueues.Columns.Add("所属项目", 150);
            listViewQueues.Columns.Add("队列名称", 200);
            listViewQueues.Columns.Add("总图片数", 100);
            listViewQueues.Columns.Add("图片组数", 100);

            panelStats.Controls.Add(listViewQueues);
            panelStats.Controls.Add(toolStrip);
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

        private async void ListViewQueues_DoubleClickHandler(object? s, EventArgs e)
        {
            if (listViewQueues.SelectedItems.Count > 0)
            {
                var queue = listViewQueues.SelectedItems[0].Tag as Models.Queue;
                if (queue != null)
                {
                    var result = MessageBox.Show(
                        $"选择操作:\n\n点击'是'导出选择记录\n点击'否'导出进度数据",
                        "快速导出",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        await QuickExportSelectionsAsync(queue.Id);
                    }
                    else if (result == DialogResult.No)
                    {
                        await QuickExportProgressAsync(queue.Id);
                    }
                }
            }
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