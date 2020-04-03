using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using HandyScreenshot.Interop;

namespace HandyScreenshot
{
    public class MainWindowViewModel : BindableBase
    {
        private Point _mousePosition;

        private DisplayWindowInfo _selectedWindow;

        public DisplayWindowInfo SelectedWindow
        {
            get => _selectedWindow;
            set => SetProperty(ref _selectedWindow, value);
        }

        public Point MousePosition
        {
            get => _mousePosition;
            set => SetProperty(ref _mousePosition, value);
        }

        public ICommand CloseCommand { get; } = new RelayCommand(() => Application.Current.Shutdown());

        public ICommand RefreshWindowsCommand { get; }

        private IReadOnlyList<DisplayWindowInfo> _windows;

        public MainWindowViewModel()
        {
            RefreshWindowsCommand = new RelayCommand(() => _windows = GetDisplayWindowInfos().ToList());
            RefreshWindowsCommand.Execute(null);
            App.FreeHook = HookWindowEvents();
        }

        private static IEnumerable<DisplayWindowInfo> GetDisplayWindowInfos()
        {
            return WindowInterop.GetTopLevelWindows()
                .Where(NativeMethods.IsWindowVisible)
                .Select(DisplayWindowInfo.FromHWnd)
                .Where(item => item.Info.Window != DisplayWindowInfo.RectZero);
        }

        private IDisposable HookWindowEvents()
        {
            var gcHandle = GCHandle.Alloc(new NativeMethods.HookProc(WndProc));

            var hookId = NativeMethods.SetWindowsHookEx(
                NativeMethods.HookType.WH_MOUSE_LL,
                (NativeMethods.HookProc)gcHandle.Target,
                // ReSharper disable once PossibleNullReferenceException
                Process.GetCurrentProcess().MainModule.BaseAddress,
                0);

            return Disposable.Create(() =>
            {
                NativeMethods.UnhookWindowsHookEx(hookId);
                gcHandle.Free();
            });
        }

        private IntPtr WndProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode != 0)
                return NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);

            switch ((long)wParam)
            {
                case NativeMethods.WM_MOUSEMOVE:
                    MousePosition = WindowInterop.GetMousePosition().ToPoint(0.8);
                    SelectedWindow = GetDisplayWindowInfo(_windows, MousePosition);
                    break;
            }

            return NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private static DisplayWindowInfo GetDisplayWindowInfo(IEnumerable<DisplayWindowInfo> windows, Point point)
        {
            var window = windows.FirstOrDefault(item => item.Info.Window.Contains(point));

            if (window != null && window.Children.Any())
            {
                var temp = window.Children
                    .Where(item => item.Info.Window.Contains(point))
                    .ToList();

                if (temp.Any())
                {
                    window = temp.Select(item => (source: item, area: item.Info.Window.Width * item.Info.Window.Height))
                        .Aggregate((acc, item) => acc.area > item.area ? item : acc)
                        .source;
                }
            }

            return window;
        }
    }
}
