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

namespace _20260409
{
    public class DataThumb : Thumb
    {
        static DataThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DataThumb), new FrameworkPropertyMetadata(typeof(DataThumb)));
        }
        public DataThumb()
        {

        }
    }





    public class GeoThumb : Thumb
    {

        public GeoLineData MyData
        {
            get { return (GeoLineData)GetValue(MyDataProperty); }
            set { SetValue(MyDataProperty, value); }
        }
        public static readonly DependencyProperty MyDataProperty =
            DependencyProperty.Register(nameof(MyData), typeof(GeoLineData), typeof(GeoThumb), new PropertyMetadata(null));

        static GeoThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GeoThumb), new FrameworkPropertyMetadata(typeof(GeoThumb)));
        }

        public GeoThumb()
        {
            Loaded += GeoThumb_Loaded;
            DragDelta += GeoThumb_DragDelta;
        }

        private void GeoThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (MyData is not null)
            {
                MyData.InternalX += e.HorizontalChange;
                MyData.InternalY += e.VerticalChange;
                e.Handled = true;
            }
        }

        private void GeoThumb_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is GeoLineData data)
            {
                MyData = data;
                //MyData.ParentPath = this;
                MyData.UpdatePen();
            }
        }
    }
}
