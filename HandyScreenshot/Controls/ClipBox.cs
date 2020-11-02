using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HandyScreenshot.Controls
{
    public class ClipBox : FrameworkElement
    {
        #region Static Members

        private const double MinDisplayPointLimit = 80;
        private const double PointRadius = 4.5;

        private static readonly Rect RectZero = new Rect(0, 0, 0, 0);
        private static readonly Point PointZero = new Point(0, 0);
        private static readonly Brush MaskBrush;
        private static readonly Brush PrimaryBrush;
        private static readonly Pen PrimaryPen;
        private static readonly Pen WhitePen;

        private static readonly Pen WhiteThinPen;
        private static readonly Pen BlackThinPen;
        private static readonly Pen CrossLinePen;

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
            WhiteThinPen = new Pen(Brushes.White, 0.8);
            WhiteThinPen.Freeze();
            BlackThinPen = new Pen(Brushes.Black, 0.8);
            BlackThinPen.Freeze();
            CrossLinePen = new Pen(new SolidColorBrush(Color.FromArgb(0x60, 0x20, 0x80, 0xf0)), 6.4);
            CrossLinePen.Freeze();
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

        #region Data for drawing magnifier

        private const double MagnifierWidth = 121.6;
        private const double MagnifierHeight = 83.2;
        private const double CrosshairSize = 6.4;
        private const double HalfCrosshairSize = CrosshairSize / 2;

        private Rect _magnifierRect = new Rect(PointZero, new Size(MagnifierWidth, MagnifierHeight));
        private Rect _magnifierBorderWhiteRect = new Rect(PointZero, new Size(MagnifierWidth + 0.8, MagnifierHeight + 0.8));
        private Rect _magnifierBorderBlackRect = new Rect(PointZero, new Size(MagnifierWidth + 1.6, MagnifierHeight + 1.6));
        private Rect _crosshairWhiteRect = new Rect(PointZero, new Size(CrosshairSize - 0.8, CrosshairSize - 0.8));
        private Rect _crosshairBlackRect = new Rect(PointZero, new Size(CrosshairSize, CrosshairSize));

        private Point _crossLineTopPoint1 = PointZero;
        private Point _crossLineTopPoint2 = PointZero;
        private Point _crossLineLeftPoint1 = PointZero;
        private Point _crossLineLeftPoint2 = PointZero;
        private Point _crossLineRightPoint1 = PointZero;
        private Point _crossLineRightPoint2 = PointZero;
        private Point _crossLineBottomPoint1 = PointZero;
        private Point _crossLineBottomPoint2 = PointZero;

        #endregion

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
            _magnifierRect.X = x + 1;
            _magnifierRect.Y = y + 1;

            _magnifierBorderWhiteRect.X = x;
            _magnifierBorderWhiteRect.Y = y;

            _magnifierBorderBlackRect.X = x - 1;
            _magnifierBorderBlackRect.Y = y - 1;

            _crosshairWhiteRect.X = x + (MagnifierWidth - CrosshairSize) / 2;
            _crosshairWhiteRect.Y = y + (MagnifierHeight - CrosshairSize) / 2;

            _crosshairBlackRect.X = _crosshairWhiteRect.X - 1;
            _crosshairBlackRect.Y = _crosshairWhiteRect.Y - 1;

            var centralX = x + MagnifierWidth / 2;
            var centralY = y + MagnifierHeight / 2;

            _crossLineTopPoint1.X = _crossLineBottomPoint1.X = _crossLineTopPoint2.X = _crossLineBottomPoint2.X = centralX;
            _crossLineLeftPoint1.X = x;
            _crossLineRightPoint1.X = x + MagnifierWidth;

            _crossLineLeftPoint1.Y = _crossLineRightPoint1.Y = _crossLineLeftPoint2.Y = _crossLineRightPoint2.Y = centralY;
            _crossLineTopPoint1.Y = y;
            _crossLineBottomPoint1.Y = y + MagnifierHeight;

            _crossLineLeftPoint2.X = centralX - HalfCrosshairSize;
            _crossLineRightPoint2.X = centralX + HalfCrosshairSize;

            _crossLineTopPoint2.Y = centralY - HalfCrosshairSize;
            _crossLineBottomPoint2.Y = centralY + HalfCrosshairSize;

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

        private bool inRect = false;

        private void DrawMagnifier(DrawingContext dc)
        {
            if (!RectProxy.Contains(MousePosition.X, MousePosition.Y))
            {
                if (inRect)
                {
                    dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, 5000, 5000));
                    inRect = false;
                }
                return;
            }

            inRect = true;


            var m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            var dpiFactor = 1 / m.M11;
            var scale = new ScaleTransform(dpiFactor, dpiFactor);


            var group = new DrawingGroup();
            RenderOptions.SetEdgeMode(group, EdgeMode.Aliased);
            var crossLineDC = group.Open();

            //crossLineDC.PushTransform(scale);

            crossLineDC.DrawRectangle(MagnifierBrush, null, _magnifierRect);
            crossLineDC.DrawRectangle(Brushes.Transparent, WhiteThinPen, _magnifierBorderWhiteRect);
            crossLineDC.DrawRectangle(Brushes.Transparent, BlackThinPen, _magnifierBorderBlackRect);
            //crossLineDC.DrawRectangle(Brushes.Transparent, WhiteThinPen, _crosshairWhiteRect);
            //crossLineDC.DrawRectangle(Brushes.Transparent, BlackThinPen, _crosshairBlackRect);
            crossLineDC.DrawLine(CrossLinePen, _crossLineTopPoint1, _crossLineTopPoint2);
            crossLineDC.DrawLine(CrossLinePen, _crossLineRightPoint1, _crossLineRightPoint2);
            crossLineDC.DrawLine(CrossLinePen, _crossLineBottomPoint1, _crossLineBottomPoint2);
            crossLineDC.DrawLine(CrossLinePen, _crossLineLeftPoint1, _crossLineLeftPoint2);

            //crossLineDC.Pop();
            crossLineDC.Close();

            dc.DrawDrawing(group);
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
