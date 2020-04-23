using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HandyScreenshot.Controls
{
    public class MagnifierToolTip : Control, INotifyPropertyChanged
    {
        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(
            "Target", typeof(Visual), typeof(MagnifierToolTip), new PropertyMetadata(default(Visual)));
        public static readonly DependencyProperty PointProxyProperty = DependencyProperty.Register(
            "PointProxy", typeof(PointProxy), typeof(MagnifierToolTip), new PropertyMetadata(null, PointProxyChanged));
        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
            "Scale", typeof(double), typeof(MagnifierToolTip), new PropertyMetadata(1D, ScaleChanged));
        public static readonly DependencyProperty BitmapSourceProperty = DependencyProperty.Register(
            "BitmapSource", typeof(BitmapSource), typeof(MagnifierToolTip), new PropertyMetadata(default(BitmapSource)));

        private static void ScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MagnifierToolTip self)
            {
                self.Sizes = new MagnifierToolTipSizes(self.Scale);
            }
        }

        private static void PointProxyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MagnifierToolTip self)
            {
                self.Attach();
            }
        }

        public Visual Target
        {
            get => (Visual)GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }

        public PointProxy PointProxy
        {
            get => (PointProxy)GetValue(PointProxyProperty);
            set => SetValue(PointProxyProperty, value);
        }

        public double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        public BitmapSource BitmapSource
        {
            get => (BitmapSource)GetValue(BitmapSourceProperty);
            set => SetValue(BitmapSourceProperty, value);
        }

        private double _positionX;
        private double _positionY;
        private double _mousePointX;
        private double _mousePointY;
        private Rect _region;
        private MagnifierToolTipSizes _sizes;
        private string _colorString;
        private Color _color;

        public double PositionX
        {
            get => _positionX;
            set => SetProperty(ref _positionX, value);
        }

        public double PositionY
        {
            get => _positionY;
            set => SetProperty(ref _positionY, value);
        }

        public double MousePointX
        {
            get => _mousePointX;
            set => SetProperty(ref _mousePointX, value);
        }

        public double MousePointY
        {
            get => _mousePointY;
            set => SetProperty(ref _mousePointY, value);
        }

        public Rect Region
        {
            get => _region;
            set => SetProperty(ref _region, value);
        }

        public string ColorString
        {
            get => _colorString;
            set => SetProperty(ref _colorString, value);
        }

        public Color Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        public MagnifierToolTipSizes Sizes
        {
            get => _sizes;
            set => SetProperty(ref _sizes, value);
        }


        public MagnifierToolTip()
        {
            Sizes = new MagnifierToolTipSizes(Scale);
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
            PositionX = x + Sizes.OffsetFromMouse;
            PositionY = y + Sizes.OffsetFromMouse;
            MousePointX = x;
            MousePointY = y;
            Region = new Rect(x - Sizes.HalfRegionWidth, y - Sizes.HalfRegionHeight, Sizes.RegionWidth, Sizes.RegionHeight);
            var (r, g, b) = Dispatcher.Invoke(() => GetRgb(BitmapSource, x, y));
            ColorString = $"#{r:X2}{g:X2}{b:X2}";
            Color = Color.FromRgb(r, g, b);
        }

        private static Int32Rect _sampleRect = new Int32Rect(0, 0, 1, 1);
        private static readonly byte[] _sampleBytes = new byte[4];

        private static (byte r, byte g, byte b) GetRgb(BitmapSource source, double x, double y)
        {
            _sampleRect.X = (int)(x / 0.8);
            _sampleRect.Y = (int)(y / 0.8);
            source.CopyPixels(_sampleRect, _sampleBytes, 4, 0);
            return (_sampleBytes[2], _sampleBytes[1], _sampleBytes[0]);
        }

        static MagnifierToolTip()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MagnifierToolTip), new FrameworkPropertyMetadata(typeof(MagnifierToolTip)));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
