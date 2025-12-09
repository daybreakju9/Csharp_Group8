using System.Reflection;
using System.Windows.Forms;

namespace ImageAnnotationApp.Helpers
{
    /// Control扩展方法
    public static class ControlExtensions
    {
        /// 为Control启用或禁用双缓冲
        public static void DoubleBuffered(this Control control, bool enable)
        {
            var property = typeof(Control).GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);
            property?.SetValue(control, enable, null);
        }
    }
}
