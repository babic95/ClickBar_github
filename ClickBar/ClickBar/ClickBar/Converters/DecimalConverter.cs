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
    public class Adding : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal A = System.Convert.ToDecimal(value[0]);
            decimal B = System.Convert.ToDecimal(value[1]);

            return string.Format("{0:#,##0.00}", Decimal.Round(System.Convert.ToDecimal(A + B), 2)).Replace(',', '#').Replace('.', ',').Replace('#', '.');
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class Subtracting : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal A = System.Convert.ToDecimal(value[0]);
            decimal B = System.Convert.ToDecimal(value[1]);
            return string.Format("{0:#,##0.00}", Decimal.Round(System.Convert.ToDecimal(A - B), 2)).Replace(',', '#').Replace('.', ',').Replace('#', '.');
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class Multiplication : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal A = System.Convert.ToDecimal(value[0]);
            decimal B = System.Convert.ToDecimal(value[1]);
            return string.Format("{0:#,##0.00}", Decimal.Round(System.Convert.ToDecimal(A * B), 2)).Replace(',', '#').Replace('.', ',').Replace('#', '.');
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class Dividing : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal A = System.Convert.ToDecimal(value[0]);
            decimal B = System.Convert.ToDecimal(value[1]);
            return string.Format("{0:#,##0.00}", Decimal.Round(System.Convert.ToDecimal(A / B), 2)).Replace(',', '#').Replace('.', ',').Replace('#', '.');
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class VrednostPDV : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal Value = System.Convert.ToDecimal(value[0]);
            decimal StopaPDV = System.Convert.ToDecimal(value[1]);
            return string.Format("{0:#,##0.00}", Decimal.Round(System.Convert.ToDecimal(Value - (Value * 100 / (StopaPDV + 100))), 2)).Replace(',', '#').Replace('.', ',').Replace('#', '.');
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class OdbijanjePDV : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal Value = System.Convert.ToDecimal(value[0]);
            decimal StopaPDV = System.Convert.ToDecimal(value[1]);
            return string.Format("{0:#,##0.00}", Decimal.Round(System.Convert.ToDecimal(Value * 100 / (StopaPDV + 100)), 2)).Replace(',', '#').Replace('.', ',').Replace('#', '.');
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class DodavanjePDV : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal Value = System.Convert.ToDecimal(value[0]);
            decimal StopaPDV = System.Convert.ToDecimal(value[1]);
            return string.Format("{0:#,##0.00}", Decimal.Round(System.Convert.ToDecimal(Value * (1 + StopaPDV / 100)), 2)).Replace(',', '#').Replace('.', ',').Replace('#', '.');
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class NivelacijaPDV : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal OldPrice = System.Convert.ToDecimal(value[0]);
            decimal NewPrice = System.Convert.ToDecimal(value[1]);
            decimal StopaPDV = System.Convert.ToDecimal(value[2]);

            decimal oldPDV = OldPrice - (OldPrice * 100 / (StopaPDV + 100));
            decimal newPDV = NewPrice - (NewPrice * 100 / (StopaPDV + 100));
            return string.Format("{0:#,##0.00}", Decimal.Round(oldPDV - newPDV, 2)).Replace(',', '#').Replace('.', ',').Replace('#', '.');
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class DecimalToString : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format("{0:#,##0.00}", Decimal.Round(System.Convert.ToDecimal(value[0]), 2)).Replace(',', '#').Replace('.', ',').Replace('#', '.');
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class DateToString : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToDateTime(value[0]).ToString("dd.MM.yyyy HH:mm:ss");
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class DateToStringSingle : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToDateTime(value).ToString("dd.MM.yyyy HH:mm:ss");
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class DecimalNumberToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format("{0:#,##0.00}", Decimal.Round(System.Convert.ToDecimal(value), 2)).Replace(',', '#').Replace('.', ',').Replace('#', '.');
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class DecimalNumberQuantityToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format("{0:#,##0.000}", Decimal.Round(System.Convert.ToDecimal(value), 3)).Replace(',', '#').Replace('.', ',').Replace('#', '.');
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class FractionHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double actualHeight && double.TryParse(parameter?.ToString(), out double fraction))
            {
                return actualHeight * fraction;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
