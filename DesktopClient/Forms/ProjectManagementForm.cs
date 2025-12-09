using ImageAnnotationApp.Services;
using ImageAnnotationApp.Models;
using ImageAnnotationApp.Helpers;
using System.Drawing;
using System.Windows.Forms;

namespace ImageAnnotationApp.Forms
{
    public partial class ProjectManagementForm : BaseForm
    {
        private readonly ProjectService _projectService;
        private ListView listView = null!;

        public ProjectManagementForm()
        {
            _projectService = new ProjectService();
            InitializeCustomComponents();
            _ = LoadProjectsAsync();
        }

        private void InitializeCustomComponents()
        {
            this.Text = UIConstants.FormatWindowTitle("项目管理");
            this.Size = UIConstants.WindowSizes.Medium;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = UIConstants.Fonts.Normal;
            this.BackColor = UIConstants.Colors.Background;

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
            listView.Columns.Add("项目名称", 200);
            listView.Columns.Add("描述", 300);
            listView.Columns.Add("队列数", 80);
            listView.Columns.Add("创建者", 120);
            listView.Columns.Add("创建时间", 150);

            // 顶部工具栏
            var topPanel = new Panel
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
                AutoScroll = false,
                BackColor = this.BackColor
            };

            // 按钮
            var btnAdd = UIConstants.CreatePrimaryButton("新建项目", null);
            btnAdd.Size = UIConstants.ButtonSizes.Medium;
            btnAdd.Font = UIConstants.Fonts.Normal;
            btnAdd.Margin = new Padding(0, 0, UIConstants.Spacing.Medium, 0);
            btnAdd.Click += BtnAdd_Click;

            var btnEdit = UIConstants.CreateButton("编辑", UIConstants.ButtonSizes.Medium, UIConstants.Colors.PrimaryButton, null);
            btnEdit.Font = UIConstants.Fonts.Normal;
            btnEdit.Margin = new Padding(0, 0, UIConstants.Spacing.Medium, 0);
            btnEdit.Click += BtnEdit_Click;

            var btnDelete = UIConstants.CreateDangerButton("删除", null);
            btnDelete.Size = UIConstants.ButtonSizes.Medium;
            btnDelete.Font = UIConstants.Fonts.Normal;
            btnDelete.Margin = new Padding(0, 0, UIConstants.Spacing.Medium, 0);
            btnDelete.Click += async (s, e) => await BtnDelete_ClickAsync();

            var btnRefresh = UIConstants.CreateButton("刷新", UIConstants.ButtonSizes.Medium, UIConstants.Colors.PrimaryButton, null);
            btnRefresh.Font = UIConstants.Fonts.Normal;
            btnRefresh.Margin = new Padding(0, 0, UIConstants.Spacing.Medium, 0);
            btnRefresh.Click += async (s, e) => await LoadProjectsAsync();

            // 应用幽灵样式
            ApplyGhostButtonStyle(btnAdd);
            ApplyGhostButtonStyle(btnEdit);
            ApplyGhostButtonStyle(btnDelete);
            ApplyGhostButtonStyle(btnRefresh);

            // 将按钮加入流式面板
            flow.Controls.Add(btnAdd);
            flow.Controls.Add(btnEdit);
            flow.Controls.Add(btnDelete);
            flow.Controls.Add(btnRefresh);

            topPanel.Controls.Add(flow);

            // 添加控件到窗体
            this.Controls.Add(listView);
            this.Controls.Add(topPanel);
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
            btn.MouseEnter += (s, e) =>
            {
                btn.BackColor = hover;
            };
            btn.MouseLeave += (s, e) =>
            {
                btn.BackColor = normalColor;
            };
        }

        private async Task LoadProjectsAsync()
        {
            try
            {
                UpdateStatus(UIConstants.StatusMessages.Loading);
                listView.Items.Clear();
                var projects = await _projectService.GetAllAsync();

                foreach (var project in projects)
                {
                    var item = new ListViewItem(project.Id.ToString());
                    item.SubItems.Add(project.Name);
                    item.SubItems.Add(project.Description ?? "");
                    item.SubItems.Add(project.QueueCount.ToString());
                    item.SubItems.Add(project.CreatedByUsername);
                    item.SubItems.Add(project.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
                    item.Tag = project;
                    listView.Items.Add(item);
                }
                UpdateStatus(UIConstants.StatusMessages.Ready);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载项目列表失败: {ex.Message}", UIConstants.MessageTitles.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus(UIConstants.Messages.LoadFailed);
            }
        }

        private async void BtnAdd_Click(object? sender, EventArgs e)
        {
            var name = PromptInput("项目名称:", "新建项目");
            if (string.IsNullOrEmpty(name)) return;

            var description = PromptInput("项目描述:", "新建项目");

            try
            {
                UpdateStatus(UIConstants.StatusMessages.Saving);

                var created = await _projectService.CreateAsync(new CreateProjectDto
                {
                    Name = name,
                    Description = description
                });

                if (created != null)
                {
                    MessageBox.Show(UIConstants.Messages.CreateSuccess, UIConstants.MessageTitles.Success,
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadProjectsAsync();
                }
                else
                {
                    MessageBox.Show(UIConstants.Messages.CreateFailed, UIConstants.MessageTitles.Error,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    UpdateStatus(UIConstants.Messages.CreateFailed);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{UIConstants.Messages.CreateFailed}: {ex.Message}", UIConstants.MessageTitles.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus(UIConstants.Messages.CreateFailed);
            }
        }

        private async void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0)
            {
                MessageBox.Show(UIConstants.Messages.SelectItem, UIConstants.MessageTitles.Warning,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var project = listView.SelectedItems[0].Tag as Project;
            if (project == null) return;

            var name = PromptInput("项目名称:", "编辑项目", project.Name);
            if (string.IsNullOrEmpty(name)) return;

            var description = PromptInput("项目描述:", "编辑项目", project.Description ?? "");

            try
            {
                UpdateStatus(UIConstants.StatusMessages.Saving);

                var updated = await _projectService.UpdateAsync(project.Id, new UpdateProjectDto
                {
                    Name = name,
                    Description = description
                });

                if (updated != null)
                {
                    MessageBox.Show(UIConstants.Messages.UpdateSuccess, UIConstants.MessageTitles.Success,
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadProjectsAsync();
                }
                else
                {
                    MessageBox.Show(UIConstants.Messages.UpdateFailed, UIConstants.MessageTitles.Error,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    UpdateStatus(UIConstants.Messages.UpdateFailed);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{UIConstants.Messages.UpdateFailed}: {ex.Message}", UIConstants.MessageTitles.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus(UIConstants.Messages.UpdateFailed);
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

            var project = listView.SelectedItems[0].Tag as Project;
            if (project == null) return;

            var result = MessageBox.Show(
                $"确定要删除项目 '{project.Name}' 吗？\n此操作将删除该项目及其所有队列和图片！",
                UIConstants.MessageTitles.Confirm,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            try
            {
                UpdateStatus(UIConstants.StatusMessages.Processing);
                await _projectService.DeleteAsync(project.Id);
                MessageBox.Show(UIConstants.Messages.DeleteSuccess, UIConstants.MessageTitles.Success,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadProjectsAsync();
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
                Size = new Size(448, 244),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var label = new Label { Text = prompt, Location = new Point(10, 20), AutoSize = true };
            var textBox = new TextBox { Location = new Point(10, 50), Size = new Size(400, 23), Text = defaultValue };
            var btnOk = new Button
            {
                Text = "确定",
                DialogResult = DialogResult.OK,
                Location = new Point(106, 120),
                Size = UIConstants.ButtonSizes.Medium
            };
            var btnCancel = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Location = new Point(216, 120),
                Size = UIConstants.ButtonSizes.Medium
            };

            form.Controls.AddRange(new Control[] { label, textBox, btnOk, btnCancel });
            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;

            return form.ShowDialog() == DialogResult.OK ? textBox.Text : null;
        }
    }
    internal static class ControlExtensions
    {
        public static void InvokeIfRequired(this Control ctl, Action action)
        {
            if (ctl == null) return;
            if (ctl.InvokeRequired)
                ctl.Invoke(action);
            else
                action();
        }
    }
}