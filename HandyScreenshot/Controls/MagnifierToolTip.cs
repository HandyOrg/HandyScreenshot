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
        public static readonly DependencyProperty PointProxyProperty = DependencyProperty.Register(
            "PointProxy", typeof(PointProxy), typeof(MagnifierToolTip), new PropertyMetadata(null, (o, args) =>
            {
                if (o is MagnifierToolTip self)
                {
                    self.Attach();
                }
            }));

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

        public PointProxy PointProxy
        {
            get => (PointProxy)GetValue(PointProxyProperty);
            set => SetValue(PointProxyProperty, value);
        }

        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(
            "Position", typeof(Point), typeof(MagnifierToolTip), new PropertyMetadata(default(Point)));

        public Point Position
        {
            get { return (Point)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        public static readonly DependencyProperty MousePointProperty = DependencyProperty.Register(
            "MousePoint", typeof(Point), typeof(MagnifierToolTip), new PropertyMetadata(default(Point)));

        public Point MousePoint
        {
            get { return (Point)GetValue(MousePointProperty); }
            set { SetValue(MousePointProperty, value); }
        }

        private void Attach()
        {
            if (PointProxy != null)
            {
                PointProxy.PointChanged += OnPointChanged;
            }
        }

        private void OnPointChanged(double x, double y)
        {
            Dispatcher.Invoke(() =>
            {
                Position = new Point(x + 20, y + 20);
                Region = new Rect(x - 7, y - 4, 15, 9);
                MousePoint = new Point(x, y);
            });
        }

        static MagnifierToolTip()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MagnifierToolTip), new FrameworkPropertyMetadata(typeof(MagnifierToolTip)));
        }
    }
}
