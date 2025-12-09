using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageAnnotationApp.Services;
using ImageAnnotationApp.Models;
using ImageAnnotationApp.Helpers;

namespace ImageAnnotationApp.Forms
{
    public partial class QueueManagementForm : BaseForm
    {
        private readonly QueueService _queueService;
        private readonly ProjectService _projectService;
        private ListView listView = null!;
        private ComboBox cmbProjectFilter = null!;
        private List<Project> _allProjects = new();

        public QueueManagementForm()
        {
            _queueService = new QueueService();
            _projectService = new ProjectService();
            InitializeCustomComponents();
            _ = LoadProjectsAsync();
        }

        private void InitializeCustomComponents()
        {
            this.Text = UIConstants.FormatWindowTitle("队列管理");
            this.Size = UIConstants.WindowSizes.Large;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = UIConstants.Fonts.Normal;
            this.BackColor = UIConstants.Colors.Background;

            // 筛选面板（置于最顶端）
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 54,
                Padding = new Padding(10),
                BackColor = this.BackColor
            };

            var lblProject = new Label
            {
                Text = "项目筛选:",
                Location = new Point(10, 15),
                AutoSize = true,
                Font = UIConstants.Fonts.Normal
            };

            cmbProjectFilter = new ComboBox
            {
                Location = new Point(100, 12),
                Size = new Size(250, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UIConstants.Fonts.Normal
            };
            cmbProjectFilter.SelectedIndexChanged += async (s, e) => await LoadQueuesAsync();

            filterPanel.Controls.Add(lblProject);
            filterPanel.Controls.Add(cmbProjectFilter);

            // 按钮面板（位于筛选面板下方）
            var buttonPanel = new Panel
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
                BackColor = this.BackColor
            };

            var btnAdd = new Button
            {
                Text = "新建队列",
                Size = UIConstants.ButtonSizes.Medium,
                Font = UIConstants.Fonts.Normal,
                Margin = new Padding(0, 0, UIConstants.Spacing.Medium, 0)
            };
            btnAdd.Click += BtnAdd_ClickAsync;

            var btnEdit = new Button
            {
                Text = "编辑",
                Size = UIConstants.ButtonSizes.Medium,
                Font = UIConstants.Fonts.Normal,
                Margin = new Padding(0, 0, UIConstants.Spacing.Medium, 0)
            };
            btnEdit.Click += BtnEdit_ClickAsync;

            var btnManage = new Button
            {
                Text = "管理队列",
                Size = UIConstants.ButtonSizes.Medium,
                Font = UIConstants.Fonts.Normal,
                Margin = new Padding(0, 0, UIConstants.Spacing.Medium, 0)
            };
            btnManage.Click += BtnManage_Click;

            var btnDelete = new Button
            {
                Text = "删除",
                Size = UIConstants.ButtonSizes.Medium,
                Font = UIConstants.Fonts.Normal,
                Margin = new Padding(0, 0, UIConstants.Spacing.Medium, 0)
            };
            btnDelete.Click += async (s, e) => await BtnDelete_ClickAsync();

            var btnRefresh = new Button
            {
                Text = "刷新",
                Size = UIConstants.ButtonSizes.Medium,
                Font = UIConstants.Fonts.Normal,
                Margin = new Padding(0, 0, UIConstants.Spacing.Medium, 0)
            };
            btnRefresh.Click += async (s, e) => await LoadQueuesAsync();

            // 应用幽灵按钮样式（默认与背景相同、有细边框，悬停变天蓝色）
            ApplyGhostButtonStyle(btnAdd);
            ApplyGhostButtonStyle(btnEdit);
            ApplyGhostButtonStyle(btnManage);
            ApplyGhostButtonStyle(btnDelete);
            ApplyGhostButtonStyle(btnRefresh);

            flow.Controls.Add(btnAdd);
            flow.Controls.Add(btnEdit);
            flow.Controls.Add(btnManage);
            flow.Controls.Add(btnDelete);
            flow.Controls.Add(btnRefresh);
            buttonPanel.Controls.Add(flow);

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
            listView.Columns.Add("队列名称", 200);
            listView.Columns.Add("所属项目", 200);
            listView.Columns.Add("对比图片数", 100);
            listView.Columns.Add("总图片数", 100);
            listView.Columns.Add("图片组数", 100);
            listView.Columns.Add("创建时间", 150);
            listView.Columns.Add("操作提示", 150);
            listView.DoubleClick += ListView_DoubleClick;

            // Controls 添加顺序：先 listView，再按钮面板，再筛选面板
            this.Controls.Add(listView);
            this.Controls.Add(buttonPanel);
            this.Controls.Add(filterPanel);
        }

        // 幽灵按钮样式工具（与其它窗体一致）
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

        private async Task LoadProjectsAsync()
        {
            try
            {
                UpdateStatus(UIConstants.StatusMessages.Loading);
                _allProjects = await _projectService.GetAllAsync();
                cmbProjectFilter.Items.Clear();
                cmbProjectFilter.Items.Add("全部项目");

                foreach (var project in _allProjects)
                {
                    cmbProjectFilter.Items.Add(project.Name);
                }

                cmbProjectFilter.SelectedIndex = 0;
                UpdateStatus(UIConstants.StatusMessages.Ready);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载项目列表失败: {ex.Message}", UIConstants.MessageTitles.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus(UIConstants.Messages.LoadFailed);
            }
        }

        private async Task LoadQueuesAsync()
        {
            try
            {
                UpdateStatus(UIConstants.StatusMessages.Loading);
                listView.Items.Clear();

                int? projectId = null;
                if (cmbProjectFilter.SelectedIndex > 0 && _allProjects.Count > 0)
                {
                    projectId = _allProjects[cmbProjectFilter.SelectedIndex - 1].Id;
                }

                var queues = await _queueService.GetAllAsync(projectId);

                foreach (var queue in queues)
                {
                    var item = new ListViewItem(queue.Id.ToString());
                    item.SubItems.Add(queue.Name);
                    item.SubItems.Add(queue.ProjectName);
                    item.SubItems.Add(queue.ComparisonCount.ToString());
                    item.SubItems.Add(queue.TotalImageCount.ToString());
                    item.SubItems.Add(queue.GroupCount.ToString());
                    item.SubItems.Add(queue.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
                    item.SubItems.Add("双击管理");
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
        }

        private async void BtnAdd_ClickAsync(object? sender, EventArgs e)
        {
            try
            {
                if (_allProjects.Count == 0)
                {
                    MessageBox.Show("请先创建项目", UIConstants.MessageTitles.Warning,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var projectId = ShowQueueDialog(null);
                if (projectId.HasValue)
                {
                    await LoadQueuesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{UIConstants.Messages.CreateFailed}: {ex.Message}", UIConstants.MessageTitles.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnEdit_ClickAsync(object? sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0)
            {
                MessageBox.Show(UIConstants.Messages.SelectItem, UIConstants.MessageTitles.Warning,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var queue = listView.SelectedItems[0].Tag as Models.Queue;
            if (queue == null) return;

            try
            {
                var projectId = ShowQueueDialog(queue);
                if (projectId.HasValue)
                {
                    await LoadQueuesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{UIConstants.Messages.UpdateFailed}: {ex.Message}", UIConstants.MessageTitles.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int? ShowQueueDialog(Models.Queue? existingQueue)
        {
            var form = new Form
            {
                Text = existingQueue == null ? "新建队列" : "编辑队列",
                Size = new Size(450, 250),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lblProject = new Label { Text = "所属项目:", Location = new Point(20, 20), AutoSize = true };
            var cmbProject = new ComboBox
            {
                Location = new Point(122, 17),
                Size = new Size(280, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "Name",
                ValueMember = "Id"
            };
            foreach (var project in _allProjects)
            {
                cmbProject.Items.Add(project);
            }
            if (existingQueue != null)
            {
                var projectIndex = _allProjects.FindIndex(p => p.Id == existingQueue.ProjectId);
                if (projectIndex >= 0) cmbProject.SelectedIndex = projectIndex;
            }
            else if (cmbProject.Items.Count > 0)
            {
                cmbProject.SelectedIndex = 0;
            }
            cmbProject.Enabled = existingQueue == null; 

            var lblName = new Label { Text = "队列名称:", Location = new Point(20, 60), AutoSize = true };
            var txtName = new TextBox
            {
                Location = new Point(122, 57),
                Size = new Size(280, 23),
                Text = existingQueue?.Name ?? ""
            };

            var lblImageCount = new Label { Text = "对比图片数:", Location = new Point(20, 100), AutoSize = true };
            var numImageCount = new NumericUpDown
            {
                Location = new Point(122, 97),
                Size = new Size(100, 23),
                Minimum = 2,
                Maximum = 10,
                Value = existingQueue?.ComparisonCount ?? 3
            };

            var btnOk = new Button
            {
                Text = "确定",
                Location = new Point(112, 150),
                Size = UIConstants.ButtonSizes.Medium
            };
            var btnCancel = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Location = new Point(222, 150),
                Size = UIConstants.ButtonSizes.Medium
            };

            bool success = false;
            btnOk.Click += async (s, e) =>
            {
                if (cmbProject.SelectedItem is Project selectedProject && !string.IsNullOrWhiteSpace(txtName.Text))
                {
                    try
                    {
                        UpdateStatus(UIConstants.StatusMessages.Saving);
                        if (existingQueue == null)
                        {
                            await _queueService.CreateAsync(new CreateQueueDto
                            {
                                ProjectId = selectedProject.Id,
                                Name = txtName.Text.Trim(),
                                ComparisonCount = (int)numImageCount.Value
                            });
                            MessageBox.Show(UIConstants.Messages.CreateSuccess, UIConstants.MessageTitles.Success,
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            await _queueService.UpdateAsync(existingQueue.Id, new UpdateQueueDto
                            {
                                Name = txtName.Text.Trim(),
                                ComparisonCount = (int)numImageCount.Value
                            });
                            MessageBox.Show(UIConstants.Messages.UpdateSuccess, UIConstants.MessageTitles.Success,
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        success = true;
                        form.DialogResult = DialogResult.OK;
                        form.Close();
                        UpdateStatus(UIConstants.StatusMessages.Ready);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"操作失败: {ex.Message}", UIConstants.MessageTitles.Error,
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        UpdateStatus(UIConstants.Messages.SaveFailed);
                    }
                }
                else
                {
                    MessageBox.Show(UIConstants.Messages.FillRequired, UIConstants.MessageTitles.Warning,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            form.Controls.AddRange(new Control[]
            {
                lblProject, cmbProject,
                lblName, txtName,
                lblImageCount, numImageCount,
                btnOk, btnCancel
            });
            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;

            if (form.ShowDialog() == DialogResult.OK && success)
            {
                if (cmbProject.SelectedItem is Project selectedProject)
                {
                    return selectedProject.Id;
                }
            }
            return null;
        }

        private void BtnManage_Click(object? sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0)
            {
                MessageBox.Show(UIConstants.Messages.SelectItem, UIConstants.MessageTitles.Warning,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var queue = listView.SelectedItems[0].Tag as Models.Queue;
            if (queue == null) return;

            OpenQueueDetailManagement(queue);
        }

        private void ListView_DoubleClick(object? sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0) return;

            var queue = listView.SelectedItems[0].Tag as Models.Queue;
            if (queue == null) return;

            OpenQueueDetailManagement(queue);
        }

        private void OpenQueueDetailManagement(Models.Queue queue)
        {
            try
            {
                var detailForm = new QueueDetailManagementForm(queue);
                detailForm.ShowDialog();
                _ = LoadQueuesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开队列管理界面失败: {ex.Message}", UIConstants.MessageTitles.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task BtnDelete_ClickAsync()
        {
            if (listView.SelectedItems.Count == 0)
            {
                MessageBox.Show(UIConstants.Messages.SelectItem, UIConstants.MessageTitles.Warning,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var queue = listView.SelectedItems[0].Tag as Models.Queue;
            if (queue == null) return;

            var result = MessageBox.Show(
                $"确定要删除队列 '{queue.Name}' 吗？",
                UIConstants.MessageTitles.Confirm,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            try
            {
                UpdateStatus(UIConstants.StatusMessages.Processing);
                await _queueService.DeleteAsync(queue.Id);
                MessageBox.Show(UIConstants.Messages.DeleteSuccess, UIConstants.MessageTitles.Success,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadQueuesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{UIConstants.Messages.DeleteFailed}: {ex.Message}", UIConstants.MessageTitles.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus(UIConstants.Messages.DeleteFailed);
            }
        }

        private string? PromptInput(string prompt, string title, string defaultValue = "")
        {
            var form = new Form
            {
                Text = title,
                Size = new Size(400, 150),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent
            };

            var label = new Label { Text = prompt, Location = new Point(10, 20), AutoSize = true };
            var textBox = new TextBox { Location = new Point(10, 50), Size = new Size(360, 23), Text = defaultValue };
            var btnOk = new Button { Text = "确定", DialogResult = DialogResult.OK, Location = new Point(200, 80) };
            var btnCancel = new Button { Text = "取消", DialogResult = DialogResult.Cancel, Location = new Point(290, 80) };

            form.Controls.AddRange(new Control[] { label, textBox, btnOk, btnCancel });
            return form.ShowDialog() == DialogResult.OK ? textBox.Text : null;
        }
    }
}
