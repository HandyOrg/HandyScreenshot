using System.Windows;
using System.Windows.Media;
using HandyScreenshot.Helpers;
using static HandyScreenshot.Controls.ClipBox;

namespace HandyScreenshot.Controls
{
    internal class ClipBoxPointVisual : DrawingControlBase
    {
        public static readonly DependencyProperty RectProxyProperty = DependencyProperty.Register(
            "RectProxy", typeof(RectProxy), typeof(ClipBoxPointVisual), new PropertyMetadata(default(RectProxy), OnRectProxyChanged));
        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
            "Scale", typeof(double), typeof(ClipBoxPointVisual), new PropertyMetadata(default(double), OnScaleChanged));

        private static void OnRectProxyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.UpdateDependencyProperty<ClipBoxPointVisual, RectProxy>(e,
                (self, newValue) => newValue.RectChanged += self.OnRectChanged,
                (self, oldValue) => oldValue.RectChanged -= self.OnRectChanged);
        }

        private static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.UpdateDependencyProperty<ClipBoxPointVisual, double>(e,
                (self, newValue) =>
                {
                    var correction = PrimaryPenThickness * newValue / 2;
                    self._positionCorrection = correction;
                    self._sizeCorrection = 3 * correction;
                });
        }

        public RectProxy RectProxy
        {
            get => (RectProxy)GetValue(RectProxyProperty);
            set => SetValue(RectProxyProperty, value);
        }

        private const double MinDisplayPointLimit = 80;
        private const double PointRadius = 4.5;

        private double _positionCorrection = PrimaryPenThickness / 2d;
        private double _sizeCorrection = 1.5 * PrimaryPenThickness;

        private void OnRectChanged(double x, double y, double w, double h)
        {
            Dispatcher.Invoke(RefreshClipBoxPoint);
        }

        private void RefreshClipBoxPoint() => GetDrawingVisual().Using(DrawClipBoxPoint);

        private void DrawClipBoxPoint(DrawingContext dc)
        {
            var x = RectProxy.X - _positionCorrection;
            var y = RectProxy.Y - _positionCorrection;
            var w = RectProxy.Width + _sizeCorrection;
            var h = RectProxy.Height + _sizeCorrection;
            var r = x + w;
            var b = y + h;

            if (w > MinDisplayPointLimit && h > MinDisplayPointLimit)
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

        private static void DrawPoint(DrawingContext dc, Point point)
        {
            dc.DrawEllipse(PrimaryBrush, WhitePen, point, PointRadius, PointRadius);
        }
    }
}
