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
        private string _dpiString = string.Empty;

        public string DpiString
        {
            get => _dpiString;
            set => SetProperty(ref _dpiString, value);
        }

        public Func<double, double, Color> ColorGetter { get; }

        public PointProxy MousePoint { get; } = new PointProxy();

        public RectDetector Detector { get; }

        public BitmapSource Background { get; }

        public MonitorInfo MonitorInfo { get; }

        public double ScaleX { get; set; }

        public double ScaleY { get; set; }

        public double Scale { get; set; }

        public ICommand CloseCommand { get; } = new RelayCommand(() => Application.Current.Shutdown());

        private static readonly byte[] SampleBytes = new byte[4];

        public ScreenshotState State { get; }

        public MainWindowViewModel(
            IObservable<(MouseMessage message, double x, double y)> mouseEventSource,
            BitmapSource background,
            MonitorInfo monitorInfo,
            RectDetector detector)
        {
            State = new ScreenshotState(
                DetectRectFromPhysicalPoint,
                ToDisplayPoint);
            Background = background;
            MonitorInfo = monitorInfo;
            Detector = detector;

            var disposable1 = mouseEventSource
                .Subscribe(i => State.PushState(i.message, i.x, i.y));
            var disposable2 = mouseEventSource
                .Subscribe(i => SetMagnifierState(i.message, i.x, i.y));

            ColorGetter = (x, y) =>
            {
                var physicalX = (int)(x / ScaleX);
                var physicalY = (int)(y / ScaleX);

                if (physicalX < 0 || physicalX >= Background.PixelWidth ||
                    physicalY < 0 || physicalY >= Background.PixelHeight) return Colors.Transparent;

                Background.CopyPixels(
                    new Int32Rect(physicalX, physicalY, 1, 1),
                    SampleBytes, 4, 0);

                return Color.FromArgb(SampleBytes[3], SampleBytes[2], SampleBytes[1], SampleBytes[0]);
            };

            SharedProperties.Disposables.Push(disposable1);
            SharedProperties.Disposables.Push(disposable2);
        }

        public void Initialize()
        {
            var initPoint = Win32Helper.GetPhysicalMousePosition();
            State.PushState(MouseMessage.MouseMove, initPoint.X, initPoint.Y);
            SetMagnifierState(MouseMessage.MouseMove, initPoint.X, initPoint.Y);
        }

        private void SetMagnifierState(MouseMessage mouseMessage, double physicalX, double physicalY)
        {
            if (mouseMessage != MouseMessage.MouseMove) return;

            var (displayX, displayY) = ToDisplayPoint(physicalX, physicalY);
            MousePoint.Set(displayX, displayY);
        }

        private ReadOnlyRect DetectRectFromPhysicalPoint(double physicalX, double physicalY)
        {
            var rect = Detector.GetByPhysicalPoint(physicalX, physicalY);
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
