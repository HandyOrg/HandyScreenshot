using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace HandyScreenshot
{
    public static class WindowInterop
    {
        public static readonly NativeMethods.RECT RectZero = new NativeMethods.RECT();
        public static readonly NativeMethods.POINT PointZero = new NativeMethods.POINT();

        public static NativeMethods.WINDOWPLACEMENT GetWindowPlacement(this IntPtr hWnd)
        {
            var result = NativeMethods.WINDOWPLACEMENT.Default;
            NativeMethods.GetWindowPlacement(hWnd, ref result);
            return result;
        }

        public static NativeMethods.WINDOWINFO GetWindowInfo(this IntPtr hWnd)
        {
            var info = new NativeMethods.WINDOWINFO { cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.WINDOWINFO)) };
            return NativeMethods.GetWindowInfo(hWnd, ref info) ? info : default;
        }

        public static IEnumerable<IntPtr> GetTopLevelWindows()
        {
            var result = new List<IntPtr>();
            var param = new IntPtr(1);
            NativeMethods.EnumWindows((hWnd, lParam) =>
            {
                if (lParam == param)
                {
                    result.Add(hWnd);
                }

                return true;
            }, param);

            return result;
        }

        public static NativeMethods.POINT GetMousePosition()
        {
            var position = new NativeMethods.POINT();
            NativeMethods.GetCursorPos(ref position);
            return position;
        }



        // Extension Methods

        public static IEnumerable<IntPtr> GetChildWindows(this IntPtr parent)
        {
            var result = new List<IntPtr>();

            if (NativeMethods.IsWindow(parent))
            {
                NativeMethods.EnumChildWindows(parent, (hWnd, param) =>
                {
                    if (param == parent &&
                        hWnd != IntPtr.Zero)
                    {
                        result.Add(hWnd);
                    }

                    return true;
                }, parent);
            }

            return result;
        }

        public static NativeMethods.RECT GetWindowRect(this IntPtr hWnd)
        {
            return NativeMethods.GetWindowRect(hWnd, out var rect)
                ? rect
                : RectZero;
        }

        public static string GetWindowClassName(this IntPtr handle)
        {
            var lpClassName = new StringBuilder(256);
            return NativeMethods.GetClassName(handle, lpClassName, lpClassName.Capacity) == 0
                ? null
                : lpClassName.ToString();
        }

        public static string GetWindowTitle(this IntPtr hWnd)
        {
            var lpString = new StringBuilder(256);
            return NativeMethods.GetWindowText(hWnd, lpString, lpString.Capacity) == 0
                ? null
                : lpString.ToString();
        }
    }
}
