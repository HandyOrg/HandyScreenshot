using System.Windows;

namespace HandyScreenshot
{
    public class WindowInfo
    {
        public Rect Window { get; set; }

        public Rect ClientArea { get; set; }

        public WindowStyles WindowStyles { get; set; }

        public WindowStylesEx WindowStylesEx { get; set; }

        public bool IsActiveCaption { get; set; }

        public double WindowBorderWidth { get; set; }

        public double WindowBorderHeight { get; set; }

        public static WindowInfo FromWindowInfo(NativeMethods.WINDOWINFO info)
        {
            return new WindowInfo
            {
                Window = info.rcWindow.ToRect(0.8),
                ClientArea = info.rcClient.ToRect(0.8),
                WindowStyles = (WindowStyles)info.dwStyle,
                WindowStylesEx = (WindowStylesEx)info.dwExStyle,
                IsActiveCaption = info.dwWindowStatus == 0x0001,
                WindowBorderWidth = info.cxWindowBorders * 0.8,
                WindowBorderHeight = info.cyWindowBorders * 0.8
            };
        }
    }
}
