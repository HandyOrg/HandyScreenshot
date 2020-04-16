using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using HandyScreenshot.Detection;
using HandyScreenshot.Helpers;

namespace HandyScreenshot
{
    public class MainWindowViewModel : BindableBase
    {
        private Point _physicalStartPoint;
        private Rect _clipRect;
        private ClipBoxState _state;

        public Rect ClipRect
        {
            get => _clipRect;
            set => SetProperty(ref _clipRect, value);
        }

        public ClipBoxState State
        {
            get => _state;
            set => SetProperty(ref _state, value);
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
                .Subscribe(item => ChangeState(item.message, item.point));

            App.HookDisposables.Add(disposable);
        }

        public void ChangeState(MouseMessage mouseMessage, Point physicalPoint)
        {
            switch (mouseMessage)
            {
                case MouseMessage.LeftButtonDown:
                    if (State == ClipBoxState.AutoDetect)
                    {
                        State = ClipBoxState.Dragging;
                        _physicalStartPoint = physicalPoint;
                    }
                    else if (State == ClipBoxState.Static)
                    {
                        ClipRect = Union(ClipRect, ToDisplayPoint(physicalPoint));
                    }
                    break;
                case MouseMessage.LeftButtonUp:
                    if (State == ClipBoxState.Dragging)
                    {
                        State = ClipBoxState.Static;
                    }
                    break;
                case MouseMessage.RightButtonDown:
                    if (State == ClipBoxState.Static)
                    {
                        State = ClipBoxState.AutoDetect;
                    }
                    else if (State == ClipBoxState.AutoDetect)
                    {
                        // Exit
                        Application.Current.Dispatcher.Invoke(Application.Current.Shutdown);
                    }
                    break;
                case MouseMessage.MouseMove:
                    if (State == ClipBoxState.AutoDetect)
                    {
                        if (MonitorInfo.PhysicalScreenRect.Contains(physicalPoint))
                        {
                            var rect = Detector.GetByPhysicalPoint(physicalPoint);
                            ClipRect = rect == Rect.Empty ? Constants.RectZero : ToDisplayRect(rect);
                        }
                        else
                        {
                            ClipRect = Constants.RectZero;
                        }
                    }
                    else if (State == ClipBoxState.Dragging)
                    {
                        // Update Rect
                        ClipRect = ToDisplayRect(new Rect(_physicalStartPoint, physicalPoint));
                    }
                    break;
            }
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
