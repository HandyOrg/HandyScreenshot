using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace HandyScreenshot.Interop
{
    public static class Win32Helper
    {
        public static IDisposable HookMouseMoveEvent(Action<NativeMethods.POINT> action)
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

            IntPtr WndProc(int nCode, IntPtr wParam, IntPtr lParam)
            {
                if (nCode != 0)
                    return NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);

                switch ((long)wParam)
                {
                    case NativeMethods.WM_MOUSEMOVE:
                        action(GetMousePosition());
                        break;
                }

                return NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
            }
        }

        public static NativeMethods.POINT GetMousePosition()
        {
            var position = new NativeMethods.POINT();
            NativeMethods.GetCursorPos(ref position);
            return position;
        }

        public static Point ToPoint(this NativeMethods.POINT point, double scale = 1)
        {
            return new Point(point.X * scale, point.Y * scale);
        }

        public static Rect Scale(this Rect rect, double scale)
        {
            return new Rect(rect.Left * scale, rect.Top * scale, rect.Width * scale, rect.Height * scale);
        }
    }
}
