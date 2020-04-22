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
using HandyScreenshot.Mvvm;

namespace HandyScreenshot.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _dpiString;
        private ClipBoxStatus _status;

        public string DpiString
        {
            get => _dpiString;
            set => SetProperty(ref _dpiString, value);
        }

        public ClipBoxStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public RectProxy ClipBoxRect { get; } = new RectProxy();

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
                        var p = Win32Helper.GetPhysicalMousePosition();
                        o.OnNext((message, p.X, p.Y));
                    }))
                .ObserveOn(NewThreadScheduler.Default)
                .Subscribe(item => SetState(item.message, item.x, item.y));

            SharedProperties.Disposables.Enqueue(disposable);
        }

        public void Initialize()
        {
            var initPoint = Win32Helper.GetPhysicalMousePosition();
            SetState(MouseMessage.MouseMove, initPoint.X, initPoint.Y);
        }

        private double _displayStartPointX;
        private double _displayStartPointY;

        private void SetState(MouseMessage mouseMessage, double physicalX, double physicalY)
        {
            switch (mouseMessage)
            {
                case MouseMessage.LeftButtonDown:
                    if (Status == ClipBoxStatus.AutoDetect)
                    {
                        Status = ClipBoxStatus.ResizingVertex;
                        (_displayStartPointX, _displayStartPointY) = ToDisplayPoint(physicalX, physicalY);
                    }
                    else if (Status == ClipBoxStatus.Static)
                    {
                        (double displayX, double displayY) = ToDisplayPoint(physicalX, physicalY);
                        if (ClipBoxRect.Contains(displayX, displayY))
                        {
                            Status = ClipBoxStatus.Moving;
                            (_displayStartPointX, _displayStartPointY) = ToDisplayPoint(physicalX, physicalY);
                        }
                        else
                        {
                            var right = ClipBoxRect.X + ClipBoxRect.Width;
                            var bottom = ClipBoxRect.Y + ClipBoxRect.Height;
                            if (displayX < ClipBoxRect.X && displayY < ClipBoxRect.Y)
                            {
                                Status = ClipBoxStatus.ResizingVertex;
                                _displayStartPointX = right;
                                _displayStartPointY = bottom;
                            }
                            else if (displayX > right && displayY < ClipBoxRect.Y)
                            {
                                Status = ClipBoxStatus.ResizingVertex;
                                _displayStartPointX = ClipBoxRect.X;
                                _displayStartPointY = bottom;
                            }
                            else if (displayX < ClipBoxRect.X && displayY > bottom)
                            {
                                Status = ClipBoxStatus.ResizingVertex;
                                _displayStartPointX = right;
                                _displayStartPointY = ClipBoxRect.Y;
                            }
                            else if (displayX > right && displayY > bottom)
                            {
                                Status = ClipBoxStatus.ResizingVertex;
                                _displayStartPointX = ClipBoxRect.X;
                                _displayStartPointY = ClipBoxRect.Y;
                            }
                            else if (displayX > ClipBoxRect.X && displayX < right)
                            {
                                if (displayY < ClipBoxRect.Y)
                                {
                                    Status = ClipBoxStatus.ResizingTopEdge;
                                }
                                else if (displayY > bottom)
                                {
                                    Status = ClipBoxStatus.ResizingBottomEdge;
                                }
                            }
                            else if (displayY > ClipBoxRect.Y && displayY < bottom)
                            {
                                if (displayX < ClipBoxRect.X)
                                {
                                    Status = ClipBoxStatus.ResizingLeftEdge;
                                }
                                else if (displayX > right)
                                {
                                    Status = ClipBoxStatus.ResizingRightEdge;
                                }
                            }

                            ClipBoxRect.Union(displayX, displayY);
                        }
                    }
                    break;
                case MouseMessage.LeftButtonUp:
                    Status = ClipBoxStatus.Static;
                    break;
                case MouseMessage.RightButtonDown:
                    if (Status == ClipBoxStatus.Static)
                    {
                        Status = ClipBoxStatus.AutoDetect;
                        var (x, y, w, h) = DetectRectFromPhysicalPoint(physicalX, physicalY);
                        ClipBoxRect.Set(x, y, w, h);
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
                        ClipBoxRect.Set(x, y, w, h);
                    }
                    else if (Status == ClipBoxStatus.ResizingVertex)
                    {
                        // Update Rect
                        var (displayX, displayY) = ToDisplayPoint(physicalX, physicalY);
                        var (x, y, w, h) = CalculateRectByTwoPoint(_displayStartPointX, _displayStartPointY, displayX, displayY);
                        ClipBoxRect.Set(x, y, w, h);
                    }
                    else if (Status == ClipBoxStatus.ResizingLeftEdge)
                    {
                        var displayX = ToDisplayX(physicalX);
                        if (displayX > ClipBoxRect.X + ClipBoxRect.Width)
                        {
                            Status = ClipBoxStatus.ResizingRightEdge;
                            break;
                        }

                        ClipBoxRect.SetLeft(displayX);
                    }
                    else if (Status == ClipBoxStatus.ResizingRightEdge)
                    {
                        var displayX = ToDisplayX(physicalX);
                        if (displayX < ClipBoxRect.X)
                        {
                            Status = ClipBoxStatus.ResizingLeftEdge;
                            break;
                        }

                        ClipBoxRect.SetRight(displayX);
                    }
                    else if (Status == ClipBoxStatus.ResizingTopEdge)
                    {
                        var displayY = ToDisplayY(physicalY);
                        if (displayY > ClipBoxRect.Y + ClipBoxRect.Height)
                        {
                            Status = ClipBoxStatus.ResizingBottomEdge;
                            break;
                        }

                        ClipBoxRect.SetTop(displayY);
                    }
                    else if (Status == ClipBoxStatus.ResizingBottomEdge)
                    {
                        var displayY = ToDisplayY(physicalY);
                        if (displayY < ClipBoxRect.Y)
                        {
                            Status = ClipBoxStatus.ResizingTopEdge;
                            break;
                        }

                        ClipBoxRect.SetBottom(displayY);
                    }
                    else if (Status == ClipBoxStatus.Moving)
                    {
                        (double x2, double y2) = ToDisplayPoint(physicalX, physicalY);
                        ClipBoxRect.Offset(_displayStartPointX, _displayStartPointY, x2, y2);
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

        private static ReadOnlyRect CalculateRectByTwoPoint(double x1, double y1, double x2, double y2)
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

        private double ToDisplayX(double x) => (x - MonitorInfo.PhysicalScreenRect.X) * ScaleX;

        private double ToDisplayY(double y) => (y - MonitorInfo.PhysicalScreenRect.Y) * ScaleY;
    }
}
