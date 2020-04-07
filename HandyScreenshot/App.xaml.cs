using System;
using System.Collections.Generic;
using System.Windows;
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
            foreach (var monitorInfo in MonitorHelper.GetMonitorInfos(Constants.ScaleFactor))
            {
                var window = new MainWindow
                {
                    Left = monitorInfo.ScreenRect.Left,
                    Top = monitorInfo.ScreenRect.Top,
                    Width = monitorInfo.ScreenRect.Width,
                    Height = monitorInfo.ScreenRect.Height
                };

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
    }
}
