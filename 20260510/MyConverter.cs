using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace _20260510
{

    public class MyConvGroupWakuColor : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var normal = (SolidColorBrush)values[1];
            var edit = (SolidColorBrush)values[2];
            if (values[0] == DependencyProperty.UnsetValue)
            {
                return Brushes.Transparent;
            }
            var isEditing = (bool)values[0];
            if (isEditing) { return edit; }
            else { return normal; }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    internal class MyConverter
    {
    }
}
