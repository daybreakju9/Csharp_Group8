using System.Drawing;

namespace ImageAnnotationApp.Helpers
{
    /// UI常量，定义应用程序的统一样式
    public static class UIConstants
    {
        // 应用程序信息 
        public const string AppName = "图片标注系统";

        // 颜色方案 
        public static class Colors
        {
            // 主要按钮颜色
            public static readonly Color PrimaryButton = Color.FromArgb(0, 122, 204);
            public static readonly Color SuccessButton = Color.FromArgb(40, 167, 69);
            public static readonly Color DangerButton = Color.FromArgb(220, 53, 69);
            public static readonly Color WarningButton = Color.FromArgb(255, 193, 7);

            // 文本颜色
            public static readonly Color TextPrimary = Color.Black;
            public static readonly Color TextSecondary = Color.DarkGray;
            public static readonly Color TextHint = Color.Gray;

            // 状态颜色
            public static readonly Color Selected = Color.LightBlue;
            public static readonly Color Hover = Color.LightGreen;
            public static readonly Color Error = Color.LightCoral;

            // 背景颜色
            public static readonly Color Background = SystemColors.Control;
            public static readonly Color Panel = Color.White;
        }

        // 导航与布局 
        public static class Navigation
        {
            // 侧边栏默认宽度（像素），可以在主窗体里作为最小/默认值使用
            public const int NavWidth = 220;
            // 较宽侧栏（大屏使用）
            public const int NavWidthLarge = 260;
        }

        // 按钮尺寸
        public static class ButtonSizes
        {
            public static readonly Size Small = new Size(80, 28);
            public static readonly Size Medium = new Size(110, 36);
            public static readonly Size Large = new Size(150, 44);
            public static readonly Size TouchFriendlyLarge = new Size(170, 48);
        }

        // 窗口尺寸
        public static class WindowSizes
        {
            public static readonly Size Small = new Size(600, 400);
            public static readonly Size Medium = new Size(900, 600);
            public static readonly Size Large = new Size(1200, 700);
            public static readonly Size ExtraLarge = new Size(1400, 800);
        }

        // 间距和填充
        public static class Spacing
        {
            public const int Small = 5;
            public const int Medium = 10;
            public const int Large = 15;
            public const int ExtraLarge = 20;
        }

        // 字体 
        public static class Fonts
        {
            public static readonly Font Title = new Font("Microsoft YaHei", 12F, FontStyle.Bold);
            public static readonly Font Subtitle = new Font("Microsoft YaHei", 10F, FontStyle.Bold);
            public static readonly Font Normal = new Font("Microsoft YaHei UI", 9F);
            public static readonly Font Small = new Font("Microsoft YaHei UI", 8F);
        }

        // 消息框标题 
        public static class MessageTitles
        {
            public const string Success = "成功";
            public const string Error = "错误";
            public const string Warning = "提示";
            public const string Confirm = "确认";
            public const string Information = "信息";
        }

        // 通用提示信息
        public static class Messages
        {
            public const string CreateSuccess = "创建成功";
            public const string UpdateSuccess = "更新成功";
            public const string DeleteSuccess = "删除成功";
            public const string SaveSuccess = "保存成功";
            public const string SubmitSuccess = "提交成功";
            public const string ExportSuccess = "导出完成";

            public const string LoadFailed = "加载失败";
            public const string CreateFailed = "创建失败";
            public const string UpdateFailed = "更新失败";
            public const string DeleteFailed = "删除失败";
            public const string SaveFailed = "保存失败";
            public const string SubmitFailed = "提交失败";
            public const string ExportFailed = "导出失败";

            public const string SelectItem = "请选择项目";
            public const string FillRequired = "请填写完整信息";
            public const string NoPermission = "您没有权限执行此操作";

            public const string ConfirmDelete = "确定要删除吗？";
        }

        // 状态栏消息
        public static class StatusMessages
        {
            public const string Loading = "正在加载...";
            public const string Saving = "正在保存...";
            public const string Submitting = "正在提交...";
            public const string Exporting = "正在导出...";
            public const string Processing = "正在处理...";
            public const string Ready = "就绪";
            public const string Completed = "完成";
        }

        // 窗口标题格式化
        public static string FormatWindowTitle(string formName, string? context = null)
        {
            if (string.IsNullOrEmpty(context))
                return $"{AppName} - {formName}";
            return $"{AppName} - {formName} - {context}";
        }

        // 创建标准按钮
        public static Button CreateButton(string text, Size size, Color backColor, EventHandler? clickHandler = null)
        {
            var button = new Button
            {
                Text = text,
                Size = size,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            if (clickHandler != null)
                button.Click += clickHandler;

            return button;
        }

        // 创建标准主按钮
        public static Button CreatePrimaryButton(string text, EventHandler? clickHandler = null)
        {
            return CreateButton(text, ButtonSizes.Medium, Colors.PrimaryButton, clickHandler);
        }

        // 创建标准成功按钮
        public static Button CreateSuccessButton(string text, EventHandler? clickHandler = null)
        {
            return CreateButton(text, ButtonSizes.Medium, Colors.SuccessButton, clickHandler);
        }

        // 创建标准危险按钮
        public static Button CreateDangerButton(string text, EventHandler? clickHandler = null)
        {
            return CreateButton(text, ButtonSizes.Medium, Colors.DangerButton, clickHandler);
        }
    }
}
