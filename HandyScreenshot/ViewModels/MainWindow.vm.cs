using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using HandyScreenshot.Common;
using HandyScreenshot.Controls;
using HandyScreenshot.Detection;
using HandyScreenshot.Helpers;
using HandyScreenshot.Interop;
using HandyScreenshot.Mvvm;

namespace HandyScreenshot.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private ClipBoxStatus _status;

        private string _dpiString;

        public string DpiString
        {
            get => _dpiString;
            set => SetProperty(ref _dpiString, value);
        }

        public RectOperation RectOperation { get; } = new RectOperation();

        public ClipBoxStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public RectDetector Detector { get; set; }

        public BitmapSource Background { get; set; }

        public MonitorInfo MonitorInfo { get; set; }

        public double ScaleX { get; set; }

        public double ScaleY { get; set; }

        public ICommand CloseCommand { get; } = new RelayCommand(() => Application.Current.Shutdown());

        public MainWindowViewModel()
        {
            var disposable = Observable.Create<(MouseMessage message, double x, double y)>(o =>
                    Win32Helper.SubscribeMouseHook((message, info) =>
                    {
                        var (x, y) = ToPoint(Win32Helper.GetPhysicalMousePosition());
                        o.OnNext((message, x, y));
                    }))
                .ObserveOn(NewThreadScheduler.Default)
                .Subscribe(item => SetState(item.message, item.x, item.y));

            SharedProperties.Disposables.Enqueue(disposable);
        }

        private double _displayStartPointX;
        private double _displayStartPointY;

        public void SetState(MouseMessage mouseMessage, double physicalX, double physicalY)
        {
            switch (mouseMessage)
            {
                case MouseMessage.LeftButtonDown:
                    if (Status == ClipBoxStatus.AutoDetect)
                    {
                        Status = ClipBoxStatus.Dragging;
                        (_displayStartPointX, _displayStartPointY) = ToDisplayPoint(physicalX, physicalY);
                    }
                    else if (Status == ClipBoxStatus.Static)
                    {
                        (double x, double y) = ToDisplayPoint(physicalX, physicalY);
                        if (RectOperation.Contains(x, y))
                        {
                            Status = ClipBoxStatus.Moving;
                            (_displayStartPointX, _displayStartPointY) = ToDisplayPoint(physicalX, physicalY);
                        }
                        else
                        {
                            Status = ClipBoxStatus.Dragging;
                            RectOperation.Union(x, y);
                        }
                    }
                    break;
                case MouseMessage.LeftButtonUp:
                    if (Status == ClipBoxStatus.Dragging || Status == ClipBoxStatus.Moving)
                    {
                        Status = ClipBoxStatus.Static;
                    }
                    break;
                case MouseMessage.RightButtonDown:
                    if (Status == ClipBoxStatus.Static)
                    {
                        Status = ClipBoxStatus.AutoDetect;
                        var (x, y, w, h) = DetectRectFromPhysicalPoint(physicalX, physicalY);
                        RectOperation.Set(x, y, w, h);
                    }
                    else if (Status == ClipBoxStatus.AutoDetect)
                    {
                        // Exit
                        Application.Current.Dispatcher.Invoke(Application.Current.Shutdown);
                    }
                    break;
                case MouseMessage.MouseMove:
                    if (Status == ClipBoxStatus.AutoDetect)
                    {
                        var (x, y, w, h) = DetectRectFromPhysicalPoint(physicalX, physicalY);
                        RectOperation.Set(x, y, w, h);
                    }
                    else if (Status == ClipBoxStatus.Dragging)
                    {
                        // Update Rect
                        var (displayX, displayY) = ToDisplayPoint(physicalX, physicalY);
                        var (x, y, w, h) = CalculateRect(_displayStartPointX, _displayStartPointY, displayX, displayY);
                        RectOperation.Set(x, y, w, h);
                    }
                    else if (Status == ClipBoxStatus.Moving)
                    {
                        (double x2, double y2) = ToDisplayPoint(physicalX, physicalY);
                        RectOperation.Offset(_displayStartPointX, _displayStartPointY, x2, y2);
                        (_displayStartPointX, _displayStartPointY) = (x2, y2);
                    }
                    break;
            }
        }

        private ReadOnlyRect DetectRectFromPhysicalPoint(double physicalX, double physicalY)
        {
            var rect = Detector.GetByPhysicalPoint(physicalX, physicalY);
            return rect != ReadOnlyRect.Empty && MonitorInfo.PhysicalScreenRect.IntersectsWith(rect)
                ? ToDisplayRect(rect)
                : ReadOnlyRect.Zero;
        }

        private static ReadOnlyRect CalculateRect(double x1, double y1, double x2, double y2)
        {
            var x = Math.Min(x1, x2);
            var y = Math.Min(y1, y2);
            return (x, y,
                    Math.Max(Math.Max(x1, x2) - x, 0.0),
                    Math.Max(Math.Max(y1, y2) - y, 0.0));
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

        private static (double X, double Y) ToPoint(NativeMethods.POINT point) => (point.X, point.Y);
    }
}
