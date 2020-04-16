using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using HandyScreenshot.Helpers;
using HandyScreenshot.UiElementDetection;

namespace HandyScreenshot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static ICollection<IDisposable> HookDisposables { get; } = new List<IDisposable>();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            StartScreenshot();
        }

        public static void StartScreenshot()
        {
            var monitorInfos = MonitorHelper.GetMonitorInfos().ToList();
            var primaryScreen = monitorInfos.First(item => item.IsPrimaryScreen);
            var (primaryScreenScaleX, primaryScreenScaleY) = MonitorHelper.GetScaleFactorFromMonitor(primaryScreen.Handle);

            
            var detector = new RectDetector();
            detector.Snapshot(monitorInfos
                .Select(item => item.PhysicalScreenRect)
                .Aggregate((acc, item) =>
                {
                    acc.Union(item);
                    return acc;
                }));
            foreach (var monitorInfo in monitorInfos)
            {
                var window = new MainWindow();

                var physicalRect = monitorInfo.PhysicalScreenRect;
                var rect = new Rect(physicalRect.X, physicalRect.Y, physicalRect.Width, physicalRect.Height);
                var (scaleX, scaleY) = MonitorHelper.GetScaleFactorFromMonitor(monitorInfo.Handle);
                rect.Scale(primaryScreenScaleX, primaryScreenScaleY);

                SetWindowRect(window, rect);

                window.DataContext = new MainWindowViewModel
                {
                    MonitorInfo = monitorInfo,
                    ScaleX = scaleX,
                    ScaleY = scaleY,
                    Background = ScreenshotHelper.CaptureScreen(monitorInfo.PhysicalScreenRect),
                    Detector = detector
                };

                window.Show();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            foreach (var hookDisposable in HookDisposables)
            {
                hookDisposable.Dispose();
            }

            base.OnExit(e);
        }

        private static void SetWindowRect(Window window, Rect rect)
        {
            window.Left = rect.Left;
            window.Top = rect.Top;
            window.Width = rect.Width;
            window.Height = rect.Height;
        }
    }
}
