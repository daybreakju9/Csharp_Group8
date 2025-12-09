using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ImageAnnotationApp.Forms;

namespace ImageAnnotationApp.Helpers
{
   
    /// 窗体/控件导航管理器，统一管理所有页面的加载和导航
    public class NavigationManager
    {
        private readonly MainForm _mainForm;
        private readonly Panel _mainPanel;
        private readonly Stack<Control> _navigationStack;
        private readonly ToolStripStatusLabel _statusLabel;

        public NavigationManager(MainForm mainForm, Panel mainPanel, ToolStripStatusLabel statusLabel)
        {
            _mainForm = mainForm ?? throw new ArgumentNullException(nameof(mainForm));
            _mainPanel = mainPanel ?? throw new ArgumentNullException(nameof(mainPanel));
            _statusLabel = statusLabel ?? throw new ArgumentNullException(nameof(statusLabel));
            _navigationStack = new Stack<Control>();
        }

        /// 加载 Form
        public void NavigateTo(Form form, bool addToStack = true)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            var wrapped = WrapFormAsControl(form);
            NavigateToControl(wrapped, addToStack);
        }

        /// 加载任意 Control
        public void NavigateToControl(Control control, bool addToStack = true)
        {
            if (control == null) throw new ArgumentNullException(nameof(control));

            try
            {
                if (addToStack && _mainPanel.Controls.Count > 0)
                {
                    var current = _mainPanel.Controls[0];
                    if (current != null)
                    {
                        _navigationStack.Push(current);
                    }
                }

                _mainPanel.Controls.Clear();
                control.Dock = DockStyle.Fill;
                _mainPanel.Controls.Add(control);
                control.BringToFront();

                UpdateStatus($"已加载: {GetControlTitle(control)}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载页面失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// 返回到上一个页面  
        public void NavigateBack()
        {
            try
            {
                if (_navigationStack.Count > 0)
                {
                    var previous = _navigationStack.Pop();

                    // 清理当前控件
                    _mainPanel.Controls.Clear();

                    previous.Dock = DockStyle.Fill;
                    _mainPanel.Controls.Add(previous);
                    previous.BringToFront();

                    UpdateStatus($"返回到: {GetControlTitle(previous)}");
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

        /// 清空导航栈并加载新页面（用于主菜单导航）
        public void NavigateToRoot(Control control)
        {
            _navigationStack.Clear();
            NavigateToControl(control, addToStack: false);
        }

        public void NavigateToRoot(Form form)
        {
            _navigationStack.Clear();
            NavigateTo(form, addToStack: false);
        }

        /// 更新状态栏
        public void UpdateStatus(string message)
        {
            if (_statusLabel != null && !_statusLabel.IsDisposed)
            {
                _statusLabel.Text = message;
            }
        }

        public int GetNavigationDepth() => _navigationStack.Count;

        public MainForm GetMainForm() => _mainForm;

        // 辅助

        private Control WrapFormAsControl(Form form)
        {
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            var host = new Panel { Dock = DockStyle.Fill };
            host.Controls.Add(form);
            form.Show();
            return host;
        }

        private string GetControlTitle(Control ctrl)
        {
            if (ctrl is Panel p && p.Controls.Count > 0 && p.Controls[0] is Form f)
                return f.Text;
            if (ctrl is Form form) return form.Text;
            return ctrl.Name ?? ctrl.GetType().Name;
        }
    }
}
