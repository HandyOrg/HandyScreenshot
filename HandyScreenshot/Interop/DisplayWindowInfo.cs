using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace HandyScreenshot.Interop
{
    [DebuggerDisplay("{ClassName}, {Title}")]
    public class DisplayWindowInfo
    {
        public static readonly Rect RectZero = new Rect(0, 0, 0, 0);

        private IReadOnlyList<DisplayWindowInfo> _children;

        public IntPtr Handle { get; set; }

        public string Title { get; set; }

        public string ClassName { get; set; }

        public NativeMethods.ShowWindowCommands ShowCmd { get; set; }

        public WindowInfo Info { get; set; }

        public IReadOnlyList<DisplayWindowInfo> Children => _children ??= GetChildWindows(Handle);

        private static IReadOnlyList<DisplayWindowInfo> GetChildWindows(IntPtr parent)
        {
            return parent.GetChildWindows()
                .Where(NativeMethods.IsWindowVisible)
                .Select(FromHWnd)
                .Where(item => item.Info.Window != RectZero)
                .ToList();
        }

        public static DisplayWindowInfo FromHWnd(IntPtr hWnd)
        {
            return new DisplayWindowInfo
            {
                Title = hWnd.GetWindowTitle(),
                ClassName = hWnd.GetWindowClassName(),
                Info = WindowInfo.FromWindowInfo(hWnd.GetWindowInfo()),
                ShowCmd = hWnd.GetWindowPlacement().ShowCmd,
                Handle = hWnd
            };
        }
    }
}
