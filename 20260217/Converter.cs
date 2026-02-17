using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace _20260217
{
    public class MyConvDT : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            decimal dd = (decimal)values[0];
            int ds = (int)values[1];
            return dd.ToString("F" + ds);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            // 1.23
            string ss = (string)value;
            object[] ds = new object[2];
            ds[0] = decimal.Parse(ss);
            ds[1] = 3;
            return ds;
        }
    }

    public class MyConvDecimalText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal dd = (decimal)value;
            return dd;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string ss = (string)value;
            return ss;
        }
    }


}
