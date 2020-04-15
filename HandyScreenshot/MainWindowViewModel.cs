using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using HandyScreenshot.Helpers;
using HandyScreenshot.Interop;
using HandyScreenshot.UiElementDetection;

namespace HandyScreenshot
{
    public class MainWindowViewModel : BindableBase
    {
        private Point _mousePosition;
        private CachedElement _selectedElement;
        private Rect _rect;

        public CachedElement SelectedElement
        {
            get => _selectedElement;
            set => SetProperty(ref _selectedElement, value);
        }

        public Point MousePosition
        {
            get => _mousePosition;
            set => SetProperty(ref _mousePosition, value);
        }

        public Rect Rect
        {
            get => _rect;
            set => SetProperty(ref _rect, value);
        }

        public MonitorInfo MonitorInfo { get; set; }

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
                .Subscribe(point =>
                {
                    MousePosition = point;
                    if (MonitorInfo.PhysicalScreenRect.Contains(MousePosition))
                    {
                        SelectedElement = detector.GetByPoint(MousePosition);
                        Rect = SelectedElement != null
                            ? RebaseRect(SelectedElement.PhysicalRect, MonitorInfo.PhysicalScreenRect.Left, MonitorInfo.PhysicalScreenRect.Top)
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

        private static Rect RebaseRect(Rect rect, double originX, double originY)
        {
            return new Rect(
                rect.Left - originX,
                rect.Top - originY,
                rect.Width,
                rect.Height);
        }
    }
}
