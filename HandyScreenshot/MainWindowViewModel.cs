using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using HandyScreenshot.Helpers;
using HandyScreenshot.UiElementDetection;

namespace HandyScreenshot
{
    public class MainWindowViewModel : BindableBase
    {
        private CachedElement _selectedElement;
        private Rect _rect;

        public CachedElement SelectedElement
        {
            get => _selectedElement;
            set => SetProperty(ref _selectedElement, value);
        }

        public Rect Rect
        {
            get => _rect;
            set => SetProperty(ref _rect, value);
        }

        public MonitorInfo MonitorInfo { get; set; }

        public double ScaleX { get; set; }

        public double ScaleY { get; set; }

        public ICommand CloseCommand { get; } = new RelayCommand(() => Application.Current.Shutdown());

        public MainWindowViewModel()
        {
            var detector = new ElementDetector();
            detector.Snapshot();

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
                        SelectedElement = detector.GetByPhysicalPoint(physicalPoint);
                        Rect = SelectedElement != null
                            ? ToDisplayRect(SelectedElement.PhysicalRect)
                            : Constants.RectZero;
                    }
                    else
                    {
                        SelectedElement = null;
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
