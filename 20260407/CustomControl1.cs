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

namespace _20260407
{
    public class DataThumb : Thumb
    {
        static DataThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DataThumb), new FrameworkPropertyMetadata(typeof(DataThumb)));
        }


        public GeoLineData MyGeoData
        {
            get { return (GeoLineData)GetValue(MyGeoDataProperty); }
            set { SetValue(MyGeoDataProperty, value); }
        }
        public static readonly DependencyProperty MyGeoDataProperty =
            DependencyProperty.Register(nameof(MyGeoData), typeof(GeoLineData), typeof(DataThumb), new PropertyMetadata(null));



        public DataThumb()
        {
            Loaded += (s, e) => { if (DataContext is GeoLineData data) { MyGeoData = data; } };
        }

    }





    public class InternalThumb : Thumb
    {
        static InternalThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(InternalThumb), new FrameworkPropertyMetadata(typeof(InternalThumb)));
        }

        #region 依存関係プロパティ

        public GeoLineData MyGeoData
        {
            get { return (GeoLineData)GetValue(MyGeoDataProperty); }
            set { SetValue(MyGeoDataProperty, value); }
        }
        public static readonly DependencyProperty MyGeoDataProperty =
            DependencyProperty.Register(nameof(MyGeoData), typeof(GeoLineData), typeof(InternalThumb), new PropertyMetadata(null));



        // ドラッグ移動切り替え
        public bool IsCanDragMove
        {
            get { return (bool)GetValue(IsCanDragMoveProperty); }
            set { SetValue(IsCanDragMoveProperty, value); }
        }
        public static readonly DependencyProperty IsCanDragMoveProperty =
            DependencyProperty.Register(nameof(IsCanDragMove), typeof(bool), typeof(InternalThumb), new FrameworkPropertyMetadata(false, OnMyIsDragMoveChanged));

        // DragDeltaの付け外し
        private static void OnMyIsDragMoveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InternalThumb thumb)
            {
                if (e.NewValue is bool isMove && isMove)
                {
                    if (double.IsNaN(Canvas.GetLeft(thumb)))
                    {
                        Canvas.SetLeft(thumb, 0);
                        Canvas.SetTop(thumb, 0);
                    }
                    thumb.DragDelta += Thumb_DragDelta;
                    thumb.DragCompleted += Thumb_DragCompleted;
                }
                else
                {
                    thumb.DragDelta -= Thumb_DragDelta;
                    thumb.DragCompleted -= Thumb_DragCompleted;
                }
            }
        }


        #endregion 依存関係プロパティ

        public InternalThumb()
        {
            Loaded += GeoThumb_Loaded;
        }

        private void GeoThumb_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is GeoLineData data)
            {
                MyGeoData = data;
            }
        }



        #region ドラッグ移動


        private static void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is InternalThumb thumb)
            {
                thumb.MyGeoData.InternalX += e.HorizontalChange;
                thumb.MyGeoData.InternalY += e.VerticalChange;
                e.Handled = true;
            }

        }

        private static void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (sender is InternalThumb thumb)
            {
                // 移動完了時、DataのWidthとheightを更新する
                // この値はParentのThumbのサイズとバインドしている
                var data = thumb.MyGeoData;
                data.Width = data.BoundsWidth + data.InternalX;
                data.Height = data.BoundsHeight + data.InternalY;

                if (data.X > 0) { data.Width -= data.X; }
                if (data.Y > 0) { data.Height -= data.Y; }


                // 移動後座標がマイナスになったときは、0にしたいので、
                // その分ParentThumbを逆に移動させる
                if (data.InternalX < 0)
                {
                    data.Width = data.BoundsWidth; // ParentThumbのサイズは図形と同じになるはず
                    data.X += data.InternalX; // ParentThumbを逆側に移動させてから
                    data.InternalX = 0; // 自身の座標
                                        //data.X = 0; // ParentThumbがマイナス座標になったときの処理はRootで行うはず
                }
                if (data.InternalY < 0)
                {
                    data.Height = data.BoundsHeight;
                    data.Y += data.InternalY;
                    data.InternalY = 0;
                }
            }
        }

        #endregion ドラッグ移動



    }


    //public class CustomThumb : Thumb
    //{
    //    static CustomThumb()
    //    {
    //        DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomThumb), new FrameworkPropertyMetadata(typeof(CustomThumb)));
    //    }

    //    #region 依存関係プロパティ

    //    public Data MyData
    //    {
    //        get { return (Data)GetValue(MyDataProperty); }
    //        set { SetValue(MyDataProperty, value); }
    //    }
    //    public static readonly DependencyProperty MyDataProperty =
    //        DependencyProperty.Register(nameof(MyData), typeof(Data), typeof(CustomThumb), new PropertyMetadata(null));

    //    public FrameworkElement MyContent
    //    {
    //        get { return (FrameworkElement)GetValue(MyContentProperty); }
    //        set { SetValue(MyContentProperty, value); }
    //    }
    //    public static readonly DependencyProperty MyContentProperty =
    //        DependencyProperty.Register(nameof(MyContent), typeof(FrameworkElement), typeof(CustomThumb), new PropertyMetadata(null));



    //    #endregion 依存関係プロパティ

    //    // コンストラクタ
    //    public CustomThumb()
    //    {
    //        Loaded += CustomThumb_Loaded;
    //    }

    //    private void CustomThumb_Loaded(object sender, RoutedEventArgs e)
    //    {
    //        if (DataContext is GeoLineData data)
    //        {
    //            MyData = data;
    //        }
    //    }





    //    #region ドラッグ移動


    //    private static void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
    //    {
    //        if (sender is CustomThumb thumb)
    //        {
    //            thumb.MyData.X += e.HorizontalChange;
    //            thumb.MyData.Y += e.VerticalChange;
    //            e.Handled = true;
    //        }

    //    }
    //    #endregion ドラッグ移動



    //}


}