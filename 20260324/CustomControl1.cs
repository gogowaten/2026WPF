using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20260324
{
    public class GeoLineThumb : Thumb
    {

        public GeoLineData MyData
        {
            get { return (GeoLineData)GetValue(MyDataProperty); }
            set { SetValue(MyDataProperty, value); }
        }
        public static readonly DependencyProperty MyDataProperty =
            DependencyProperty.Register(nameof(MyData), typeof(GeoLineData), typeof(GeoLineThumb), new PropertyMetadata(null));

        static GeoLineThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GeoLineThumb), new FrameworkPropertyMetadata(typeof(GeoLineThumb)));
        }
        public GeoLineThumb()
        {
            //this.DataContext = this;
            //this.DataContext = MyData;
        }
    }
}
