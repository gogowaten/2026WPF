using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20260403
{
    //public class GeoCanvas : Canvas
    //{
    //    static GeoCanvas()
    //    {
    //        DefaultStyleKeyProperty.OverrideMetadata(typeof(GeoCanvas), new FrameworkPropertyMetadata(typeof(GeoCanvas)));
    //    }


    //    public GeoLineData MyGeoData
    //    {
    //        get { return (GeoLineData)GetValue(MyGeoDataProperty); }
    //        set { SetValue(MyGeoDataProperty, value); }
    //    }
    //    public static readonly DependencyProperty MyGeoDataProperty =
    //        DependencyProperty.Register(nameof(MyGeoData), typeof(GeoLineData), typeof(GeoCanvas), new PropertyMetadata(null));


    //    public GeoCanvas()
    //    {
    //        Loaded += GeoCanvas_Loaded;
    //    }

    //    private void GeoCanvas_Loaded(object sender, RoutedEventArgs e)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    [ContentProperty(nameof(MyContent))]
    public class CustomThumb : Thumb
    {
        static CustomThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomThumb), new FrameworkPropertyMetadata(typeof(CustomThumb)));
        }

        #region 依存関係プロパティ

        public Data MyData
        {
            get { return (Data)GetValue(MyDataProperty); }
            set { SetValue(MyDataProperty, value); }
        }
        public static readonly DependencyProperty MyDataProperty =
            DependencyProperty.Register(nameof(MyData), typeof(Data), typeof(CustomThumb), new PropertyMetadata(null));

        public FrameworkElement MyContent
        {
            get { return (FrameworkElement)GetValue(MyContentProperty); }
            set { SetValue(MyContentProperty, value); }
        }
        public static readonly DependencyProperty MyContentProperty =
            DependencyProperty.Register(nameof(MyContent), typeof(FrameworkElement), typeof(CustomThumb), new PropertyMetadata(null));


        public bool IsCanDragMove
        {
            get { return (bool)GetValue(IsCanDragMoveProperty); }
            set { SetValue(IsCanDragMoveProperty, value); }
        }
        public static readonly DependencyProperty IsCanDragMoveProperty =
            DependencyProperty.Register(nameof(IsCanDragMove), typeof(bool), typeof(CustomThumb), new FrameworkPropertyMetadata(false, OnMyIsDragMoveChanged));

        #endregion 依存関係プロパティ

        // コンストラクタ
        public CustomThumb()
        {
            Loaded += CustomThumb_Loaded;
        }

        private void CustomThumb_Loaded(object sender, RoutedEventArgs e)
        {
            if(DataContext is GeoLineData data)
            {
                MyData = data;
            }
        }

      



        #region ドラッグ移動

        private static void OnMyIsDragMoveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CustomThumb thumb)
            {
                if (e.NewValue is bool isMove && isMove)
                {
                    if (double.IsNaN(Canvas.GetLeft(thumb)))
                    {
                        Canvas.SetLeft(thumb, 0);
                        Canvas.SetTop(thumb, 0);
                    }
                    thumb.DragDelta += Thumb_DragDelta;
                }
                else
                {
                    thumb.DragDelta -= Thumb_DragDelta;
                }
            }
        }

        private static void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is CustomThumb thumb)
            {
                Canvas.SetLeft(thumb, Canvas.GetLeft(thumb) + e.HorizontalChange);
                Canvas.SetTop(thumb, Canvas.GetTop(thumb) + e.VerticalChange);
            }
        }
        #endregion ドラッグ移動



    }


}
