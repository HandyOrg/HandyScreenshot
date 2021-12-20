using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using HandyScreenshot.Common;
using static HandyScreenshot.Interop.NativeMethods;

namespace HandyScreenshot.Helpers
{
    public static class Extensions
    {
        public static ReadOnlyRect ToReadOnlyRect(this RECT rect)
        {
            return (rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
        }

        public static ReadOnlyRect ToReadOnlyRect(this RectProxy rect)
        {
            return (rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static void Using(this DrawingVisual self, Action<DrawingContext> callback)
        {
            var dc = self.RenderOpen();
            callback(dc);
            dc.Close();
        }

        public static void UpdateDependencyProperty<T, TU>(
            this DependencyObject d,
            DependencyPropertyChangedEventArgs e,
            Action<T, TU> loadNewValue,
            Action<T, TU>? unloadOldValue = null
            )
        {
            if (d is T t)
            {
                if (e.OldValue is TU oldValue)
                {
                    unloadOldValue?.Invoke(t, oldValue);
                }

                if (e.NewValue is TU newValue)
                {
                    loadNewValue(t, newValue);
                }
            }
        }

        public static IntPtr GetHandle(this Window window)
        {
            return new WindowInteropHelper(window).EnsureHandle();
        }
    }
}
