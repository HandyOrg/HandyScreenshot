using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HandyScreenshot.Controls
{
    public class ClipBox : FrameworkElement
    {
        private class MognifierDrawingData
        {
            public readonly Pen WhiteThinPen;
            public readonly Pen BlackThinPen;
            public readonly Pen CrossLinePen;

            private const double MagnifierWidth = 152;
            private const double MagnifierHeight = 104;
            private const double CrosshairSize = 8;
            private const double HalfCrosshairSize = CrosshairSize / 2;

            public Rect MagnifierRect;
            public Rect MagnifierBorderWhiteRect;
            public Rect MagnifierBorderBlackRect;
            public Rect CrosshairWhiteRect;
            public Rect CrosshairBlackRect;
            public Point CrossLineTopPoint1 = PointZero;
            public Point CrossLineTopPoint2 = PointZero;
            public Point CrossLineLeftPoint1 = PointZero;
            public Point CrossLineLeftPoint2 = PointZero;
            public Point CrossLineRightPoint1 = PointZero;
            public Point CrossLineRightPoint2 = PointZero;
            public Point CrossLineBottomPoint1 = PointZero;
            public Point CrossLineBottomPoint2 = PointZero;

            private readonly double _scale;

            public MognifierDrawingData(double scale)
            {
                _scale = scale;
                WhiteThinPen = new Pen(Brushes.White, scale);
                WhiteThinPen.Freeze();
                BlackThinPen = new Pen(Brushes.Black, scale);
                BlackThinPen.Freeze();
                CrossLinePen = new Pen(new SolidColorBrush(Color.FromArgb(0x60, 0x20, 0x80, 0xf0)), CrosshairSize * scale);
                CrossLinePen.Freeze();

                MagnifierRect = new Rect(PointZero, new Size(MagnifierWidth * scale, MagnifierHeight * scale));
                MagnifierBorderWhiteRect = new Rect(PointZero, new Size((MagnifierWidth + 1) * scale, (MagnifierHeight + 1) * scale));
                MagnifierBorderBlackRect = new Rect(PointZero, new Size((MagnifierWidth + 2) * scale, (MagnifierHeight + 2) * scale));
                CrosshairWhiteRect = new Rect(PointZero, new Size((CrosshairSize - 1) * scale, (CrosshairSize - 1) * scale));
                CrosshairBlackRect = new Rect(PointZero, new Size(CrosshairSize * scale, CrosshairSize * scale));
            }

            public void OnPointChanged(double x, double y)
            {
                MagnifierRect.X = x;
                MagnifierRect.Y = y;

                MagnifierBorderWhiteRect.X = x;
                MagnifierBorderWhiteRect.Y = y;

                MagnifierBorderBlackRect.X = x - 1;
                MagnifierBorderBlackRect.Y = y - 1;

                CrosshairWhiteRect.X = x + (MagnifierWidth - CrosshairSize) / 2 * _scale;
                CrosshairWhiteRect.Y = y + (MagnifierHeight - CrosshairSize) / 2 * _scale;

                CrosshairBlackRect.X = (CrosshairWhiteRect.X - 1) * _scale;
                CrosshairBlackRect.Y = (CrosshairWhiteRect.Y - 1) * _scale;

                var centralX = x + MagnifierWidth / 2 * _scale;
                var centralY = y + MagnifierHeight / 2 * _scale;

                CrossLineTopPoint1.X = CrossLineBottomPoint1.X = CrossLineTopPoint2.X = CrossLineBottomPoint2.X = centralX;
                CrossLineLeftPoint1.X = x;
                CrossLineRightPoint1.X = x + MagnifierWidth * _scale;

                CrossLineLeftPoint1.Y = CrossLineRightPoint1.Y = CrossLineLeftPoint2.Y = CrossLineRightPoint2.Y = centralY;
                CrossLineTopPoint1.Y = y;
                CrossLineBottomPoint1.Y = y + MagnifierHeight * _scale;

                CrossLineLeftPoint2.X = centralX - HalfCrosshairSize * _scale;
                CrossLineRightPoint2.X = centralX + HalfCrosshairSize * _scale;

                CrossLineTopPoint2.Y = centralY - HalfCrosshairSize * _scale;
                CrossLineBottomPoint2.Y = centralY + HalfCrosshairSize * _scale;
            }
        }

        #region Static Members

        private const double MinDisplayPointLimit = 80;
        private const double PointRadius = 4.5;

        private static readonly Rect RectZero = new Rect(0, 0, 0, 0);
        private static readonly Point PointZero = new Point(0, 0);
        private static readonly Brush MaskBrush;
        private static readonly Brush PrimaryBrush;
        private static readonly Brush CrossLineBrush;
        private static readonly Pen PrimaryPen;
        private static readonly Pen WhitePen;

        private static readonly Pen WhiteThinPen;
        private static readonly Pen BlackThinPen;
        private static readonly Pen CrossLinePen;
        private static readonly Brush InfoBackgroundBrush;

        static ClipBox()
        {
            MaskBrush = new SolidColorBrush(Color.FromArgb(0xA0, 0, 0, 0));
            MaskBrush.Freeze();
            PrimaryBrush = new SolidColorBrush(Color.FromRgb(0x20, 0x80, 0xf0));
            PrimaryBrush.Freeze();
            PrimaryPen = new Pen(PrimaryBrush, 2);
            PrimaryPen.Freeze();
            WhitePen = new Pen(Brushes.White, 1.5);
            WhitePen.Freeze();

            WhiteThinPen = new Pen(Brushes.White, 1);
            WhiteThinPen.Freeze();
            BlackThinPen = new Pen(Brushes.Black, 1);
            BlackThinPen.Freeze();
            CrossLineBrush = new SolidColorBrush(Color.FromArgb(0x60, 0x20, 0x80, 0xf0));
            CrossLineBrush.Freeze();
            CrossLinePen = new Pen(CrossLineBrush, 8);
            CrossLinePen.Freeze();
            InfoBackgroundBrush = new SolidColorBrush(Color.FromArgb(0xE0, 0, 0, 0));
            InfoBackgroundBrush.Freeze();
        }

        public static readonly DependencyProperty RectProxyProperty = DependencyProperty.Register(
            "RectProxy", typeof(RectProxy), typeof(ClipBox), new PropertyMetadata(null, RectProxyChanged));
        public static readonly DependencyProperty MousePositionProperty = DependencyProperty.Register(
            "MousePosition", typeof(PointProxy), typeof(ClipBox), new PropertyMetadata(null, MousePositionChanged));
        public static readonly DependencyProperty BackgroundImageProperty = DependencyProperty.Register(
            "BackgroundImage", typeof(BitmapSource), typeof(ClipBox), new PropertyMetadata(null));

        private static void RectProxyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UpdateSubscribe<ClipBox, RectProxy>(d, e,
                (magnifier, oldValue) => oldValue.RectChanged -= magnifier.OnRectChanged,
                (magnifier, newValue) => newValue.RectChanged += magnifier.OnRectChanged);
        }

        private static void MousePositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UpdateSubscribe<ClipBox, PointProxy>(d, e,
                (magnifier, oldValue) => oldValue.PointChanged -= magnifier.OnPointChanged,
                (magnifier, newValue) => newValue.PointChanged += magnifier.OnPointChanged);
        }

        private static void UpdateSubscribe<T, U>(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e,
            Action<T, U> unsubscribe,
            Action<T, U> subscribe)
        {
            if (d is T t)
            {
                if (e.OldValue is U oldValue)
                {
                    unsubscribe?.Invoke(t, oldValue);
                }

                if (e.NewValue is U newValue)
                {
                    subscribe?.Invoke(t, newValue);
                }
            }
        }

        #endregion

        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Scale.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(double), typeof(ClipBox), new PropertyMetadata(0D, (d, e) =>
            {
                if (d is ClipBox clipBox)
                {
                    clipBox._data = new MognifierDrawingData(clipBox.Scale);
                }
            }));

        public RectProxy RectProxy
        {
            get => (RectProxy)GetValue(RectProxyProperty);
            set => SetValue(RectProxyProperty, value);
        }

        public PointProxy MousePosition
        {
            get { return (PointProxy)GetValue(MousePositionProperty); }
            set { SetValue(MousePositionProperty, value); }
        }

        public BitmapSource BackgroundImage
        {
            get { return (BitmapSource)GetValue(BackgroundImageProperty); }
            set { SetValue(BackgroundImageProperty, value); }
        }



        public VisualBrush MagnifierBrush
        {
            get { return (VisualBrush)GetValue(MagnifierBrushProperty); }
            set { SetValue(MagnifierBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MagnifierBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MagnifierBrushProperty =
            DependencyProperty.Register("MagnifierBrush", typeof(VisualBrush), typeof(ClipBox), new PropertyMetadata(null));



        private readonly VisualCollection _visualCollection;

        #region Data for drawing clip box

        private Rect _topRect = RectZero;
        private Rect _rightRect = RectZero;
        private Rect _bottomRect = RectZero;
        private Rect _leftRect = RectZero;
        private Rect _centralRect = RectZero;

        private Point _leftTopPoint = PointZero;
        private Point _topPoint = PointZero;
        private Point _rightTopPoint = PointZero;
        private Point _rightPoint = PointZero;
        private Point _rightBottomPoint = PointZero;
        private Point _bottomPoint = PointZero;
        private Point _leftBottomPoint = PointZero;
        private Point _leftPoint = PointZero;

        #endregion

        private MognifierDrawingData? _data;

        public ClipBox()
        {
            _visualCollection = new VisualCollection(this)
            {
                // Index 0: ClipBox
                new DrawingVisual(),
                // Index 1: Magnifier
                new DrawingVisual()
            };
            SizeChanged += OnSizeChanged;
        }

        protected override int VisualChildrenCount => _visualCollection.Count;

        protected override Visual GetVisualChild(int index)
        {
            if (index >= _visualCollection.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _visualCollection[index];
        }

        private void OnRectChanged(double x, double y, double w, double h)
        {
            var h0 = Math.Max(h, 0);
            var r = x + w;
            var b = y + h;

            _leftRect.Y = y;
            _leftRect.Width = Math.Max(x, 0);
            _leftRect.Height = h0;

            _topRect.Height = Math.Max(y, 0);

            _rightRect.X = r;
            _rightRect.Y = y;
            _rightRect.Width = Math.Max(ActualWidth - r, 0);
            _rightRect.Height = h0;

            _bottomRect.Y = b;
            _bottomRect.Height = Math.Max(ActualHeight - b, 0);

            _centralRect.X = x;
            _centralRect.Y = y;
            _centralRect.Width = Math.Max(w, 0);
            _centralRect.Height = h0;

            if (_centralRect.Width > MinDisplayPointLimit && _centralRect.Height > MinDisplayPointLimit)
            {
                var halfR = x + w / 2D;
                var halfB = y + h / 2D;

                _leftTopPoint.X = x;
                _leftTopPoint.Y = y;

                _topPoint.X = halfR;
                _topPoint.Y = y;

                _rightTopPoint.X = r;
                _rightTopPoint.Y = y;

                _rightPoint.X = r;
                _rightPoint.Y = halfB;

                _rightBottomPoint.X = r;
                _rightBottomPoint.Y = b;

                _bottomPoint.X = halfR;
                _bottomPoint.Y = b;

                _leftBottomPoint.X = x;
                _leftBottomPoint.Y = b;

                _leftPoint.X = x;
                _leftPoint.Y = halfB;
            }

            Dispatcher.Invoke(RefreshClipBox);
        }

        private void OnPointChanged(double x, double y)
        {
            if (_data == null) return;

            _data.OnPointChanged(x, y);

            Dispatcher.Invoke(RefreshMagnifier);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _topRect.Width = ActualWidth;
            _bottomRect.Width = ActualWidth;
        }

        private void RefreshClipBox() => UsingDrawingContext(_visualCollection[0], DrawClipBox);

        private void RefreshMagnifier() => UsingDrawingContext(_visualCollection[1], DrawMagnifier);

        private void DrawClipBox(DrawingContext dc)
        {
            dc.DrawRectangle(MaskBrush, null, _leftRect);
            dc.DrawRectangle(MaskBrush, null, _topRect);
            dc.DrawRectangle(MaskBrush, null, _rightRect);
            dc.DrawRectangle(MaskBrush, null, _bottomRect);
            dc.DrawRectangle(Brushes.Transparent, PrimaryPen, _centralRect);

            if (_centralRect.Width > MinDisplayPointLimit && _centralRect.Height > MinDisplayPointLimit)
            {
                DrawPoint(dc, _leftTopPoint);
                DrawPoint(dc, _topPoint);
                DrawPoint(dc, _rightTopPoint);
                DrawPoint(dc, _rightPoint);
                DrawPoint(dc, _rightBottomPoint);
                DrawPoint(dc, _bottomPoint);
                DrawPoint(dc, _leftBottomPoint);
                DrawPoint(dc, _leftPoint);
            }
        }

        private void DrawMagnifier(DrawingContext dc)
        {
            if (!RectProxy.Contains(MousePosition.X, MousePosition.Y))
            {
                return;
            }

            var rect = _data!.MagnifierRect;
            Debug.WriteLine(rect);
            DrawMagnifier(dc, rect.X, rect.Y, rect.Width, rect.Height);
        }

        private void DrawMagnifier(DrawingContext dc, double x, double y, double width, double height)
        {
            var magnifierRect = new Rect(x, y, width, height);
            var innerRect = new Rect(x - Scale, y - Scale, width + 2 * Scale, height + 2 * Scale);
            var outlineRect = new Rect(x - 2 * Scale, y - 2 * Scale, width + 4 * Scale, height + 4 * Scale);

            var guidelines = new GuidelineSet();
            var halfPixel = Scale / 2;

            guidelines.GuidelinesX.Add(outlineRect.Left);
            guidelines.GuidelinesX.Add(outlineRect.Right);
            guidelines.GuidelinesY.Add(outlineRect.Top);
            guidelines.GuidelinesY.Add(outlineRect.Bottom);

            var halfPixelMagnified = _data.CrossLinePen.Thickness / 2;
            var centerLineX = (innerRect.Left + innerRect.Right) / 2;
            var centerLineY = (innerRect.Top + innerRect.Bottom) / 2;
            guidelines.GuidelinesX.Add(centerLineX + halfPixelMagnified);
            guidelines.GuidelinesX.Add(centerLineX - halfPixelMagnified);
            guidelines.GuidelinesY.Add(centerLineY + halfPixelMagnified);
            guidelines.GuidelinesY.Add(centerLineY - halfPixelMagnified);

            var centerInnerRect = new Rect(
                centerLineX - halfPixelMagnified + halfPixel - Scale,
                centerLineY - halfPixelMagnified + halfPixel - Scale,
                2 * (halfPixelMagnified - halfPixel) + 2 * Scale,
                2 * (halfPixelMagnified - halfPixel) + 2 * Scale);
            var centerOutlineRect = new Rect(
                centerLineX - halfPixelMagnified + halfPixel - 2 * Scale,
                centerLineY - halfPixelMagnified + halfPixel - 2 * Scale,
                2 * (halfPixelMagnified - halfPixel) + 4 * Scale,
                2 * (halfPixelMagnified - halfPixel) + 4 * Scale);

            guidelines.GuidelinesX.Add(centerInnerRect.Left - halfPixel);
            guidelines.GuidelinesX.Add(centerInnerRect.Right + halfPixel);
            guidelines.GuidelinesY.Add(centerInnerRect.Top - halfPixel);
            guidelines.GuidelinesY.Add(centerInnerRect.Bottom + halfPixel);

            var infoBackgroundRect = new Rect(outlineRect.Left, outlineRect.Bottom, outlineRect.Width, 80 * Scale);

            dc.PushGuidelineSet(guidelines);

            dc.DrawRectangle(Brushes.Black, null, outlineRect);
            dc.DrawRectangle(Brushes.White, null, innerRect);

            if (MagnifierBrush != null)
            {
                dc.DrawRectangle(MagnifierBrush, null, magnifierRect);
            }

            dc.DrawLine(
                _data.CrossLinePen,
                new Point(centerLineX, innerRect.Top),
                new Point(centerLineX, centerLineY - halfPixelMagnified - 2 * Scale));
            dc.DrawLine(
                _data.CrossLinePen,
                new Point(centerLineX, innerRect.Bottom),
                new Point(centerLineX, centerLineY + halfPixelMagnified + 2 * Scale));
            dc.DrawLine(
                _data.CrossLinePen,
                new Point(innerRect.Left, centerLineY),
                new Point(centerLineX - halfPixelMagnified - 2 * Scale, centerLineY));
            dc.DrawLine(
                _data.CrossLinePen,
                new Point(innerRect.Right, centerLineY),
                new Point(centerLineX + halfPixelMagnified + 2 * Scale, centerLineY));

            dc.DrawRectangle(Brushes.Transparent, _data.BlackThinPen, centerOutlineRect);
            dc.DrawRectangle(Brushes.Transparent, _data.WhiteThinPen, centerInnerRect);

            dc.DrawRectangle(InfoBackgroundBrush, null, infoBackgroundRect);
            var formattedText = new FormattedText("#ffffff", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Microsoft YaHei"), 12, Brushes.White, 1);
            dc.DrawText(
                formattedText,
                new Point(centerLineX - formattedText.Width / 2, outlineRect.Bottom + 32));

            var formattedText2 = new FormattedText($"({x / Scale:0}, {y / Scale:0})", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Microsoft YaHei"), 12, Brushes.White, 1);
            dc.DrawText(
                formattedText2,
                new Point(centerLineX - formattedText2.Width / 2, outlineRect.Bottom + 16));

            dc.Pop();
        }

        private static void UsingDrawingContext(Visual visual, Action<DrawingContext> callback)
        {
            if (visual is DrawingVisual dv)
            {
                var dc = dv.RenderOpen();
                callback?.Invoke(dc);
                dc.Close();
            }
        }

        private static void DrawPoint(DrawingContext dc, Point point)
        {
            dc.DrawEllipse(PrimaryBrush, WhitePen, point, PointRadius, PointRadius);
        }
    }
}
