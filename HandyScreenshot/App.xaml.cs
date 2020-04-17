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

            ScreenshotHelper.StartScreenshot();
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
