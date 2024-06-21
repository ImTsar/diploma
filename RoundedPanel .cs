using System.Drawing.Drawing2D;

namespace GW_1
{
    public class RoundedPanel : Panel
    {
        public int CornerRadius { get; set; } = 30;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(new Rectangle(0, 0, CornerRadius, CornerRadius), 180, 90);
                path.AddArc(new Rectangle(Width - CornerRadius, 0, CornerRadius, CornerRadius), -90, 90);
                path.AddArc(new Rectangle(Width - CornerRadius, Height - CornerRadius, CornerRadius, CornerRadius), 0, 90);
                path.AddArc(new Rectangle(0, Height - CornerRadius, CornerRadius, CornerRadius), 90, 90);
                path.CloseAllFigures();

                Region = new Region(path);
                using (SolidBrush brush = new SolidBrush(this.BackColor))
                {
                    e.Graphics.FillPath(brush, path);
                }
            }
        }
    }
}
