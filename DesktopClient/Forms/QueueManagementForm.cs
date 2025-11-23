using ImageAnnotationApp.Services;
using ImageAnnotationApp.Models;

namespace ImageAnnotationApp.Forms
{
    public partial class QueueManagementForm : Form
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
            LoadProjectsAsync();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "队列管理";
            this.Size = new Size(1200, 700);

            // 筛选面板
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(10)
            };

            var lblProject = new Label
            {
                Text = "项目筛选:",
                Location = new Point(10, 15),
                AutoSize = true
            };

            cmbProjectFilter = new ComboBox
            {
                Location = new Point(100, 12),
                Size = new Size(250, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbProjectFilter.SelectedIndexChanged += async (s, e) => await LoadQueuesAsync();

            filterPanel.Controls.Add(lblProject);
            filterPanel.Controls.Add(cmbProjectFilter);

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
            listView.Columns.Add("所属项目", 200);
            listView.Columns.Add("对比图片数", 100);
            listView.Columns.Add("总图片数", 100);
            listView.Columns.Add("图片组数", 100);
            listView.Columns.Add("创建时间", 150);

            // 工具栏
            var toolStrip = new ToolStrip();
            var btnAdd = new ToolStripButton("新建队列");
            var btnEdit = new ToolStripButton("编辑");
            var btnImport = new ToolStripButton("导入图片");
            var btnDelete = new ToolStripButton("删除");
            var btnRefresh = new ToolStripButton("刷新");

            btnAdd.Click += BtnAdd_ClickAsync;
            btnEdit.Click += BtnEdit_ClickAsync;
            btnImport.Click += BtnImport_Click;
            btnDelete.Click += async (s, e) => await BtnDelete_ClickAsync();
            btnRefresh.Click += async (s, e) => await LoadQueuesAsync();

            toolStrip.Items.Add(btnAdd);
            toolStrip.Items.Add(btnEdit);
            toolStrip.Items.Add(btnImport);
            toolStrip.Items.Add(btnDelete);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(btnRefresh);

            this.Controls.Add(listView);
            this.Controls.Add(toolStrip);
            this.Controls.Add(filterPanel);
            toolStrip.Dock = DockStyle.Top;
        }

        private async Task LoadProjectsAsync()
        {
            try
            {
                _allProjects = await _projectService.GetAllAsync();
                cmbProjectFilter.Items.Clear();
                cmbProjectFilter.Items.Add("全部项目");
                
                foreach (var project in _allProjects)
                {
                    cmbProjectFilter.Items.Add(project.Name);
                }
                
                cmbProjectFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载项目列表失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadQueuesAsync()
        {
            try
            {
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
                    item.SubItems.Add(queue.ImageCount.ToString());
                    item.SubItems.Add(queue.TotalImages.ToString());
                    var groupCount = queue.ImageCount > 0 ? queue.TotalImages / queue.ImageCount : 0;
                    item.SubItems.Add(groupCount.ToString());
                    item.SubItems.Add(queue.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
                    item.Tag = queue;
                    listView.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载队列列表失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnAdd_ClickAsync(object? sender, EventArgs e)
        {
            try
            {
                if (_allProjects.Count == 0)
                {
                    MessageBox.Show("请先创建项目", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var projectId = await ShowQueueDialogAsync(null);
                if (projectId.HasValue)
                {
                    await LoadQueuesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"创建队列失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnEdit_ClickAsync(object? sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选择要编辑的队列", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var queue = listView.SelectedItems[0].Tag as Models.Queue;
            if (queue == null) return;

            try
            {
                var projectId = await ShowQueueDialogAsync(queue);
                if (projectId.HasValue)
                {
                    await LoadQueuesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新队列失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<int?> ShowQueueDialogAsync(Models.Queue? existingQueue)
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
                Location = new Point(120, 17),
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
            cmbProject.Enabled = existingQueue == null; // 编辑时不允许修改项目

            var lblName = new Label { Text = "队列名称:", Location = new Point(20, 60), AutoSize = true };
            var txtName = new TextBox
            {
                Location = new Point(120, 57),
                Size = new Size(280, 23),
                Text = existingQueue?.Name ?? ""
            };

            var lblImageCount = new Label { Text = "对比图片数:", Location = new Point(20, 100), AutoSize = true };
            var numImageCount = new NumericUpDown
            {
                Location = new Point(120, 97),
                Size = new Size(100, 23),
                Minimum = 2,
                Maximum = 10,
                Value = existingQueue?.ImageCount ?? 3
            };

            var btnOk = new Button
            {
                Text = "确定",
                Location = new Point(200, 150),
                Size = new Size(80, 30)
            };
            var btnCancel = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Location = new Point(290, 150),
                Size = new Size(80, 30)
            };

            bool success = false;
            btnOk.Click += async (s, e) =>
            {
                if (cmbProject.SelectedItem is Project selectedProject && !string.IsNullOrWhiteSpace(txtName.Text))
                {
                    try
                    {
                        if (existingQueue == null)
                        {
                            await _queueService.CreateAsync(new CreateQueueDto
                            {
                                ProjectId = selectedProject.Id,
                                Name = txtName.Text.Trim(),
                                ImageCount = (int)numImageCount.Value
                            });
                            MessageBox.Show("队列创建成功", "成功",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            await _queueService.UpdateAsync(existingQueue.Id, new UpdateQueueDto
                            {
                                Name = txtName.Text.Trim(),
                                ImageCount = (int)numImageCount.Value
                            });
                            MessageBox.Show("队列更新成功", "成功",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        success = true;
                        form.DialogResult = DialogResult.OK;
                        form.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"操作失败: {ex.Message}", "错误",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("请填写完整信息", "提示",
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

        private void BtnImport_Click(object? sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选择要导入图片的队列", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var queue = listView.SelectedItems[0].Tag as Models.Queue;
            if (queue == null) return;

            var importForm = new ImageImportForm(queue);
            importForm.ShowDialog();
            LoadQueuesAsync();
        }

        private async Task BtnDelete_ClickAsync()
        {
            if (listView.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选择要删除的队列", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var queue = listView.SelectedItems[0].Tag as Models.Queue;
            if (queue == null) return;

            var result = MessageBox.Show(
                $"确定要删除队列 '{queue.Name}' 吗？",
                "确认删除",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            try
            {
                await _queueService.DeleteAsync(queue.Id);
                MessageBox.Show("队列删除成功", "成功",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadQueuesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除队列失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
