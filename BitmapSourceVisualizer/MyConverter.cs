using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace BitmapSourceVisualizer
{
    public class MyConvColorToTextARGB : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color c = (Color)value;
            string argb = $"{c.A}, {c.R}, {c.G}, {c.B}";
            return argb;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    internal class MyConverter
    {
    }
}
