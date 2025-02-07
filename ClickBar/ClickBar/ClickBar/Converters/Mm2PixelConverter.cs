using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ClickBar.Converters
{
    [ValueConversion(typeof(double), typeof(double))]
    class Mm2PixelConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double mm = System.Convert.ToDouble((decimal)value);

            return WinUtil.Mm2Pixel(mm);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    public static class WinUtil
    {
        public static double Mm2Pixel(double mm)
        {
            const double factor = 96 / 25.4;

            return mm * factor;
        }
        public static decimal Pixel2Mm(double pixel)
        {
            const double factor = 96 / 25.4;

            return Convert.ToDecimal(pixel / factor);
        }

        public static Size Mm2Pixel(Size mm_size)
        {
            return new Size(Mm2Pixel(mm_size.Width), Mm2Pixel(mm_size.Height));
        }
    }
}
