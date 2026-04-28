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

namespace _20260428_GeoLineEx
{

    public class FlatHandle : Thumb
    {
        static FlatHandle()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FlatHandle), new FrameworkPropertyMetadata(typeof(FlatHandle)));
        }
        public FlatHandle()
        {

        }


        public Brush MyFillBrush
        {
            get { return (Brush)GetValue(MyFillBrushProperty); }
            set { SetValue(MyFillBrushProperty, value); }
        }
        public static readonly DependencyProperty MyFillBrushProperty =
            DependencyProperty.Register(nameof(MyFillBrush), typeof(Brush), typeof(FlatHandle), new PropertyMetadata(Brushes.Transparent));

        public double MyLeft
        {
            get { return (double)GetValue(MyLeftProperty); }
            set { SetValue(MyLeftProperty, value); }
        }
        public static readonly DependencyProperty MyLeftProperty =
            DependencyProperty.Register(nameof(MyLeft), typeof(double), typeof(FlatHandle), new PropertyMetadata(0.0));

        public double MyTop
        {
            get { return (double)GetValue(MyTopProperty); }
            set { SetValue(MyTopProperty, value); }
        }
        public static readonly DependencyProperty MyTopProperty =
            DependencyProperty.Register(nameof(MyTop), typeof(double), typeof(FlatHandle), new PropertyMetadata(0.0));



    }


}
