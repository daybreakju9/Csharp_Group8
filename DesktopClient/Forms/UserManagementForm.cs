using System;
using System.Threading.Tasks;
using System.Windows.Forms;
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

            tabControl = new TabControl { Dock = DockStyle.Fill };

            // 待审核游客标签页
            var tabGuests = new TabPage("待审核游客");
            listViewGuests = CreateListView();
            var toolStripGuests = new ToolStrip();
            var btnApprove = new ToolStripButton("批准");
            var btnReject = new ToolStripButton("拒绝");
            var btnRefreshGuests = new ToolStripButton("刷新");

            btnApprove.Click += async (s, e) => await ApproveUserAsync();
            btnReject.Click += async (s, e) => await RejectUserAsync();
            btnRefreshGuests.Click += async (s, e) => await LoadGuestUsersAsync();

            toolStripGuests.Items.Add(btnApprove);
            toolStripGuests.Items.Add(btnReject);
            toolStripGuests.Items.Add(new ToolStripSeparator());
            toolStripGuests.Items.Add(btnRefreshGuests);

            tabGuests.Controls.Add(listViewGuests);
            tabGuests.Controls.Add(toolStripGuests);
            toolStripGuests.Dock = DockStyle.Top;

            // 所有用户标签页
            var tabAllUsers = new TabPage("所有用户");
            listViewAllUsers = CreateListView();
            var toolStripAll = new ToolStrip();
            var btnChangeRole = new ToolStripButton("修改权限");
            var btnDeleteUser = new ToolStripButton("删除用户");
            var btnRefreshAll = new ToolStripButton("刷新");
            
            btnChangeRole.Click += async (s, e) => await ChangeUserRoleAsync();
            btnDeleteUser.Click += async (s, e) => await DeleteUserFromAllAsync();
            btnRefreshAll.Click += async (s, e) => await LoadAllUsersAsync();
            
            toolStripAll.Items.Add(btnChangeRole);
            toolStripAll.Items.Add(new ToolStripSeparator());
            toolStripAll.Items.Add(btnDeleteUser);
            toolStripAll.Items.Add(new ToolStripSeparator());
            toolStripAll.Items.Add(btnRefreshAll);

            tabAllUsers.Controls.Add(listViewAllUsers);
            tabAllUsers.Controls.Add(toolStripAll);
            toolStripAll.Dock = DockStyle.Top;

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
                GridLines = true
            };
            listView.Columns.Add("ID", 50);
            listView.Columns.Add("用户名", 200);
            listView.Columns.Add("角色", 150);
            listView.Columns.Add("注册时间", 200);
            return listView;
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
