using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using ImageAnnotationApp.Services;
using ImageAnnotationApp.Models;
using ImageAnnotationApp.Helpers;

namespace ImageAnnotationApp.Forms
{
    public partial class UserManagementForm : BaseForm
    {
        private readonly UserService _userService;
        private TabControl tabControl = null!;
        private ListView listViewGuests = null!;
        private ListView listViewAllUsers = null!;

        public UserManagementForm()
        {
            _userService = new UserService();
            InitializeCustomComponents();
            _ = LoadUsersAsync();
        }

        private void InitializeCustomComponents()
        {
            this.Text = UIConstants.FormatWindowTitle("用户管理");
            this.Size = UIConstants.WindowSizes.Large;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = UIConstants.Fonts.Normal;
            this.BackColor = UIConstants.Colors.Background;

            tabControl = new TabControl { Dock = DockStyle.Fill };

            // 待审核游客标签页
            var tabGuests = new TabPage("待审核游客");
            listViewGuests = CreateListView();

            var topPanelGuests = new Panel
            {
                Dock = DockStyle.Top,
                Height = UIConstants.ButtonSizes.Medium.Height + UIConstants.Spacing.Large * 2,
                Padding = new Padding(UIConstants.Spacing.Medium),
                BackColor = this.BackColor
            };
            var flowGuests = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = false,
                AutoScroll = false,
                BackColor = this.BackColor
            };

            var btnApprove = new Button
            {
                Text = "批准",
                Size = UIConstants.ButtonSizes.Medium,
                Font = UIConstants.Fonts.Normal,
                Margin = new Padding(0, 0, UIConstants.Spacing.Medium, 0)
            };
            btnApprove.Click += async (s, e) => await ApproveUserAsync();

            var btnReject = new Button
            {
                Text = "拒绝",
                Size = UIConstants.ButtonSizes.Medium,
                Font = UIConstants.Fonts.Normal,
                Margin = new Padding(0, 0, UIConstants.Spacing.Medium, 0)
            };
            btnReject.Click += async (s, e) => await RejectUserAsync();

            var btnRefreshGuests = new Button
            {
                Text = "刷新",
                Size = UIConstants.ButtonSizes.Medium,
                Font = UIConstants.Fonts.Normal,
                Margin = new Padding(0, 0, UIConstants.Spacing.Medium, 0)
            };
            btnRefreshGuests.Click += async (s, e) => await LoadGuestUsersAsync();

            ApplyGhostButtonStyle(btnApprove);
            ApplyGhostButtonStyle(btnReject);
            ApplyGhostButtonStyle(btnRefreshGuests);

            flowGuests.Controls.Add(btnApprove);
            flowGuests.Controls.Add(btnReject);
            flowGuests.Controls.Add(btnRefreshGuests);
            topPanelGuests.Controls.Add(flowGuests);

            tabGuests.Controls.Add(listViewGuests);
            tabGuests.Controls.Add(topPanelGuests);

            // 所有用户标签页
            var tabAllUsers = new TabPage("所有用户");
            listViewAllUsers = CreateListView();

            var topPanelAll = new Panel
            {
                Dock = DockStyle.Top,
                Height = UIConstants.ButtonSizes.Medium.Height + UIConstants.Spacing.Large * 2,
                Padding = new Padding(UIConstants.Spacing.Medium),
                BackColor = this.BackColor
            };
            var flowAll = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = false,
                AutoScroll = false,
                BackColor = this.BackColor
            };

            var btnChangeRole = new Button
            {
                Text = "修改权限",
                Size = UIConstants.ButtonSizes.Medium,
                Font = UIConstants.Fonts.Normal,
                Margin = new Padding(0, 0, UIConstants.Spacing.Medium, 0)
            };
            btnChangeRole.Click += async (s, e) => await ChangeUserRoleAsync();

            var btnDeleteUser = new Button
            {
                Text = "删除用户",
                Size = UIConstants.ButtonSizes.Medium,
                Font = UIConstants.Fonts.Normal,
                Margin = new Padding(0, 0, UIConstants.Spacing.Medium, 0)
            };
            btnDeleteUser.Click += async (s, e) => await DeleteUserFromAllAsync();

            var btnRefreshAll = new Button
            {
                Text = "刷新",
                Size = UIConstants.ButtonSizes.Medium,
                Font = UIConstants.Fonts.Normal,
                Margin = new Padding(0, 0, UIConstants.Spacing.Medium, 0)
            };
            btnRefreshAll.Click += async (s, e) => await LoadAllUsersAsync();

            ApplyGhostButtonStyle(btnChangeRole);
            ApplyGhostButtonStyle(btnDeleteUser);
            ApplyGhostButtonStyle(btnRefreshAll);

            flowAll.Controls.Add(btnChangeRole);
            flowAll.Controls.Add(btnDeleteUser);
            flowAll.Controls.Add(btnRefreshAll);
            topPanelAll.Controls.Add(flowAll);

            tabAllUsers.Controls.Add(listViewAllUsers);
            tabAllUsers.Controls.Add(topPanelAll);

            tabControl.TabPages.Add(tabGuests);
            tabControl.TabPages.Add(tabAllUsers);

            this.Controls.Add(tabControl);
        }

        private ListView CreateListView()
        {
            var listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = UIConstants.Fonts.Normal
            };
            listView.Columns.Add("ID", 50);
            listView.Columns.Add("用户名", 200);
            listView.Columns.Add("角色", 150);
            listView.Columns.Add("注册时间", 200);
            return listView;
        }

        // 幽灵按钮样式：默认与背景一致、显示细边框，悬停/按下时变为天蓝色
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

        private async Task LoadUsersAsync()
        {
            await LoadGuestUsersAsync();
            await LoadAllUsersAsync();
        }

        private async Task LoadGuestUsersAsync()
        {
            try
            {
                UpdateStatus(UIConstants.StatusMessages.Loading);
                listViewGuests.Items.Clear();
                var guests = await _userService.GetGuestUsersAsync();

                foreach (var user in guests)
                {
                    var item = new ListViewItem(user.Id.ToString());
                    item.SubItems.Add(user.Username);
                    item.SubItems.Add(GetRoleDisplayName(user.Role));
                    item.SubItems.Add(user.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                    item.Tag = user;
                    listViewGuests.Items.Add(item);
                }
                UpdateStatus(UIConstants.StatusMessages.Ready);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载待审核游客失败: {ex.Message}", UIConstants.MessageTitles.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus(UIConstants.Messages.LoadFailed);
            }
        }

        private async Task LoadAllUsersAsync()
        {
            try
            {
                UpdateStatus(UIConstants.StatusMessages.Loading);
                listViewAllUsers.Items.Clear();
                var users = await _userService.GetAllUsersAsync();

                foreach (var user in users)
                {
                    var item = new ListViewItem(user.Id.ToString());
                    item.SubItems.Add(user.Username);
                    item.SubItems.Add(GetRoleDisplayName(user.Role));
                    item.SubItems.Add(user.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                    item.Tag = user;
                    listViewAllUsers.Items.Add(item);
                }
                UpdateStatus(UIConstants.StatusMessages.Ready);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载用户列表失败: {ex.Message}", UIConstants.MessageTitles.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus(UIConstants.Messages.LoadFailed);
            }
        }

        private async Task ApproveUserAsync()
        {
            if (listViewGuests.SelectedItems.Count == 0)
            {
                MessageBox.Show(UIConstants.Messages.SelectItem, UIConstants.MessageTitles.Warning,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var user = listViewGuests.SelectedItems[0].Tag as UserDto;
            if (user == null) return;

            try
            {
                UpdateStatus(UIConstants.StatusMessages.Processing);
                await _userService.ApproveUserAsync(user.Id);
                MessageBox.Show($"用户 {user.Username} 已批准", UIConstants.MessageTitles.Success,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadUsersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"批准用户失败: {ex.Message}", UIConstants.MessageTitles.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("批准失败");
            }
        }

        private async Task RejectUserAsync()
        {
            if (listViewGuests.SelectedItems.Count == 0)
            {
                MessageBox.Show(UIConstants.Messages.SelectItem, UIConstants.MessageTitles.Warning,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var user = listViewGuests.SelectedItems[0].Tag as UserDto;
            if (user == null) return;

            var result = MessageBox.Show(
                $"确定要拒绝用户 '{user.Username}' 吗？",
                UIConstants.MessageTitles.Confirm,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            try
            {
                UpdateStatus(UIConstants.StatusMessages.Processing);
                await _userService.DeleteUserAsync(user.Id);
                MessageBox.Show($"用户 {user.Username} 已拒绝", UIConstants.MessageTitles.Success,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadUsersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"拒绝用户失败: {ex.Message}", UIConstants.MessageTitles.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("拒绝失败");
            }
        }

        private async Task ChangeUserRoleAsync()
        {
            if (listViewAllUsers.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选择要修改角色的用户", UIConstants.MessageTitles.Warning,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var user = listViewAllUsers.SelectedItems[0].Tag as UserDto;
            if (user == null) return;

            // 创建权限选择对话框
            using var dialog = new Form
            {
                Text = "修改用户权限",
                Size = new Size(350, 200),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lblUser = new Label
            {
                Text = $"用户: {user.Username}",
                Location = new Point(20, 20),
                AutoSize = true
            };

            var lblCurrentRole = new Label
            {
                Text = $"当前权限: {GetRoleDisplayName(user.Role)}",
                Location = new Point(20, 50),
                AutoSize = true
            };

            var lblNewRole = new Label
            {
                Text = "新权限:",
                Location = new Point(20, 80),
                AutoSize = true
            };

            var comboRole = new ComboBox
            {
                Location = new Point(80, 77),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            // 添加中文选项
            comboRole.Items.AddRange(new[] { "游客", "普通用户", "管理员" });
            // 设置当前选中的项（根据用户当前角色）
            comboRole.SelectedItem = GetRoleDisplayName(user.Role);

            var btnOk = new Button
            {
                Text = "确定",
                DialogResult = DialogResult.OK,
                Location = new Point(150, 120),
                Size = new Size(75, 30)
            };

            var btnCancel = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Location = new Point(235, 120),
                Size = new Size(75, 30)
            };

            dialog.Controls.AddRange(new Control[] { lblUser, lblCurrentRole, lblNewRole, comboRole, btnOk, btnCancel });
            dialog.AcceptButton = btnOk;
            dialog.CancelButton = btnCancel;

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                var selectedRoleDisplayName = comboRole.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(selectedRoleDisplayName))
                {
                    return;
                }

                // 将中文权限名称转换为英文角色值
                var newRole = GetRoleFromDisplayName(selectedRoleDisplayName);
                if (string.IsNullOrEmpty(newRole) || newRole == user.Role)
                {
                    return;
                }

                try
                {
                    UpdateStatus(UIConstants.StatusMessages.Processing);
                    var updatedUser = await _userService.UpdateUserRoleAsync(user.Id, newRole);
                    MessageBox.Show(
                        $"用户 {updatedUser.Username} 的权限已更新为 {GetRoleDisplayName(updatedUser.Role)}",
                        UIConstants.MessageTitles.Success,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    await LoadAllUsersAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"修改用户权限失败: {ex.Message}", UIConstants.MessageTitles.Error,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    UpdateStatus("修改权限失败");
                }
            }
        }

        private async Task DeleteUserFromAllAsync()
        {
            if (listViewAllUsers.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选择要删除的用户", UIConstants.MessageTitles.Warning,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var user = listViewAllUsers.SelectedItems[0].Tag as UserDto;
            if (user == null) return;

            var result = MessageBox.Show(
                $"确定要删除用户 '{user.Username}' 吗？",
                UIConstants.MessageTitles.Confirm,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            try
            {
                UpdateStatus(UIConstants.StatusMessages.Processing);
                await _userService.DeleteUserAsync(user.Id);
                MessageBox.Show($"用户 {user.Username} 已删除", UIConstants.MessageTitles.Success,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAllUsersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除用户失败: {ex.Message}", UIConstants.MessageTitles.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("删除失败");
            }
        }

        private string GetRoleDisplayName(string role)
        {
            return role switch
            {
                "Admin" => "管理员",
                "User" => "普通用户",
                "Guest" => "游客",
                _ => role
            };
        }

        private string GetRoleFromDisplayName(string displayName)
        {
            return displayName switch
            {
                "管理员" => "Admin",
                "普通用户" => "User",
                "游客" => "Guest",
                _ => displayName
            };
        }
    }
}
