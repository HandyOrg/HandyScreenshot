using System.Windows;
using System.Windows.Controls;

namespace HandyScreenshot.Controls
{
    [TemplatePart(Name = ClipRectName, Type = typeof(FrameworkElement))]
    public class RectCanvas : Control
    {
        private const string ClipRectName = "PART_ClipRect";

        public static readonly DependencyProperty RectOperationProperty = DependencyProperty.Register(
            "RectOperation", typeof(RectOperation), typeof(RectCanvas), new PropertyMetadata(null, (o, args) =>
            {
                if (o is RectCanvas canvas)
                {
                    canvas.AttachToRectOperation();
                }
            }));

        public RectOperation RectOperation
        {
            get => (RectOperation)GetValue(RectOperationProperty);
            set => SetValue(RectOperationProperty, value);
        }

        private FrameworkElement _clipRect;

        private void AttachToRectOperation()
        {
            RectOperation?.Attach((x, y, w, h) => Dispatcher.Invoke(() =>
            {
                Canvas.SetLeft(_clipRect, x);
                Canvas.SetTop(_clipRect, y);
                _clipRect.Width = w;
                _clipRect.Height = h;
            }));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _clipRect = GetTemplateChild(ClipRectName) as FrameworkElement;
        }

        static RectCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RectCanvas), new FrameworkPropertyMetadata(typeof(RectCanvas)));
        }
    }
}
