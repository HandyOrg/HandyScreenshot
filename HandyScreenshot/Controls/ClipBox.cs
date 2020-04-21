using System;
using System.Windows;
using System.Windows.Media;

namespace HandyScreenshot.Controls
{
    public class ClipBox : FrameworkElement
    {
        private const double MinDisplayPointLimit = 80;
        private const double PointRadius = 4.5;

        private static readonly Rect RectZero = new Rect(0, 0, 0, 0);
        private static readonly Point PointZero = new Point(0, 0);
        private static readonly Brush MaskBrush;
        private static readonly Brush PrimaryBrush;
        private static readonly Pen PrimaryPen;
        private static readonly Pen WhitePen;

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
        }

        public static readonly DependencyProperty RectOperationProperty = DependencyProperty.Register(
            "RectOperation", typeof(RectOperation), typeof(ClipBox), new PropertyMetadata(null, RectOperationChanged));

        private static void RectOperationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ClipBox rectMaskControl)
            {
                rectMaskControl.Attach();
            }
        }


        public RectOperation RectOperation
        {
            get => (RectOperation)GetValue(RectOperationProperty);
            set => SetValue(RectOperationProperty, value);
        }

        private readonly DrawingVisual _drawingVisual;

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

        public ClipBox()
        {
            var children = new VisualCollection(this) { new DrawingVisual() };
            _drawingVisual = (DrawingVisual)children[0];
            SizeChanged += OnSizeChanged;
        }

        private void Attach()
        {
            RectOperation?.Attach((x, y, w, h) => Dispatcher.Invoke(() =>
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

                RefreshDrawingVisual();
            }));
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _topRect.Width = ActualWidth;
            _bottomRect.Width = ActualWidth;
        }

        private void RefreshDrawingVisual()
        {
            var dc = _drawingVisual.RenderOpen();

            DrawRectangle(dc, _leftRect, MaskBrush);
            DrawRectangle(dc, _topRect, MaskBrush);
            DrawRectangle(dc, _rightRect, MaskBrush);
            DrawRectangle(dc, _bottomRect, MaskBrush);
            DrawRectangle(dc, _centralRect, Brushes.Transparent, PrimaryPen);

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

            dc.Close();
        }

        private static void DrawRectangle(DrawingContext dc, Rect rect, Brush background, Pen pen = null)
        {
            if (rect.Width * rect.Height > 0)
            {
                dc.DrawRectangle(background, pen, rect);
            }
        }

        private static void DrawPoint(DrawingContext dc, Point point)
        {
            dc.DrawEllipse(PrimaryBrush, WhitePen, point, PointRadius, PointRadius);
        }

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException();

            return _drawingVisual;
        }
    }
}
