using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
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
        public static Task<bool> Start(string? filePath = null)
        {
            TaskCompletionSource<bool> tcs = new();
            bool success = false;

            var monitorInfos = MonitorHelper.GetMonitorInfos();
            var mouseEventSource = CreateMouseEventSource();
            var screenshotState = CreateScreenshotState(monitorInfos, mouseEventSource);

            Queue<Window> windows = new();
            foreach (var monitorInfo in monitorInfos)
            {
                var screenshot = ScreenshotHelper.CaptureScreen(monitorInfo.PhysicalScreenRect).ToBitmapSource();
                var vm = new MainWindowViewModel(screenshotState, screenshot, monitorInfo);
                vm.CloseCommandInvoked += OnCloseCommandInvoked;
                vm.SaveCommandInvoked += OnSaveCommandInvoked;

                var window = new MainWindow { DataContext = vm };
                SetWindowRect(window, monitorInfo.PhysicalScreenRect);
                window.Loaded += WindowOnLoaded;
                window.Closed += WindowOnClosed;
                window.Show();

                windows.Enqueue(window);
            }

            screenshotState.CloseCommandInvoked += OnCloseCommandInvoked;

            return tcs.Task;

            // Local Methods
            void WindowOnClosed(object sender, EventArgs e)
            {
                Dispose();
                tcs.TrySetResult(success);
            }

            void OnCloseCommandInvoked(object sender, EventArgs e)
            {
                success = false;
                CloseAllWindows();
            }

            void OnSaveCommandInvoked(object sender, EventArgs e)
            {
                success = true;
                ScreenshotHelper
                    .CaptureScreen(screenshotState.ScreenshotRect.ToReadOnlyRect())
                    .Save(filePath ?? $"screenshot-{DateTime.Now:yyyy-MM-dd-hh-mm-ss.fff}.png");
                CloseAllWindows();
            }

            void CloseAllWindows()
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    while (windows.Count > 0)
                    {
                        windows.Dequeue().Close();
                    }
                });
            }
        }

        private static void Dispose()
        {
            while (SharedProperties.Disposables.Count > 0)
            {
                SharedProperties.Disposables.Pop().Dispose();
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
            if (sender is Window { DataContext: MainWindowViewModel vm })
            {
                vm.Initialize();
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
