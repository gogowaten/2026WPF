using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace _20260405_GeoLineCanvas
{
    public class GeoCanvas : Canvas
    {
        public CustomThumb MyCustomThumb { get; set; }

        public GeoLineData MyGeoData
        {
            get { return (GeoLineData)GetValue(MyGeoDataProperty); }
            set { SetValue(MyGeoDataProperty, value); }
        }
        public static readonly DependencyProperty MyGeoDataProperty =
            DependencyProperty.Register(nameof(MyGeoData), typeof(GeoLineData), typeof(GeoCanvas), new PropertyMetadata(null));

        public GeoCanvas()
        {
            MyCustomThumb = new CustomThumb();
            Children.Add(MyCustomThumb);

            Loaded += GeoCanvas_Loaded;
        }

        private void GeoCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is GeoLineData data)
            {
                MyGeoData = data;
                MyCustomThumb.MyData = MyGeoData;
            }
        }


    }
}