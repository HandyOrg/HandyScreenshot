using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using HandyScreenshot.Interop;
using HandyScreenshot.UIAInterop;

namespace HandyScreenshot
{
    public class MainWindowViewModel : BindableBase
    {
        private Point _mousePosition;
        private CachedElement _selectedElement;

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

        public ICommand CloseCommand { get; } = new RelayCommand(() => Application.Current.Shutdown());

        private readonly IReadOnlyList<CachedElement> _elements;

        public MainWindowViewModel()
        {
            _elements = CachedElement.GetChildren(AutomationElement.RootElement);
            App.FreeHook = HookWindowEvents();
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
                    SelectedElement = GetAdjustElement(_elements, MousePosition);
                    break;
            }

            return NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private static CachedElement GetAdjustElement(IReadOnlyList<CachedElement> elements, Point point, int maxDeep = -1)
        {
            return GetAdjustElement(elements, point, 0, maxDeep);
        }

        private static CachedElement GetAdjustElement(IReadOnlyList<CachedElement> elements, Point point, int deep, int maxDeep)
        {
            if (elements == null || !elements.Any()) return null;

            var result = elements
                .FirstOrDefault(item => item.Rect.Contains(point));

            if (result != null && (maxDeep == -1 || deep < maxDeep))
            {
                result = GetAdjustElement(result.Children, point, deep + 1, maxDeep) ?? result;
            }

            return result;
        }
    }
}
