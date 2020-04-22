using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HandyScreenshot.Controls
{
    public class MagnifierToolTip : Control
    {
        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(
            "Target", typeof(Visual), typeof(MagnifierToolTip), new PropertyMetadata(default(Visual)));
        public static readonly DependencyProperty RegionProperty = DependencyProperty.Register(
            "Region", typeof(Rect), typeof(MagnifierToolTip), new PropertyMetadata(default(Rect)));

        public Visual Target
        {
            get => (Visual)GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }

        public Rect Region
        {
            get => (Rect)GetValue(RegionProperty);
            private set => SetValue(RegionProperty, value);
        }

        static MagnifierToolTip()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MagnifierToolTip), new FrameworkPropertyMetadata(typeof(MagnifierToolTip)));
        }
    }
}
