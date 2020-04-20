using System;
using System.Windows;
using System.Windows.Media;

namespace HandyScreenshot.Controls
{
    public class RectMaskControl : FrameworkElement
    {
        private static readonly Brush MaskBrush = new SolidColorBrush(Color.FromArgb(0xA0, 0, 0, 0));
        private static readonly Rect RectZero = new Rect(0, 0, 0, 0);

        public static readonly DependencyProperty RectOperationProperty = DependencyProperty.Register(
            "RectOperation", typeof(RectOperation), typeof(RectMaskControl), new PropertyMetadata(null, (o, args) =>
            {
                if (o is RectMaskControl rectMaskControl)
                {
                    rectMaskControl.Attach();
                }
            }));
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            "Background", typeof(Brush), typeof(RectMaskControl), new PropertyMetadata(MaskBrush));

        public RectOperation RectOperation
        {
            get => (RectOperation)GetValue(RectOperationProperty);
            set => SetValue(RectOperationProperty, value);
        }

        public Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        private readonly VisualCollection _children;
        private Rect _topRect = RectZero;
        private Rect _rightRect = RectZero;
        private Rect _bottomRect = RectZero;
        private Rect _leftRect = RectZero;

        public RectMaskControl()
        {
            _children = new VisualCollection(this) { new DrawingVisual() };
            SizeChanged += OnSizeChanged;
        }

        private void Attach()
        {
            RectOperation?.Attach((x, y, w, h) => Dispatcher.Invoke(() =>
            {
                _leftRect.Y = Math.Max(y, 0);
                _leftRect.Width = Math.Max(x, 0);
                _leftRect.Height = Math.Max(h, 0);

                _topRect.Height = Math.Max(y, 0);

                _rightRect.X = Math.Max(x + w, 0);
                _rightRect.Y = Math.Max(y, 0);
                _rightRect.Width = Math.Max(ActualWidth - x - w, 0);
                _rightRect.Height = Math.Max(h, 0);

                _bottomRect.Y = Math.Max(y + h, 0);
                _bottomRect.Height = Math.Max(ActualHeight - y - h, 0);

                DrawRectangles();
            }));
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _topRect.Width = ActualWidth;
            _bottomRect.Width = ActualWidth;
        }

        private void DrawRectangles()
        {
            var dc = ((DrawingVisual)_children[0]).RenderOpen();
            DrawRectangle(dc, _leftRect);
            DrawRectangle(dc, _topRect);
            DrawRectangle(dc, _rightRect);
            DrawRectangle(dc, _bottomRect);
            dc.Close();
        }

        private void DrawRectangle(DrawingContext dc, Rect rect)
        {
            if (rect.Width * rect.Height > 0)
            {
                dc.DrawRectangle(Background, null, rect);
            }
        }

        protected override int VisualChildrenCount => _children.Count;

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _children[index];
        }
    }
}
