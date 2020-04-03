using System;
using System.Runtime.InteropServices;
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable CommentTypo

namespace HandyScreenshot.Interop
{
    public static class MonitorNativeMethods
    {
        // size of a device name string
        private const int CCHDEVICENAME = 32;

        internal delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        internal static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        [DllImport("USER32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr hWnd, uint dwFlags);

        [DllImport("USER32")]
        internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("USER32")]
        internal static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("GDI32")]
        internal static extern int GetDeviceCaps(IntPtr hDc, int nIndex);

        [DllImport("SHCORE")]
        internal static extern int GetDpiForMonitor(
            IntPtr hMonitor,
            MonitorDpiType dpiType,
            out uint dpiX,
            out uint dpiY);

        public enum DeviceCap
        {
            /// <summary>
            /// Device driver version
            /// </summary>
            Driverversion = 0,
            /// <summary>
            /// Device classification
            /// </summary>
            Technology = 2,
            /// <summary>
            /// Horizontal size in millimeters
            /// </summary>
            Horzsize = 4,
            /// <summary>
            /// Vertical size in millimeters
            /// </summary>
            Vertsize = 6,
            /// <summary>
            /// Horizontal width in pixels
            /// </summary>
            Horzres = 8,
            /// <summary>
            /// Vertical height in pixels
            /// </summary>
            Vertres = 10,
            /// <summary>
            /// Number of bits per pixel
            /// </summary>
            Bitspixel = 12,
            /// <summary>
            /// Number of planes
            /// </summary>
            Planes = 14,
            /// <summary>
            /// Number of brushes the device has
            /// </summary>
            Numbrushes = 16,
            /// <summary>
            /// Number of pens the device has
            /// </summary>
            Numpens = 18,
            /// <summary>
            /// Number of markers the device has
            /// </summary>
            Nummarkers = 20,
            /// <summary>
            /// Number of fonts the device has
            /// </summary>
            Numfonts = 22,
            /// <summary>
            /// Number of colors the device supports
            /// </summary>
            Numcolors = 24,
            /// <summary>
            /// Size required for device descriptor
            /// </summary>
            Pdevicesize = 26,
            /// <summary>
            /// Curve capabilities
            /// </summary>
            Curvecaps = 28,
            /// <summary>
            /// Line capabilities
            /// </summary>
            Linecaps = 30,
            /// <summary>
            /// Polygonal capabilities
            /// </summary>
            Polygonalcaps = 32,
            /// <summary>
            /// Text capabilities
            /// </summary>
            Textcaps = 34,
            /// <summary>
            /// Clipping capabilities
            /// </summary>
            Clipcaps = 36,
            /// <summary>
            /// Bitblt capabilities
            /// </summary>
            Rastercaps = 38,
            /// <summary>
            /// Length of the X leg
            /// </summary>
            Aspectx = 40,
            /// <summary>
            /// Length of the Y leg
            /// </summary>
            Aspecty = 42,
            /// <summary>
            /// Length of the hypotenuse
            /// </summary>
            Aspectxy = 44,
            /// <summary>
            /// Shading and Blending caps
            /// </summary>
            Shadeblendcaps = 45,

            /// <summary>
            /// Logical pixels inch in X
            /// </summary>
            Logpixelsx = 88,
            /// <summary>
            /// Logical pixels inch in Y
            /// </summary>
            Logpixelsy = 90,

            /// <summary>
            /// Number of entries in physical palette
            /// </summary>
            Sizepalette = 104,
            /// <summary>
            /// Number of reserved entries in palette
            /// </summary>
            Numreserved = 106,
            /// <summary>
            /// Actual color resolution
            /// </summary>
            Colorres = 108,

            // Printing related DeviceCaps. These replace the appropriate Escapes
            /// <summary>
            /// Physical Width in device units
            /// </summary>
            Physicalwidth = 110,
            /// <summary>
            /// Physical Height in device units
            /// </summary>
            Physicalheight = 111,
            /// <summary>
            /// Physical Printable Area x margin
            /// </summary>
            Physicaloffsetx = 112,
            /// <summary>
            /// Physical Printable Area y margin
            /// </summary>
            Physicaloffsety = 113,
            /// <summary>
            /// Scaling factor x
            /// </summary>
            Scalingfactorx = 114,
            /// <summary>
            /// Scaling factor y
            /// </summary>
            Scalingfactory = 115,

            /// <summary>
            /// Current vertical refresh rate of the display device (for displays only) in Hz
            /// </summary>
            Vrefresh = 116,
            /// <summary>
            /// Vertical height of entire desktop in pixels
            /// </summary>
            Desktopvertres = 117,
            /// <summary>
            /// Horizontal width of entire desktop in pixels
            /// </summary>
            Desktophorzres = 118,
            /// <summary>
            /// Preferred blt alignment
            /// </summary>
            Bltalignment = 119
        }

        public enum MonitorDpiType : uint
        {
            MdtDefault = 0,
            MdtEffectiveDpi = 0,
            MdtAngularDpi = 1,
            MdtRawDpi = 2,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MONITORINFOEX
        {
            /// <summary>
            /// The size, in bytes, of the structure. Set this member to sizeof(MONITORINFOEX) (72) before calling the GetMonitorInfo function.
            /// Doing so lets the function determine the type of structure you are passing to it.
            /// </summary>
            public int Size;

            /// <summary>
            /// A RECT structure that specifies the display monitor rectangle, expressed in virtual-screen coordinates.
            /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
            /// </summary>
            public RECT Monitor;

            /// <summary>
            /// A RECT structure that specifies the work area rectangle of the display monitor that can be used by applications,
            /// expressed in virtual-screen coordinates. Windows uses this rectangle to maximize an application on the monitor.
            /// The rest of the area in rcMonitor contains system windows such as the task bar and side bars.
            /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
            /// </summary>
            public RECT WorkArea;

            /// <summary>
            /// The attributes of the display monitor.
            ///
            /// This member can be the following value:
            ///   1 : MONITORINFOF_PRIMARY
            /// </summary>
            public uint Flags;

            /// <summary>
            /// A string that specifies the device name of the monitor being used. Most applications have no use for a display monitor name,
            /// and so can save some bytes by using a MONITORINFO structure.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string DeviceName;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            /// <summary>
            /// The x-coordinate of the upper-left corner of the rectangle.
            /// </summary>
            public int Left;

            /// <summary>
            /// The y-coordinate of the upper-left corner of the rectangle.
            /// </summary>
            public int Top;

            /// <summary>
            /// The x-coordinate of the lower-right corner of the rectangle.
            /// </summary>
            public int Right;

            /// <summary>
            /// The y-coordinate of the lower-right corner of the rectangle.
            /// </summary>
            public int Bottom;
        }
    }
}
