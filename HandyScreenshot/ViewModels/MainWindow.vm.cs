using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using HandyScreenshot.Detection;
using HandyScreenshot.Helpers;
using HandyScreenshot.Mvvm;

namespace HandyScreenshot.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private Point _physicalStartPoint;
        private Rect _clipRect;
        private ClipBoxStatus _status;

        private string _dpiString;

        public string DpiString
        {
            get => _dpiString;
            set => SetProperty(ref _dpiString, value);
        }

        public Rect ClipRect
        {
            get => _clipRect;
            set => SetProperty(ref _clipRect, value);
        }

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
            var disposable = Observable.Create<(MouseMessage message, Point point)>(o => Win32Helper.SubscribeMouseHook((message, info) =>
                    o.OnNext((message, point: Win32Helper.GetPhysicalMousePosition().ToPoint()))))
                .ObserveOn(NewThreadScheduler.Default)
                .Subscribe(item => SetState(item.message, item.point));

            App.HookDisposables.Add(disposable);
        }

        public void SetState(MouseMessage mouseMessage, Point physicalPoint)
        {
            switch (mouseMessage)
            {
                case MouseMessage.LeftButtonDown:
                    if (Status == ClipBoxStatus.AutoDetect)
                    {
                        Status = ClipBoxStatus.Dragging;
                        _physicalStartPoint = physicalPoint;
                    }
                    else if (Status == ClipBoxStatus.Static)
                    {
                        Status = ClipBoxStatus.Dragging;
                        ClipRect = Union(ClipRect, ToDisplayPoint(physicalPoint));
                    }
                    break;
                case MouseMessage.LeftButtonUp:
                    if (Status == ClipBoxStatus.Dragging)
                    {
                        Status = ClipBoxStatus.Static;
                    }
                    break;
                case MouseMessage.RightButtonDown:
                    if (Status == ClipBoxStatus.Static)
                    {
                        Status = ClipBoxStatus.AutoDetect;
                        ClipRect = DetectRectFromPhysicalPoint(physicalPoint);
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
                        ClipRect = DetectRectFromPhysicalPoint(physicalPoint);
                    }
                    else if (Status == ClipBoxStatus.Dragging)
                    {
                        // Update Rect
                        ClipRect = ToDisplayRect(new Rect(_physicalStartPoint, physicalPoint));
                    }
                    break;
            }
        }

        private Rect DetectRectFromPhysicalPoint(Point physicalPoint)
        {
            var rect = Detector.GetByPhysicalPoint(physicalPoint);
            return rect != Rect.Empty && MonitorInfo.PhysicalScreenRect.IntersectsWith(rect)
                ? ToDisplayRect(rect)
                : Constants.RectZero;
        }

        private Rect ToDisplayRect(Rect physicalRect)
        {
            var rect = new Rect(
                physicalRect.X - MonitorInfo.PhysicalScreenRect.X,
                physicalRect.Y - MonitorInfo.PhysicalScreenRect.Y,
                physicalRect.Width,
                physicalRect.Height);
            rect.Scale(ScaleX, ScaleY);

            return rect;
        }

        private Point ToDisplayPoint(Point physicalPoint)
        {
            var point = new Point(physicalPoint.X, physicalPoint.Y);
            point.Offset(MonitorInfo.PhysicalScreenRect.X, MonitorInfo.PhysicalScreenRect.Y);
            point.X *= ScaleX;
            point.Y *= ScaleY;

            return point;
        }

        private static Rect Union(Rect rect, Point point)
        {
            return new Rect(
               new Point(Math.Min(rect.X, point.X), Math.Min(rect.Y, point.Y)),
               new Point(Math.Max(rect.Right, point.X), Math.Max(rect.Bottom, point.Y)));
        }
    }
}
