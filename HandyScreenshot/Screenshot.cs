using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using HandyScreenshot.Common;
using HandyScreenshot.Detection;
using HandyScreenshot.Helpers;
using HandyScreenshot.ViewModels;
using HandyScreenshot.Views;
using static HandyScreenshot.Interop.NativeMethods;

namespace HandyScreenshot
{
    public static class Screenshot
    {
        public static void Start()
        {
            var monitorInfos = MonitorHelper.GetMonitorInfos();
            var mouseEventSource = CreateMouseEventSource();
            var screenshotState = CreateScreenshotState(monitorInfos, mouseEventSource);

            foreach (var monitorInfo in monitorInfos)
            {
                var screenshot = ScreenshotHelper.CaptureScreen(monitorInfo.PhysicalScreenRect).ToBitmapSource();
                var vm = new MainWindowViewModel(screenshotState, screenshot, monitorInfo);

                var window = new MainWindow { DataContext = vm };
                SetWindowRect(window, monitorInfo.PhysicalScreenRect);
                window.Loaded += WindowOnLoaded;
                window.Show();
            }
        }

        private static ScreenshotState CreateScreenshotState(
            IEnumerable<MonitorInfo> monitorInfos,
            IObservable<(MouseMessage message, int x, int y)> mouseEventSource)
        {
            var detector = new RectDetector();
            detector.Snapshot(monitorInfos
                .Select(item => item.PhysicalScreenRect)
                .Aggregate((acc, item) => acc.Union(item)));

            var state = new ScreenshotState(DetectRectFromPhysicalPoint);
            var disposable = mouseEventSource
                .Subscribe(tuple =>
                {
                    var (message, x, y) = tuple;
                    state.PushState(message, x, y);
                });

            SharedProperties.Disposables.Push(disposable);
            return state;

            // Local Method
            ReadOnlyRect DetectRectFromPhysicalPoint(int physicalX, int physicalY)
            {
                var rect = detector.GetByPhysicalPoint(physicalX, physicalY);
                return rect != ReadOnlyRect.Empty /*&& currentMonitorInfo.PhysicalScreenRect.IntersectsWith(rect)*/
                    ? rect
                    : ReadOnlyRect.Zero;
            }
        }

        private static IObservable<(MouseMessage message, int x, int y)> CreateMouseEventSource()
        {
            var hotSource = Observable.Create<(MouseMessage message, int x, int y)>(o =>
                    Win32Helper.SubscribeMouseHook((message, _) =>
                    {
                        var p = Win32Helper.GetPhysicalMousePosition();
                        o.OnNext((message, p.X, p.Y));
                    }))
                .ObserveOn(NewThreadScheduler.Default)
                .Publish();

            var disposable = hotSource.Connect();
            SharedProperties.Disposables.Push(disposable);

            return hotSource;
        }

        // For DEBUG
        private static void WindowOnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is Window { DataContext: MainWindowViewModel vm } window)
            {
                vm.Initialize();

                var source = PresentationSource.FromVisual(window);
                if (source?.CompositionTarget is null)
                {
                    return;
                }

                var dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                var dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;
                vm.DpiString = $"{dpiX}, {dpiY}";
            }
        }

        private static void SetWindowRect(Window window, ReadOnlyRect rect)
        {
            SetWindowPos(
                window.GetHandle(),
                (IntPtr)HWND_TOPMOST,
                rect.X,
                rect.Y,
                rect.Width,
                rect.Height,
                SWP_NOZORDER);
        }
    }
}
