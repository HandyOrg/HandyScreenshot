using System;
using System.Collections.Generic;
using System.Windows;
using HandyScreenshot.Common;
using HandyScreenshot.Helpers;

namespace HandyScreenshot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ScreenshotHelper.StartScreenshot();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            while (SharedProperties.Disposables.Count > 0)
            {
                SharedProperties.Disposables.Pop().Dispose();
            }

            base.OnExit(e);
        }
    }
}
