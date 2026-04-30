
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
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

namespace _20260428
{




    [ContentProperty(nameof(MyContent))]
    public class CustomThumb : Thumb
    {
        //// Ctrl+クリック移動後の削除判定用
        //// 移動開始時に自身は選択状態だった場合にtrue
        //private bool isSelectedAtDragStart;

        //// 移動開始時にCtrlキーが押されていたフラグ
        //private bool isDragStartWithPressedCtrl;

        static CustomThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomThumb), new FrameworkPropertyMetadata(typeof(CustomThumb)));
        }
        public CustomThumb()
        {
            DragDelta += CustomThumb_DragDelta;
            PreviewMouseLeftButtonDown += CustomThumb_PreviewMouseLeftButtonDown;
            //Loaded += CustomThumb_Loaded;

            // テスト用右クリックメニュー、図形のOffsetテスト
            ContextMenu menu = new();
            MenuItem item = new() { Header = "test" };
            item.Click += (s, e) =>
            {
                if (MyContent is GeoLineEXforData ex)
                {
                    ex.PointsTopLeftZeroFixWithOffset();
                }
            };
            menu.Items.Add(item);
            this.ContextMenu = menu;
        }

        //private void CustomThumb_Loaded(object sender, RoutedEventArgs e)
        //{
        //    if(MyContent is GeoLineEXforData ex)
        //    {
        //        ex.PointsTopLeftZeroFixWithOffset();
        //    }
        //}

        private void CustomThumb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MyData.RootData?.ClickedItemData = MyData;
        }

        private void CustomThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            MyData.X += e.HorizontalChange;
            MyData.Y += e.VerticalChange;
            e.Handled = true;
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
        #endregion 依存関係プロパティ

        #region パブリックメソッド
        //// たぶん右クリックメニューから実行
        //// 図形Thumb専用、図形にピッタリサイズにする
        //public void PerfectlyFit()
        //{
        //    if (MyContent is ResizeCanvas geot)
        //    {
        //        geot.PerfectlyFit();
        //    }
        //}

        #endregion パブリックメソッド
    }




    public class RootItemsControl : ItemsControl
    {
        static RootItemsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RootItemsControl), new FrameworkPropertyMetadata(typeof(RootItemsControl)));
        }
        public RootItemsControl()
        {
            Loaded += RootItemsControl_Loaded;
        }

        private void RootItemsControl_Loaded(object sender, RoutedEventArgs e)
        {
            //MyData.UpdateBounds();
            //MyData.UpdateSize();
        }

        public RootData MyData
        {
            get { return (RootData)GetValue(MyDataProperty); }
            set { SetValue(MyDataProperty, value); }
        }
        public static readonly DependencyProperty MyDataProperty =
            DependencyProperty.Register(nameof(MyData), typeof(RootData), typeof(RootItemsControl), new PropertyMetadata(null));

        
    }





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
