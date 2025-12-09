using System;
using System.Drawing;
using System.Windows.Forms;

namespace ImageAnnotationApp.Controls
{
    /// 双缓冲PictureBox，优化图片绘制性能
    public class OptimizedPictureBox : PictureBox
    {
        public OptimizedPictureBox()
        {
            // 启用双缓冲以减少闪烁
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.UserPaint |
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (Image == null)
            {
                base.OnPaint(pe);
                return;
            }

            // 使用高质量渲染
            pe.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            pe.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            pe.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            base.OnPaint(pe);
        }
    }
}
