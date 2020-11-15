using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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
        private ScreenshotMode _status;

        public string DpiString
        {
            get => _dpiString;
            set => SetProperty(ref _dpiString, value);
        }

        public ScreenshotMode Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public Func<double, double, Color> ColorGetter { get; }

        public RectProxy ClipBoxRect { get; } = new RectProxy();

        public PointProxy MousePoint { get; } = new PointProxy();

        public RectDetector Detector { get; set; }

        public BitmapSource Background { get; set; }

        public MonitorInfo MonitorInfo { get; set; }

        public double ScaleX { get; set; }

        public double ScaleY { get; set; }

        public double Scale { get; set; }

        public ICommand CloseCommand { get; } = new RelayCommand(() => Application.Current.Shutdown());

        private static readonly byte[] SampleBytes = new byte[4];

        public MainWindowViewModel(IObservable<(MouseMessage message, double x, double y)> mouseEventSource)
        {
            var disposable1 = mouseEventSource
                .Subscribe(i => SetState(i.message, i.x, i.y));
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
            SetState(MouseMessage.MouseMove, initPoint.X, initPoint.Y);
            SetMagnifierState(MouseMessage.MouseMove, initPoint.X, initPoint.Y);
        }

        private void SetMagnifierState(MouseMessage mouseMessage, double physicalX, double physicalY)
        {
            if (mouseMessage == MouseMessage.MouseMove)
            {
                var (displayX, displayY) = ToDisplayPoint(physicalX, physicalY);
                MousePoint.Set(displayX, displayY);
            }
        }

        private double _displayStartPointX;
        private double _displayStartPointY;

        private void SetState(MouseMessage mouseMessage, double physicalX, double physicalY)
        {
            switch (mouseMessage)
            {
                case MouseMessage.LeftButtonDown:
                    if (Status == ScreenshotMode.AutoDetect)
                    {
                        Status = ScreenshotMode.ResizingVertex;
                        (_displayStartPointX, _displayStartPointY) = ToDisplayPoint(physicalX, physicalY);
                    }
                    else if (Status == ScreenshotMode.Fixed)
                    {
                        (double displayX, double displayY) = ToDisplayPoint(physicalX, physicalY);
                        if (ClipBoxRect.Contains(displayX, displayY))
                        {
                            Status = ScreenshotMode.Moving;
                            (_displayStartPointX, _displayStartPointY) = ToDisplayPoint(physicalX, physicalY);
                        }
                        else
                        {
                            var right = ClipBoxRect.X + ClipBoxRect.Width;
                            var bottom = ClipBoxRect.Y + ClipBoxRect.Height;
                            if (displayX < ClipBoxRect.X && displayY < ClipBoxRect.Y)
                            {
                                Status = ScreenshotMode.ResizingVertex;
                                _displayStartPointX = right;
                                _displayStartPointY = bottom;
                            }
                            else if (displayX > right && displayY < ClipBoxRect.Y)
                            {
                                Status = ScreenshotMode.ResizingVertex;
                                _displayStartPointX = ClipBoxRect.X;
                                _displayStartPointY = bottom;
                            }
                            else if (displayX < ClipBoxRect.X && displayY > bottom)
                            {
                                Status = ScreenshotMode.ResizingVertex;
                                _displayStartPointX = right;
                                _displayStartPointY = ClipBoxRect.Y;
                            }
                            else if (displayX > right && displayY > bottom)
                            {
                                Status = ScreenshotMode.ResizingVertex;
                                _displayStartPointX = ClipBoxRect.X;
                                _displayStartPointY = ClipBoxRect.Y;
                            }
                            else if (displayX > ClipBoxRect.X && displayX < right)
                            {
                                if (displayY < ClipBoxRect.Y)
                                {
                                    Status = ScreenshotMode.ResizingTop;
                                }
                                else if (displayY > bottom)
                                {
                                    Status = ScreenshotMode.ResizingBottom;
                                }
                            }
                            else if (displayY > ClipBoxRect.Y && displayY < bottom)
                            {
                                if (displayX < ClipBoxRect.X)
                                {
                                    Status = ScreenshotMode.ResizingLeft;
                                }
                                else if (displayX > right)
                                {
                                    Status = ScreenshotMode.ResizingRight;
                                }
                            }

                            ClipBoxRect.Union(displayX, displayY);
                        }
                    }
                    break;
                case MouseMessage.LeftButtonUp:
                    Status = ScreenshotMode.Fixed;
                    break;
                case MouseMessage.RightButtonDown:
                    if (Status == ScreenshotMode.Fixed)
                    {
                        Status = ScreenshotMode.AutoDetect;
                        var (x, y, w, h) = DetectRectFromPhysicalPoint(physicalX, physicalY);
                        ClipBoxRect.Set(x, y, w, h);
                    }
                    else if (Status == ScreenshotMode.AutoDetect)
                    {
                        try
                        {
                            // Exit
                            Application.Current.Dispatcher.Invoke(Application.Current.Shutdown);
                        }
                        catch (OperationCanceledException)
                        {
                            // Ignore
                        }
                    }
                    break;
                case MouseMessage.MouseMove:
                    if (Status == ScreenshotMode.AutoDetect)
                    {
                        var (x, y, w, h) = DetectRectFromPhysicalPoint(physicalX, physicalY);
                        ClipBoxRect.Set(x, y, w, h);
                    }
                    else if (Status == ScreenshotMode.ResizingVertex)
                    {
                        // Update Rect
                        var (displayX, displayY) = ToDisplayPoint(physicalX, physicalY);
                        var (x, y, w, h) = CalculateRectByTwoPoint(_displayStartPointX, _displayStartPointY, displayX, displayY);
                        ClipBoxRect.Set(x, y, w, h);
                    }
                    else if (Status == ScreenshotMode.ResizingLeft)
                    {
                        var displayX = ToDisplayX(physicalX);
                        if (displayX > ClipBoxRect.X + ClipBoxRect.Width)
                        {
                            Status = ScreenshotMode.ResizingRight;
                            break;
                        }

                        ClipBoxRect.SetLeft(displayX);
                    }
                    else if (Status == ScreenshotMode.ResizingRight)
                    {
                        var displayX = ToDisplayX(physicalX);
                        if (displayX < ClipBoxRect.X)
                        {
                            Status = ScreenshotMode.ResizingLeft;
                            break;
                        }

                        ClipBoxRect.SetRight(displayX);
                    }
                    else if (Status == ScreenshotMode.ResizingTop)
                    {
                        var displayY = ToDisplayY(physicalY);
                        if (displayY > ClipBoxRect.Y + ClipBoxRect.Height)
                        {
                            Status = ScreenshotMode.ResizingBottom;
                            break;
                        }

                        ClipBoxRect.SetTop(displayY);
                    }
                    else if (Status == ScreenshotMode.ResizingBottom)
                    {
                        var displayY = ToDisplayY(physicalY);
                        if (displayY < ClipBoxRect.Y)
                        {
                            Status = ScreenshotMode.ResizingTop;
                            break;
                        }

                        ClipBoxRect.SetBottom(displayY);
                    }
                    else if (Status == ScreenshotMode.Moving)
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

        private static PointOrientation GetPointOrientation(
            double pointX,
            double pointY,
            double rectX,
            double rectY,
            double rectWidth,
            double rectHeight)
        {
            var horizontal = pointX <= rectX
                ? PointOrientation.Left
                : pointX < rectX + rectWidth
                    ? PointOrientation.Center
                    : PointOrientation.Right;
            var vertical = pointY <= rectY
                ? PointOrientation.Top
                : pointY < rectY + rectHeight
                    ? PointOrientation.Center
                    : PointOrientation.Bottom;

            return horizontal | vertical;
        }
    }
}
