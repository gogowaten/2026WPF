using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace _20260403
{
    public static class SizeObserver
    {

        // サイズ変更監視のオンオフ切り替え
        public static readonly DependencyProperty ObserveProperty =
            DependencyProperty.RegisterAttached("Observe", typeof(bool), typeof(SizeObserver), new FrameworkPropertyMetadata(OnObserveChanged));
        public static bool GetObserve(DependencyObject obj) => (bool)obj.GetValue(ObserveProperty);
        public static void SetObserve(DependencyObject obj, bool value) => obj.SetValue(ObserveProperty, value);

        // サイズ変更イベントの購読と処理の付け外し
        private static void OnObserveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement fe)
            {
                if ((bool)e.NewValue)
                {
                    fe.SizeChanged += Fe_SizeChanged;
                }
                else
                {
                    fe.SizeChanged -= Fe_SizeChanged;
                }
            }
           
        }


        // サイズ変更時の処理、新たなサイズをObservedWidthとHeightそれぞれに記録
        private static void Fe_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is FrameworkElement fe)
            {
                SetObservedWidth(fe, e.NewSize.Width);
                SetObservedHeight(fe, e.NewSize.Height);
                //SetObservedHeight(fe, fe.DesiredSize.Height); // これで問題ない？

            }
            if (sender is GeoLine geo)
            {
                SetObservedWidthRender(geo, geo.MyActualSize.Width);
                SetObservedHeightRender(geo, geo.MyActualSize.Height);
            }
        }


        // HeightRender
        public static readonly DependencyProperty ObservedHeightRenderProperty =
            DependencyProperty.RegisterAttached("ObservedHeightRender", typeof(double), typeof(SizeObserver), new PropertyMetadata(0.0));
        public static double GetObservedHeightRender(DependencyObject obj) => (double)obj.GetValue(ObservedHeightRenderProperty);
        public static void SetObservedHeightRender(DependencyObject obj, double value) => obj.SetValue(ObservedHeightRenderProperty, value);


        // _widthRender
        public static readonly DependencyProperty ObservedWidthRenderProperty =
            DependencyProperty.RegisterAttached("ObservedWidthRender", typeof(double), typeof(SizeObserver), new PropertyMetadata(0.0));
        public static double GetObservedWidthRender(DependencyObject obj) => (double)obj.GetValue(ObservedWidthRenderProperty);
        public static void SetObservedWidthRender(DependencyObject obj, double value) => obj.SetValue(ObservedWidthRenderProperty, value);

        // Width記録用
        public static readonly DependencyProperty ObservedWidthProperty =
            DependencyProperty.RegisterAttached("ObservedWidth", typeof(double), typeof(SizeObserver), new PropertyMetadata(0.0));
        public static double GetObservedWidth(DependencyObject obj) => (double)obj.GetValue(ObservedWidthProperty);
        public static void SetObservedWidth(DependencyObject obj, double value) => obj.SetValue(ObservedWidthProperty, value);

        // Height記録用
        public static readonly DependencyProperty ObservedHeightProperty =
            DependencyProperty.RegisterAttached("ObservedHeight", typeof(double), typeof(SizeObserver), new PropertyMetadata(0.0));
        public static double GetObservedHeight(DependencyObject obj) => (double)obj.GetValue(ObservedHeightProperty);
        public static void SetObservedHeight(DependencyObject obj, double value) => obj.SetValue(ObservedHeightProperty, value);

    }



    public static class AttachedProperty
    {
    }
}
