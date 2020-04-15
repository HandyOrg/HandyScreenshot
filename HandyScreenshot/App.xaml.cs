using System;
using System.Collections.Generic;
using System.Windows;
using HandyScreenshot.Helpers;
using HandyScreenshot.Interop;

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
            foreach (var monitorInfo in MonitorHelper.GetMonitorInfos())
            {
                var window = new MainWindow();
                SetWindowRect(window, monitorInfo.PhysicalScreenRect.Scale())

                if (window.DataContext is MainWindowViewModel vm)
                {
                    vm.MonitorInfo = monitorInfo;
                }

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
