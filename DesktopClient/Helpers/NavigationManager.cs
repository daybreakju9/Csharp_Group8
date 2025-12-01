using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ImageAnnotationApp.Forms;

namespace ImageAnnotationApp.Helpers
{
    /// <summary>
    /// 窗体导航管理器，统一管理所有窗体的加载和导航
    /// </summary>
    public class NavigationManager
    {
        private readonly MainForm _mainForm;
        private readonly Panel _mainPanel;
        private readonly Stack<Form> _navigationStack;
        private readonly ToolStripStatusLabel _statusLabel;

        public NavigationManager(MainForm mainForm, Panel mainPanel, ToolStripStatusLabel statusLabel)
        {
            _mainForm = mainForm ?? throw new ArgumentNullException(nameof(mainForm));
            _mainPanel = mainPanel ?? throw new ArgumentNullException(nameof(mainPanel));
            _statusLabel = statusLabel ?? throw new ArgumentNullException(nameof(statusLabel));
            _navigationStack = new Stack<Form>();
        }

        /// <summary>
        /// 加载窗体到主面板，并将当前窗体压入导航栈
        /// </summary>
        public void NavigateTo(Form form, bool addToStack = true)
        {
            if (form == null)
            {
                throw new ArgumentNullException(nameof(form));
            }

            try
            {
                // 如果需要添加到栈，保存当前窗体
                if (addToStack && _mainPanel.Controls.Count > 0)
                {
                    var currentForm = _mainPanel.Controls[0] as Form;
                    if (currentForm != null)
                    {
                        _navigationStack.Push(currentForm);
                    }
                }

                // 清空并加载新窗体
                _mainPanel.Controls.Clear();
                form.TopLevel = false;
                form.FormBorderStyle = FormBorderStyle.None;
                form.Dock = DockStyle.Fill;
                _mainPanel.Controls.Add(form);
                form.Show();

                // 更新状态栏
                UpdateStatus($"已加载: {form.Text}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载窗体失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 返回到上一个窗体
        /// </summary>
        public void NavigateBack()
        {
            try
            {
                if (_navigationStack.Count > 0)
                {
                    var previousForm = _navigationStack.Pop();

                    // 清空当前窗体并加载上一个窗体
                    _mainPanel.Controls.Clear();
                    previousForm.TopLevel = false;
                    previousForm.FormBorderStyle = FormBorderStyle.None;
                    previousForm.Dock = DockStyle.Fill;
                    _mainPanel.Controls.Add(previousForm);
                    previousForm.Show();

                    // 更新状态栏
                    UpdateStatus($"返回到: {previousForm.Text}");
                }
                else
                {
                    MessageBox.Show("已经是第一个页面了", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"返回失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 清空导航栈并加载新窗体（用于主菜单导航）
        /// </summary>
        public void NavigateToRoot(Form form)
        {
            _navigationStack.Clear();
            NavigateTo(form, addToStack: false);
        }

        /// <summary>
        /// 更新状态栏
        /// </summary>
        public void UpdateStatus(string message)
        {
            if (_statusLabel != null && !_statusLabel.IsDisposed)
            {
                _statusLabel.Text = message;
            }
        }

        /// <summary>
        /// 获取当前导航栈深度
        /// </summary>
        public int GetNavigationDepth()
        {
            return _navigationStack.Count;
        }

        /// <summary>
        /// 获取MainForm实例
        /// </summary>
        public MainForm GetMainForm()
        {
            return _mainForm;
        }
    }
}
