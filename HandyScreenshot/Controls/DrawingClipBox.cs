using System;
using System.Windows;
using System.Windows.Media;
using HandyScreenshot.Helpers;
using static HandyScreenshot.Controls.ClipBox;

namespace HandyScreenshot.Controls
{
    internal class DrawingClipBox : IDrawable<(int x, int y, int w, int h, double actualWidth, double actualHeight)>
    {
        public MonitorInfo MonitorInfo { get; set; }

        private Pen _primaryPen;

        public DrawingClipBox(MonitorInfo monitorInfo)
        {
            MonitorInfo = monitorInfo;

            _primaryPen = CreatePrimaryPen(1);
        }

        public void UpdateScale(double scale)
        {
            _primaryPen = CreatePrimaryPen(scale);
        }

        public bool CanDraw((int x, int y, int w, int h, double actualWidth, double actualHeight) drawingData) => true;

        public void Draw(DrawingContext dc, (int x, int y, int w, int h, double actualWidth, double actualHeight) drawingData)
        {
            var (physicalX, physicalY, physicalWidth, physicalHeight, actualWidth, actualHeight) = drawingData;
            var (x, y, w, h) = MonitorInfo.ToWpfAxis(physicalX, physicalY, physicalWidth, physicalHeight);

            var halfPenThickness = _primaryPen.Thickness / 2;

            x -= halfPenThickness;
            y -= halfPenThickness;
            w += _primaryPen.Thickness + halfPenThickness;
            h += _primaryPen.Thickness + halfPenThickness;

            var x0 = Math.Max(x, 0);
            var y0 = Math.Max(y, 0);
            var w0 = Math.Max(w, 0);
            var h0 = Math.Max(h, 0);

            var r = x + w;
            var b = y + h;

            var leftRect = new Rect(0, y, x0, h0);
            var topRect = new Rect(0, 0, actualWidth, y0);
            var rightRect = new Rect(r, y, Math.Max(actualWidth - r, 0), h0);
            var bottomRect = new Rect(0, b, actualWidth, Math.Max(actualHeight - b, 0));
            var centralRect = new Rect(x, y, w0, h0);

            var guidelines = new GuidelineSet(
                new[]
                {
                    centralRect.Left + halfPenThickness,
                    centralRect.Right - halfPenThickness
                },
                new[]
                {
                    centralRect.Top + halfPenThickness,
                    centralRect.Bottom - halfPenThickness
                });

            dc.PushGuidelineSet(guidelines);

            dc.DrawRectangle(MaskBrush, null, leftRect);
            dc.DrawRectangle(MaskBrush, null, topRect);
            dc.DrawRectangle(MaskBrush, null, rightRect);
            dc.DrawRectangle(MaskBrush, null, bottomRect);
            dc.DrawRectangle(Brushes.Transparent, _primaryPen, centralRect);

            dc.Pop();
        }
    }
}
