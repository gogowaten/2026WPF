using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;

namespace _20260331
{
    public static class BoundsObserver
    {

        public static readonly DependencyProperty ObserveProperty =
            DependencyProperty.RegisterAttached("Observe", typeof(bool), typeof(BoundsObserver), new FrameworkPropertyMetadata(OnObserveChanged));

        public static bool GetObserve(DependencyObject obj) => (bool)obj.GetValue(ObserveProperty);

        public static void SetObserve(DependencyObject obj, bool value) => obj.SetValue(ObserveProperty, value);

        private static void OnObserveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //if (d is FrameworkElement element)
            //{
            //    if ((bool)e.NewValue)
            //    {
            //        element.SizeChanged += Shape_SizeChanged;
            //    }
            //    else
            //    {
            //        element.SizeChanged -= Shape_SizeChanged;
            //    }
            //}
           
        }

      

        private static void Shape_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is GeoLine geo)
            {
                SetObservedWidth(geo, geo.MyGeometryBounds.Width);
                SetObservedHeight(geo, geo.MyGeometryBounds.Height);
            }
        }

        // Width記録用
        public static readonly DependencyProperty ObservedWidthProperty =
            DependencyProperty.RegisterAttached("ObservedWidth", typeof(double), typeof(BoundsObserver), new PropertyMetadata(0.0));
        public static double GetObservedWidth(DependencyObject obj) => (double)obj.GetValue(ObservedWidthProperty);
        public static void SetObservedWidth(DependencyObject obj, double value) => obj.SetValue(ObservedWidthProperty, value);

        // Height記録用
        public static readonly DependencyProperty ObservedHeightProperty =
            DependencyProperty.RegisterAttached("ObservedHeight", typeof(double), typeof(BoundsObserver), new PropertyMetadata(0.0));
        public static double GetObservedHeight(DependencyObject obj) => (double)obj.GetValue(ObservedHeightProperty);
        public static void SetObservedHeight(DependencyObject obj, double value) => obj.SetValue(ObservedHeightProperty, value);


    }
}
