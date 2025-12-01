using System;
using System.Windows.Forms;
using ImageAnnotationApp.Helpers;

namespace ImageAnnotationApp.Forms
{
    /// <summary>
    /// 所有子窗体的基类，提供统一的导航功能
    /// </summary>
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

        /// <summary>
        /// 查找MainForm实例
        /// </summary>
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

        /// <summary>
        /// 导航到指定窗体
        /// </summary>
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

        /// <summary>
        /// 返回上一个窗体
        /// </summary>
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

        /// <summary>
        /// 更新状态栏
        /// </summary>
        protected void UpdateStatus(string message)
        {
            Navigation?.UpdateStatus(message);
        }
    }
}
