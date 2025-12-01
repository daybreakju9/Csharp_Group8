using System.Reflection;
using System.Windows.Forms;

namespace ImageAnnotationApp.Helpers
{
    /// <summary>
    /// Control扩展方法
    /// </summary>
    public static class ControlExtensions
    {
        /// <summary>
        /// 为Control启用或禁用双缓冲
        /// </summary>
        public static void DoubleBuffered(this Control control, bool enable)
        {
            var property = typeof(Control).GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);
            property?.SetValue(control, enable, null);
        }
    }
}
