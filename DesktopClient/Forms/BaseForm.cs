using System;
using System.Windows.Forms;
using ImageAnnotationApp.Helpers;

namespace ImageAnnotationApp.Forms
{
    /// 所有子窗体的基类，提供统一的导航功能
    public class BaseForm : Form
    {
        protected NavigationManager? Navigation { get; private set; }

        protected BaseForm()
        {
            this.Load += BaseForm_Load;
        }

        private void BaseForm_Load(object? sender, EventArgs e)
        {
            // 查找MainForm并获取NavigationManager
            var mainForm = FindMainForm();
            if (mainForm != null)
            {
                Navigation = mainForm.Navigation;
            }
        }

        /// 查找MainForm实例
        protected MainForm? FindMainForm()
        {
            // 方法1: 通过 Parent 向上查找
            Control? current = this.Parent;
            while (current != null)
            {
                if (current is MainForm mf)
                {
                    return mf;
                }
                current = current.Parent;
            }

            // 方法2: 如果方法1失败，从 Application.OpenForms 中查找
            foreach (Form form in Application.OpenForms)
            {
                if (form is MainForm mainForm)
                {
                    return mainForm;
                }
            }

            return null;
        }

        /// 导航到指定窗体
        protected void NavigateTo(Form form)
        {
            if (Navigation != null)
            {
                Navigation.NavigateTo(form);
            }
            else
            {
                MessageBox.Show("导航管理器未初始化", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// 返回上一个窗体
        protected void NavigateBack()
        {
            if (Navigation != null)
            {
                Navigation.NavigateBack();
            }
            else
            {
                MessageBox.Show("导航管理器未初始化", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// 更新状态栏
        protected void UpdateStatus(string message)
        {
            Navigation?.UpdateStatus(message);
        }
    }
}
