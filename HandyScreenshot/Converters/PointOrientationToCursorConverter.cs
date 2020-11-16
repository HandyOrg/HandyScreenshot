using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;
using HandyScreenshot.Common;

namespace HandyScreenshot.Converters
{
    public class PointOrientationToCursorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is PointOrientation orientation)) return Binding.DoNothing;

            return orientation switch
            {
                PointOrientation.Left => Cursors.SizeWE,
                PointOrientation.Right => Cursors.SizeWE,
                PointOrientation.Top => Cursors.SizeNS,
                PointOrientation.Bottom => Cursors.SizeNS,
                PointOrientation.Left | PointOrientation.Top => Cursors.SizeNWSE,
                PointOrientation.Right | PointOrientation.Bottom => Cursors.SizeNWSE,
                PointOrientation.Right | PointOrientation.Top => Cursors.SizeNESW,
                PointOrientation.Left | PointOrientation.Bottom => Cursors.SizeNESW,
                _ => Cursors.SizeAll
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
