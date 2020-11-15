using System;
using System.Windows;
using HandyScreenshot.Common;
using HandyScreenshot.Helpers;
using HandyScreenshot.Mvvm;

namespace HandyScreenshot.ViewModels
{
    public class ScreenshotState : BindableBase
    {
        private readonly Func<double, double, ReadOnlyRect> _rectDetector;
        private readonly Func<double, double, (double x, double y)> _displayXyConvertor;

        private ScreenshotMode _mode;
        private PointOrientation _orientation;
        private double _previousX;
        private double _previousY;

        public ScreenshotMode Mode
        {
            get => _mode;
            set => SetProperty(ref _mode, value);
        }

        public PointOrientation Orientation
        {
            get => _orientation;
            set => SetProperty(ref _orientation, value);
        }

        public PointProxy MousePosition { get; } = new PointProxy();

        public RectProxy ScreenshotRect { get; } = new RectProxy();


        public ScreenshotState(
            Func<double, double, ReadOnlyRect> rectDetector,
            Func<double, double, (double x, double y)> displayXYConvertor)
        {
            _rectDetector = rectDetector;
            _displayXyConvertor = displayXYConvertor;
        }

        public void PushState(MouseMessage message, double physicalX, double physicalY)
        {
            var (x, y) = _displayXyConvertor(physicalX, physicalY);
            MousePosition.Set(x, y);

            switch (message)
            {
                case MouseMessage.LeftButtonDown:
                    switch (Mode)
                    {
                        case ScreenshotMode.AutoDetect:
                            Mode = ScreenshotMode.Resizing;
                            Orientation = PointOrientation.Center;
                            (_previousX, _previousY) = (x, y);
                            break;
                        case ScreenshotMode.Fixed when ScreenshotRect.Contains(x, y):
                            Mode = ScreenshotMode.Moving;
                            Orientation = PointOrientation.Center;
                            (_previousX, _previousY) = (x, y);
                            break;
                        case ScreenshotMode.Fixed:
                            {
                                Mode = ScreenshotMode.Resizing;
                                Orientation = GetPointOrientation(x, y);
                                var right = ScreenshotRect.X + ScreenshotRect.Width;
                                var bottom = ScreenshotRect.Y + ScreenshotRect.Height;
                                (_previousX, _previousY) = Orientation switch
                                {
                                    PointOrientation.Left | PointOrientation.Top => (right, bottom),
                                    PointOrientation.Right | PointOrientation.Top => (ScreenshotRect.X, bottom),
                                    PointOrientation.Left | PointOrientation.Bottom => (right, ScreenshotRect.Y),
                                    PointOrientation.Right | PointOrientation.Bottom => (ScreenshotRect.X, ScreenshotRect.Y),
                                    _ => (_previousX, _previousY)
                                };

                                Resize(Orientation, x, y);
                                break;
                            }
                    }
                    break;
                case MouseMessage.LeftButtonUp:
                    Mode = ScreenshotMode.Fixed;
                    break;
                case MouseMessage.RightButtonDown:
                    switch (Mode)
                    {
                        case ScreenshotMode.Fixed:
                            Mode = ScreenshotMode.AutoDetect;
                            break;
                        case ScreenshotMode.AutoDetect:
                            ExitApplication();
                            break;
                    }
                    break;
                case MouseMessage.MouseMove:
                    switch (Mode)
                    {
                        case ScreenshotMode.AutoDetect:
                            var (rectX, rectY, rectWidth, rectHeight) = _rectDetector(physicalX, physicalY);
                            ScreenshotRect.Set(rectX, rectY, rectWidth, rectHeight);
                            break;
                        case ScreenshotMode.Resizing:
                            Resize(Orientation, x, y);
                            break;
                        case ScreenshotMode.Moving:
                            ScreenshotRect.Offset(_previousX, _previousY, x, y);
                            (_previousX, _previousY) = (x, y);
                            break;
                        case ScreenshotMode.Fixed:
                            Orientation = GetPointOrientation(x, y);
                            break;
                    }
                    break;
            }
        }

        private void Resize(PointOrientation orientation, double x, double y)
        {
            var newOrientation = GetPointOrientation(x, y);

            switch (orientation)
            {
                case PointOrientation.Left:
                    if (newOrientation.HasFlag(PointOrientation.Right))
                    {
                        Orientation = PointOrientation.Right;
                        ScreenshotRect.Set(ScreenshotRect.X + ScreenshotRect.Width, ScreenshotRect.Y, 0, ScreenshotRect.Height);
                    }
                    else
                    {
                        ScreenshotRect.SetLeft(x);
                    }
                    break;
                case PointOrientation.Top:
                    if (newOrientation.HasFlag(PointOrientation.Bottom))
                    {
                        Orientation = PointOrientation.Bottom;
                        ScreenshotRect.Set(ScreenshotRect.X, ScreenshotRect.Y + ScreenshotRect.Height, ScreenshotRect.Width, 0);
                    }
                    else
                    {
                        ScreenshotRect.SetTop(y);
                    }
                    break;
                case PointOrientation.Right:
                    if (newOrientation.HasFlag(PointOrientation.Left))
                    {
                        Orientation = PointOrientation.Left;
                        ScreenshotRect.Set(ScreenshotRect.X, ScreenshotRect.Y, 0, ScreenshotRect.Height);
                    }
                    else
                    {
                        ScreenshotRect.SetRight(x);
                    }
                    break;
                case PointOrientation.Bottom:
                    if (newOrientation.HasFlag(PointOrientation.Top))
                    {
                        Orientation = PointOrientation.Top;
                        ScreenshotRect.Set(ScreenshotRect.X, ScreenshotRect.Y, ScreenshotRect.Width, 0);
                    }
                    else
                    {
                        ScreenshotRect.SetBottom(y);
                    }
                    break;
                default:
                    if (IsVertex(newOrientation))
                    {
                        Orientation = newOrientation;
                    }

                    var (newX, newY, newWidth, newHeight) = GetRectByTwoPoint(_previousX, _previousY, x, y);
                    ScreenshotRect.Set(newX, newY, newWidth, newHeight);
                    break;
            }
        }

        private PointOrientation GetPointOrientation(double x, double y)
        {
            return GetPointOrientation(
                x,
                y,
                ScreenshotRect.X,
                ScreenshotRect.Y,
                ScreenshotRect.Width,
                ScreenshotRect.Height);
        }

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

        private static ReadOnlyRect GetRectByTwoPoint(double x1, double y1, double x2, double y2)
        {
            var x = Math.Min(x1, x2);
            var y = Math.Min(y1, y2);
            return (x, y,
                Math.Max(Math.Max(x1, x2) - x, 0.0),
                Math.Max(Math.Max(y1, y2) - y, 0.0));
        }

        private static bool IsVertex(PointOrientation orientation)
        {
            return orientation is (PointOrientation.Left | PointOrientation.Top) or
                (PointOrientation.Right | PointOrientation.Top) or
                (PointOrientation.Left | PointOrientation.Bottom) or
                (PointOrientation.Right | PointOrientation.Bottom);
        }

        private static void ExitApplication()
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
    }
}
