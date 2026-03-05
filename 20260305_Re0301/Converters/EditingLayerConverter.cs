using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace _20260305_Re0301.Converters
{
    public class EditingLayerConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var parent = values[0] as GroupData;
            var editingGroup = values[1] as GroupData;

            // 一致で不透明、じゃなければ半透明
            return (parent == editingGroup) ? 1.0 : 0.3;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}