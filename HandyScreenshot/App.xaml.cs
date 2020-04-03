using System;
using System.Windows;

namespace HandyScreenshot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static IDisposable FreeHook { get; set; }

        protected override void OnExit(ExitEventArgs e)
        {
            FreeHook.Dispose();

            base.OnExit(e);
        }
    }
}
