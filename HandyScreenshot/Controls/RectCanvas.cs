using System.Windows;
using System.Windows.Controls;

namespace HandyScreenshot.Controls
{
    public class RectCanvas : Control
    {
        public static readonly DependencyProperty RectProperty = DependencyProperty.Register(
            "Rect", typeof(Rect), typeof(RectCanvas), new PropertyMetadata(default(Rect)));

        public Rect Rect
        {
            get => (Rect) GetValue(RectProperty);
            set => SetValue(RectProperty, value);
        }

        static RectCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RectCanvas), new FrameworkPropertyMetadata(typeof(RectCanvas)));
        }
    }
}
