using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace _20260316
{
    public static class SizeObserver
    {

        public static readonly DependencyProperty ObserveProperty =
            DependencyProperty.RegisterAttached("Observe", typeof(bool), typeof(SizeObserver), new FrameworkPropertyMetadata(OnObserveChanged));

        public static bool GetObserve(DependencyObject obj) => (bool)obj.GetValue(ObserveProperty);

        public static void SetObserve(DependencyObject obj, bool value) => obj.SetValue(ObserveProperty, value);


        public static readonly DependencyProperty ObserveWidthProperty =
            DependencyProperty.RegisterAttached("ObserveWidth", typeof(double), typeof(SizeObserver), new PropertyMetadata(0.0));

        public static double GetObserveWidth(DependencyObject obj) => (double)obj.GetValue(ObserveWidthProperty);

        public static void SetObserveWidth(DependencyObject obj, double value) => obj.SetValue(ObserveWidthProperty, value);


        public static readonly DependencyProperty ObserveHeightProperty =
            DependencyProperty.RegisterAttached("ObserveHeight", typeof(double), typeof(SizeObserver), new PropertyMetadata(0.0));

        public static double GetObserveHeight(DependencyObject obj) => (double)obj.GetValue(ObserveHeightProperty);

        public static void SetObserveHeight(DependencyObject obj, double value) => obj.SetValue(ObserveHeightProperty, value);




        private static void OnObserveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement fe && (bool)e.NewValue)
            {
                fe.SizeChanged += (s, args) =>
                {
                    //SetObserveWidth(fe, fe.DesiredSize.Width);
                    SetObserveWidth(fe, fe.ActualWidth);
                    SetObserveHeight(fe, fe.ActualHeight);
                };
            }
        }

    }
}
