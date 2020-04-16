using System.Windows;
using System.Windows.Controls;

namespace HandyScreenshot.Controls
{
    public class RectCanvas : Control
    {
        public static readonly DependencyProperty ClipRectProperty = DependencyProperty.Register(
            "ClipRect", typeof(Rect), typeof(RectCanvas), new PropertyMetadata(default(Rect)));

        public Rect ClipRect
        {
            get => (Rect) GetValue(ClipRectProperty);
            set => SetValue(ClipRectProperty, value);
        }

        static RectCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RectCanvas), new FrameworkPropertyMetadata(typeof(RectCanvas)));
        }
    }
}
