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
        private Rect _rect;

        public Rect Rect
        {
            get => _rect;
            set => SetProperty(ref _rect, value);
        }

        public RectDetector Detector { get; set; }

        public BitmapSource Background { get; set; }

        public MonitorInfo MonitorInfo { get; set; }

        public double ScaleX { get; set; }

        public double ScaleY { get; set; }

        public ICommand CloseCommand { get; } = new RelayCommand(() => Application.Current.Shutdown());

        public MainWindowViewModel()
        {
            var disposable = Observable.Create<Point>(o =>
                    Win32Helper.SubscribeMouseHook((message, info) =>
                    {
                        if (message == MouseMessage.MouseMove)
                        {
                            o.OnNext(Win32Helper.GetPhysicalMousePosition().ToPoint());
                        }
                    }))
                .ObserveOn(NewThreadScheduler.Default)
                .Subscribe(physicalPoint =>
                {
                    if (MonitorInfo.PhysicalScreenRect.Contains(physicalPoint))
                    {
                        var rect = Detector.GetByPhysicalPoint(physicalPoint);
                        Rect = rect == Rect.Empty ? Constants.RectZero : ToDisplayRect(rect);
                    }
                    else
                    {
                        Rect = Constants.RectZero;
                    }
                });

            App.HookDisposables.Add(disposable);
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
    }
}
