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

namespace _20260420
{
    public class GeoThumb : Thumb
    {


        static GeoThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GeoThumb), new FrameworkPropertyMetadata(typeof(GeoThumb)));
        }

        public GeoThumb()
        {
            Loaded += GeoThumb_Loaded;
            DragDelta += GeoThumb_DragDelta;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("PART_GeoLine") is GeoLine geo)
            {
                MyGeoLine = geo;
            }
            else
            {
                throw new InvalidOperationException("GeoLineが見つからん");
            }
        }

        #region プロパティ

        public GeoLine MyGeoLine
        {
            get { return (GeoLine)GetValue(MyGeoLineProperty); }
            set { SetValue(MyGeoLineProperty, value); }
        }
        public static readonly DependencyProperty MyGeoLineProperty =
            DependencyProperty.Register(nameof(MyGeoLine), typeof(GeoLine), typeof(GeoThumb), new PropertyMetadata(null));
        public GeoLineData MyData
        {
            get { return (GeoLineData)GetValue(MyDataProperty); }
            set { SetValue(MyDataProperty, value); }
        }
        public static readonly DependencyProperty MyDataProperty = DependencyProperty.Register(
                nameof(MyData), typeof(GeoLineData), typeof(GeoThumb), new PropertyMetadata(null, OnMyDataChanged));

        private static void OnMyDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GeoThumb gt && e.NewValue is GeoLineData data)
            {
                //gt.MyGeoLine.UpdateGeometryBounds();
                
            }
        }
        #endregion プロパティ


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
                //MyData.RefreshWidthHeight();
                //MyData.UpdateGeometrySize();
                //InvalidateMeasure(); // 効果なし
                //InvalidateVisual();
            }
            //MyData.UpdatePen();
        }
    }



}
