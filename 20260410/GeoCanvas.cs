using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace _20260410
{
    public class GeoCanvas : Canvas
    {

        public GeoThumb MyGeoThumb
        {
            get { return (GeoThumb)GetValue(MyGeoThumbProperty); }
            set { SetValue(MyGeoThumbProperty, value); }
        }
        public static readonly DependencyProperty MyGeoThumbProperty =
            DependencyProperty.Register(nameof(MyGeoThumb), typeof(GeoThumb), typeof(GeoCanvas), new PropertyMetadata(null));

        public GeoLineData MyData
        {
            get { return (GeoLineData)GetValue(MyDataProperty); }
            set { SetValue(MyDataProperty, value); }
        }
        public static readonly DependencyProperty MyDataProperty =
            DependencyProperty.Register(nameof(MyData), typeof(GeoLineData), typeof(GeoCanvas), new PropertyMetadata(null));

        public GeoCanvas()
        {
            Loaded += GeoCanvas_Loaded;
            //MyGeoThumb = new();
            //Children.Add(MyGeoThumb);
        }

        private void GeoCanvas_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is GeoLineData data)
            {
                MyData = data;
            }
        }
    }
}
