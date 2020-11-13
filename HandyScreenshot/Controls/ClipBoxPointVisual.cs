using System;
using System.Windows;
using System.Windows.Media;
using HandyScreenshot.Helpers;

namespace HandyScreenshot.Controls
{
    internal class ClipBoxPointVisual : DrawingControlBase
    {
        public static readonly DependencyProperty RectProxyProperty = DependencyProperty.Register(
            "RectProxy", typeof(RectProxy), typeof(ClipBoxPointVisual), new PropertyMetadata(default(RectProxy), OnRectProxyChanged));

        private static void OnRectProxyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.UpdateDependencyProperty<ClipBoxPointVisual, RectProxy>(e,
                (self, newValue) => newValue.RectChanged += self.OnRectChanged,
                (self, oldValue) => oldValue.RectChanged -= self.OnRectChanged);
        }

        public RectProxy RectProxy
        {
            get => (RectProxy)GetValue(RectProxyProperty);
            set => SetValue(RectProxyProperty, value);
        }

        private const double MinDisplayPointLimit = 80;
        private const double PointRadius = 4.5;

        private readonly Brush PrimaryBrush;
        private readonly Pen PrimaryPen;
        private readonly Pen WhitePen;

        public ClipBoxPointVisual()
        {
            PrimaryBrush = new SolidColorBrush(Color.FromRgb(0x20, 0x80, 0xf0));
            PrimaryBrush.Freeze();
            PrimaryPen = new Pen(PrimaryBrush, 1.6);
            PrimaryPen.Freeze();
            WhitePen = new Pen(Brushes.White, 1.5);
            WhitePen.Freeze();
        }

        private void OnRectChanged(double x, double y, double w, double h)
        {
            Dispatcher.Invoke(RefreshClipBoxPoint);
        }

        private void RefreshClipBoxPoint() => GetDrawingVisual().Using(DrawClipBoxPoint);

        private void DrawClipBoxPoint(DrawingContext dc)
        {
            var halfPenThickness = PrimaryPen.Thickness / 2;

            var x = RectProxy.X - halfPenThickness;
            var y = RectProxy.Y - halfPenThickness;
            var w = RectProxy.Width + PrimaryPen.Thickness + halfPenThickness;
            var h = RectProxy.Height + PrimaryPen.Thickness + halfPenThickness;

            var w0 = Math.Max(w, 0);
            var h0 = Math.Max(h, 0);

            var r = x + w;
            var b = y + h;

            if (w0 > MinDisplayPointLimit && h0 > MinDisplayPointLimit)
            {
                var halfR = x + w / 2;
                var halfB = y + h / 2;

                var leftTopPoint = new Point(x, y);
                var topPoint = new Point(halfR, y);
                var rightTopPoint = new Point(r, y);
                var rightPoint = new Point(r, halfB);
                var rightBottomPoint = new Point(r, b);
                var bottomPoint = new Point(halfR, b);
                var leftBottomPoint = new Point(x, b);
                var leftPoint = new Point(x, halfB);

                DrawPoint(dc, leftTopPoint);
                DrawPoint(dc, topPoint);
                DrawPoint(dc, rightTopPoint);
                DrawPoint(dc, rightPoint);
                DrawPoint(dc, rightBottomPoint);
                DrawPoint(dc, bottomPoint);
                DrawPoint(dc, leftBottomPoint);
                DrawPoint(dc, leftPoint);
            }
        }

        private void DrawPoint(DrawingContext dc, Point point)
        {
            dc.DrawEllipse(PrimaryBrush, WhitePen, point, PointRadius, PointRadius);
        }
    }
}
