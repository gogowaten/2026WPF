using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace _20260311
{
    //public class GeoCanvas : Canvas
    //{
    //    public CustomThumbForInternal MyCustomThumb { get; set; }

    //    public GeoLineData2 MyGeoData
    //    {
    //        get { return (GeoLineData2)GetValue(MyGeoDataProperty); }
    //        set { SetValue(MyGeoDataProperty, value); }
    //    }
    //    public static readonly DependencyProperty MyGeoDataProperty =
    //        DependencyProperty.Register(nameof(MyGeoData), typeof(GeoLineData2), typeof(GeoCanvas), new PropertyMetadata(null));

    //    public GeoCanvas()
    //    {
    //        MyCustomThumb = new CustomThumbForInternal();
    //        Children.Add(MyCustomThumb);

    //        Loaded += GeoCanvas_Loaded;
    //    }

    //    private void GeoCanvas_Loaded(object sender, RoutedEventArgs e)
    //    {
    //        if (DataContext is GeoLineData2 data)
    //        {
    //            MyGeoData = data;
    //            MyCustomThumb.MyData = MyGeoData;
    //        }
    //    }


    //}
}