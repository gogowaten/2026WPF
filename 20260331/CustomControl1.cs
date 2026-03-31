using CommunityToolkit.Mvvm.Input;
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

namespace _20260331
{

    [ContentProperty(nameof(MyContent))]
    public class CustomThumb : Thumb
    {
        static CustomThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomThumb), new FrameworkPropertyMetadata(typeof(CustomThumb)));
        }


        public object MyContent
        {
            get { return (object)GetValue(MyContentProperty); }
            set { SetValue(MyContentProperty, value); }
        }
        public static readonly DependencyProperty MyContentProperty =
            DependencyProperty.Register(nameof(MyContent), typeof(object), typeof(CustomThumb), new PropertyMetadata(null));



    }



    public partial class DataThumb : Thumb
    {

        static DataThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DataThumb), new FrameworkPropertyMetadata(typeof(DataThumb)));
        }



        public Data ThumbData
        {
            get { return (Data)GetValue(ThumbDataProperty); }
            set { SetValue(ThumbDataProperty, value); }
        }
        public static readonly DependencyProperty ThumbDataProperty =
            DependencyProperty.Register(nameof(ThumbData), typeof(Data), typeof(DataThumb), new FrameworkPropertyMetadata(null, OnThumbDataChanged));
        private static void OnThumbDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataThumb thumb)
            {
                thumb.ChangeGeoShepeRenderOffsetCommand.NotifyCanExecuteChanged();
            }
        }


        public DataThumb()
        {

            DragDelta += OnDragDelta;
            Loaded += DataThumb_Loaded;
        }

        private void DataThumb_Loaded(object sender, RoutedEventArgs e)
        {
            var neko = DataContext;
            var data = ThumbData;
        }

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (DataContext is Data data)
            {
                data.X += e.HorizontalChange;
                data.Y += e.VerticalChange;
            }
        }

        /// <summary>
        /// オフセットの切り替え、GeoShapeData専用
        /// 図形の位置が左上(0,0)になるのと、通常の位置の切り替えになる
        /// 図形の位置が変わるののでThumbのいちも相対的に変更するため、DataのX,Yを変更している
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanChangeGeoShepeRnderOffset))]
        private void ChangeGeoShepeRenderOffset()
        {
            if (ThumbData is GeoShapeData data)
            {
                data.IsOffset = !data.IsOffset;

                //if (data.IsOffset)
                //{
                //    data.IsOffset = false;
                //    data.X -= data.OriginBounds.X;
                //    data.Y -= data.OriginBounds.Y;
                //}
                //else
                //{
                //    data.IsOffset = true;
                //    data.X += data.OriginBounds.X;
                //    data.Y += data.OriginBounds.Y;
                //}

            }

        }

        private bool CanChangeGeoShepeRnderOffset()
        {
            return ThumbData is GeoShapeData;
        }

    }


    public class CustomControl1 : Control
    {
        static CustomControl1()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomControl1), new FrameworkPropertyMetadata(typeof(CustomControl1)));
        }
    }
}