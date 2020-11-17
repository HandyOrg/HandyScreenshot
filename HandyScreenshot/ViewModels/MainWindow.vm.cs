using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HandyScreenshot.Common;
using HandyScreenshot.Detection;
using HandyScreenshot.Helpers;
using HandyScreenshot.Mvvm;

namespace HandyScreenshot.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private static readonly byte[] SampleBytes = new byte[4];

        private readonly RectDetector _detector;
        private string _dpiString = string.Empty;

        public string DpiString
        {
            get => _dpiString;
            set => SetProperty(ref _dpiString, value);
        }

        public Func<double, double, Color> ColorGetter { get; }

        public BitmapSource Background { get; }

        public MonitorInfo MonitorInfo { get; }

        public Rect ScreenBound { get; }

        public double ScaleX { get; }

        public double ScaleY { get; }

        public ICommand CloseCommand { get; } = new RelayCommand(() => Application.Current.Shutdown());

        public ScreenshotState State { get; }

        public MainWindowViewModel(
            IObservable<(MouseMessage message, int x, int y)> mouseEventSource,
            BitmapSource background,
            MonitorInfo monitorInfo,
            RectDetector detector,
            double scaleX,
            double scaleY)
        {
            State = new ScreenshotState(
                DetectRectFromPhysicalPoint,
                ToDisplayPoint);
            Background = background;
            MonitorInfo = monitorInfo;
            var screenRect = monitorInfo.PhysicalScreenRect;
            var screenBound = new Rect(0, 0, screenRect.Width, screenRect.Height);
            screenBound.Scale(scaleX, scaleY);
            ScreenBound = screenBound;
            ScaleX = scaleX;
            ScaleY = scaleY;
            _detector = detector;

            var disposable = mouseEventSource
                .Subscribe(i => State.PushState(i.message, i.x, i.y));

            ColorGetter = GetColorByCoordinate;

            SharedProperties.Disposables.Push(disposable);
        }

        public void Initialize()
        {
            var initPoint = Win32Helper.GetPhysicalMousePosition();
            State.PushState(MouseMessage.MouseMove, initPoint.X, initPoint.Y);
        }

        private Color GetColorByCoordinate(double x, double y)
        {
            var physicalX = (int) (x / ScaleX);
            var physicalY = (int) (y / ScaleX);

            if (physicalX < 0 || physicalX >= Background.PixelWidth ||
                physicalY < 0 || physicalY >= Background.PixelHeight) return Colors.Transparent;

            Background.CopyPixels(
                new Int32Rect(physicalX, physicalY, 1, 1),
                SampleBytes, 4, 0);

            return Color.FromArgb(SampleBytes[3], SampleBytes[2], SampleBytes[1], SampleBytes[0]);
        }

        private ReadOnlyRect DetectRectFromPhysicalPoint(double physicalX, double physicalY)
        {
            var rect = _detector.GetByPhysicalPoint(physicalX, physicalY);
            return rect != ReadOnlyRect.Empty && MonitorInfo.PhysicalScreenRect.IntersectsWith(rect)
                ? ToDisplayRect(rect)
                : ReadOnlyRect.Zero;
        }

        private ReadOnlyRect ToDisplayRect(ReadOnlyRect physicalRect)
        {
            return physicalRect
                .Offset(MonitorInfo.PhysicalScreenRect.X, MonitorInfo.PhysicalScreenRect.Y)
                .Scale(ScaleX, ScaleY);
        }

        private (double X, double Y) ToDisplayPoint(double x, double y)
        {
            return (
                (x - MonitorInfo.PhysicalScreenRect.X) * ScaleX,
                (y - MonitorInfo.PhysicalScreenRect.Y) * ScaleY);
        }
    }
}