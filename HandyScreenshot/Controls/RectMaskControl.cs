using System;
using System.Windows;
using System.Windows.Media;

namespace HandyScreenshot.Controls
{
    public class RectMaskControl : FrameworkElement
    {
        private static readonly Brush MaskBrush = new SolidColorBrush(Color.FromArgb(0x80, 0, 0, 0));
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
            RectOperation?.Attach(
                x => Dispatcher.Invoke(() =>
                {
                    _rightRect.X = Math.Max(x + RectOperation.Width, 0);
                    _rightRect.Width = Math.Max(ActualWidth - x - RectOperation.Width, 0);
                    _leftRect.Width = Math.Max(x, 0);
                    RefreshMask();
                }),
                y => Dispatcher.Invoke(() =>
                {
                    _topRect.Height = Math.Max(y, 0);
                    _rightRect.Y = Math.Max(y, 0);
                    _bottomRect.Y = Math.Max(y + RectOperation.Height, 0);
                    _bottomRect.Height = Math.Max(ActualHeight - y - RectOperation.Height, 0);
                    _leftRect.Y = Math.Max(y, 0);
                    RefreshMask();
                }),
                w => Dispatcher.Invoke(() =>
                {
                    _rightRect.X = Math.Max(RectOperation.X + w, 0);
                    _rightRect.Width = Math.Max(ActualWidth - RectOperation.X - w, 0);
                    RefreshMask();
                }),
                h => Dispatcher.Invoke(() =>
                {
                    _bottomRect.Y = Math.Max(RectOperation.Y + h, 0);
                    _rightRect.Height = Math.Max(h, 0);
                    _bottomRect.Height = Math.Max(ActualHeight - RectOperation.Y - h, 0);
                    _leftRect.Height = Math.Max(h, 0);
                    RefreshMask();
                }));
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _topRect.Width = ActualWidth;
            _bottomRect.Width = ActualWidth;
        }

        private void RefreshMask()
        {
            var dc = ((DrawingVisual)_children[0]).RenderOpen();
            foreach (var rect in new[] { _leftRect, _topRect, _rightRect, _bottomRect })
            {
                if (rect.Width * rect.Height > 0)
                {
                    dc.DrawRectangle(Background, null, rect);
                }
            }

            dc.Close();
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
