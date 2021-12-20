using System;
using System.Windows;
using HandyScreenshot.Common;
using HandyScreenshot.Helpers;
using HandyScreenshot.Interop;
using HandyScreenshot.Mvvm;

namespace HandyScreenshot.ViewModels
{
    public class ScreenshotState : BindableBase
    {
        private readonly Func<int, int, ReadOnlyRect> _rectDetector;

        private bool _isActivated;
        private ScreenshotMode _mode;
        private PointOrientation _orientation;
        private int _previousX;
        private int _previousY;

        public bool IsActivated
        {
            get => _isActivated;
            set => SetProperty(ref _isActivated, value);
        }

        public ScreenshotMode Mode
        {
            get => _mode;
            private set => SetProperty(ref _mode, value);
        }

        public PointOrientation Orientation
        {
            get => _orientation;
            private set => SetProperty(ref _orientation, value);
        }

        public PointProxy MousePosition { get; } = new();

        public RectProxy ScreenshotRect { get; } = new();

        public ScreenshotState(Func<int, int, ReadOnlyRect> rectDetector)
        {
            _rectDetector = rectDetector;
        }

        public void PushState(MouseMessage message, int physicalX, int physicalY)
        {
            if (!IsActivated)
            {
                return;
            }

            bool canUpdateMousePosition = true;

            switch (message)
            {
                case MouseMessage.LeftButtonDown:
                    switch (Mode)
                    {
                        case ScreenshotMode.AutoDetect:
                            Mode = ScreenshotMode.Resizing;
                            Orientation = PointOrientation.Center;
                            (_previousX, _previousY) = (physicalX, physicalY);
                            break;
                        case ScreenshotMode.Fixed when ScreenshotRect.Contains(physicalX, physicalY):
                            Mode = ScreenshotMode.Moving;
                            Orientation = PointOrientation.Center;
                            (_previousX, _previousY) = (physicalX, physicalY);
                            break;
                        case ScreenshotMode.Fixed:
                            {
                                Mode = ScreenshotMode.Resizing;
                                Orientation = GetPointOrientation(physicalX, physicalY);
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

                                Resize(Orientation, physicalX, physicalY);
                                break;
                            }
                    }
                    break;
                case MouseMessage.LeftButtonUp:
                    Mode = ScreenshotMode.Fixed;
                    // [防抖]：在截图完成，释放鼠标时，人的手很可能会抖，造成大约一两个像素的偏移，尤其是用触控板时，对强迫症患者造成极大的伤害；
                    // 所以 Resizing 或 Moving 结束的最后一个鼠标点不应当被绘制（要丢弃掉），这里是指 ClipBox 和 Magnifier 控件都不要绘制。
                    // P.S. 当然，具体偏移多少，和释放时的手速有关，坐标点毕竟是离散的。
                    canUpdateMousePosition = false;
                    break;
                case MouseMessage.RightButtonDown:
                    switch (Mode)
                    {
                        case ScreenshotMode.Fixed:
                            Mode = ScreenshotMode.AutoDetect;
                            DetectAutomatically(physicalX, physicalY);
                            Orientation = PointOrientation.Center;
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
                            DetectAutomatically(physicalX, physicalY);
                            break;
                        case ScreenshotMode.Resizing:
                            Resize(Orientation, physicalX, physicalY);
                            break;
                        case ScreenshotMode.Moving:
                            ScreenshotRect.Offset(_previousX, _previousY, physicalX, physicalY);
                            (_previousX, _previousY) = (physicalX, physicalY);
                            break;
                        case ScreenshotMode.Fixed:
                            Orientation = GetPointOrientation(physicalX, physicalY);
                            break;
                    }
                    break;
            }

            if (canUpdateMousePosition)
            {
                MousePosition.Set(physicalX, physicalY);
            }
            else
            {
                // 接上面的[防抖]所述，最后一个鼠标坐标虽然没有影响绘制，但是实际上 Cursor 还是抖动到了真实的位置，
                // 这样就会使得 Cursor 与绘制的图像不重合，就很丑，显得十分不专业。
                // 所以应当重置鼠标位置到上一个鼠标位置上。（我是不是考虑的很细致？（￣︶￣）↗）
                NativeMethods.SetCursorPos(MousePosition.X, MousePosition.Y);
            }
        }

        private void DetectAutomatically(int physicalX, int physicalY)
        {
            var (rectX, rectY, rectWidth, rectHeight) = _rectDetector(physicalX, physicalY);
            ScreenshotRect.Set(rectX, rectY, rectWidth, rectHeight);
        }

        private void Resize(PointOrientation orientation, int x, int y)
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

        private static ReadOnlyRect GetRectByTwoPoint(int x1, int y1, int x2, int y2)
        {
            var x = Math.Min(x1, x2);
            var y = Math.Min(y1, y2);
            return (
                x,
                y,
                Math.Max(Math.Max(x1, x2) - x, 0),
                Math.Max(Math.Max(y1, y2) - y, 0));
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
