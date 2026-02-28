using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace _20260227.Converters
{
    public class NegativeConverter : IValueConverter
    {
        // データを画面に表示する時の処理（プラスをマイナスにする）
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                return -d;
            }
            return value;
        }

        // 画面からの入力を戻す時の処理（今回は使わない）
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                return -d;
            }
            return value;
        }
    }


}
