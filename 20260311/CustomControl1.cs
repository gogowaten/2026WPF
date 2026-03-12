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

namespace _20260311
{
    [ContentProperty(nameof(MyContent))]
    public class CustomThumb : Thumb
    {

        static CustomThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomThumb), new FrameworkPropertyMetadata(typeof(CustomThumb)));
        }


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


        //public bool MyIsSelected
        //{
        //    get { return (bool)GetValue(MyIsSelectedProperty); }
        //    set { SetValue(MyIsSelectedProperty, value); }
        //}
        //public static readonly DependencyProperty MyIsSelectedProperty =
        //    DependencyProperty.Register(nameof(MyIsSelected), typeof(bool), typeof(CustomThumb), new PropertyMetadata(false));



        public CustomThumb()
        {
            //this.DataContext = this;
            DragDelta += TThumb_DragDelta;
            PreviewMouseLeftButtonDown += CustomThumb_PreviewMouseLeftButtonDown;
        }

        //protected override void OnKeyDown(KeyEventArgs e)
        //{
        //    base.OnKeyDown(e);
        //    var imakey = e.Key;
        //}
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.F2)
            {
                if(MyData is GroupData group && group.IsCurrent) { group.RootData?.ChangeEditingGroup(group); }
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            var sou = e.Source;
            var ori = e.OriginalSource;
            var dc = this.DataContext;
            var myd = MyData;

            this.Focus();
            var isfo = this.IsFocused;
            var iskeyfo = this.IsKeyboardFocused;

            // ClickedItemの更新
            if(e.OriginalSource is FrameworkElement elm && elm.DataContext is Data data)
            {
                data.RootData?.ClickedItem = data;
            }

            if (MyData.IsSelectable) { MyData.RootData?.AddSelect(MyData); }
        }
        private void CustomThumb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Groupの中の要素の場合は、先にGroupのクリックが来た後に要素のクリックが来る
            var sou = e.Source;
            var ori = e.OriginalSource;
            var dc = this.DataContext;
            var myd = MyData;
            this.Focus();
            var isfo = this.IsFocused;
            var iskeyfo = this.IsKeyboardFocused;
            var iii = this.Focusable;
            
        }

        private void TThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var left = Canvas.GetLeft(this);
            Canvas.SetLeft(this, Canvas.GetLeft(this) + e.HorizontalChange);
            Canvas.SetTop(this, Canvas.GetTop(this) + e.VerticalChange);
            var left2 = Canvas.GetLeft(this);
            //if (MyIsSelected)
            //{
            //}
            e.Handled = true;
        }
    }









    public class AAAItemsCtrl : ItemsControl
    {

        public RootData MyRootData
        {
            get { return (RootData)GetValue(MyRootDataProperty); }
            set { SetValue(MyRootDataProperty, value); }
        }
        public static readonly DependencyProperty MyRootDataProperty =
            DependencyProperty.Register(nameof(MyRootData), typeof(RootData), typeof(AAAItemsCtrl), new PropertyMetadata(null));


        //public DataService AAADataService
        //{
        //    get { return (DataService)GetValue(AAADataServiceProperty); }
        //    set { SetValue(AAADataServiceProperty, value); }
        //}
        //public static readonly DependencyProperty AAADataServiceProperty =
        //    DependencyProperty.Register(nameof(AAADataService), typeof(DataService), typeof(AAAItemsCtrl));


        static AAAItemsCtrl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AAAItemsCtrl), new FrameworkPropertyMetadata(typeof(AAAItemsCtrl)));
        }
        public AAAItemsCtrl()
        {
            PreviewMouseLeftButtonDown += AAAItemsCtrl_PreviewMouseLeftButtonDown;
        }

        private void AAAItemsCtrl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var sou = e.Source;
            var ori = e.OriginalSource;

        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            var sou = e.Source;
            var ori = e.OriginalSource;
            var dc = this.DataContext;
            var root = MyRootData;

            if (ori is FrameworkElement elem && elem.DataContext is Data oo)
            {
                var neko = oo;
            }

            if (ori is UIElement elm)
            {
                // 要素からコンテナ取得？
                var youso = ContainerFromElement(elm);
                var youso2 = ContainerFromElement(this, elm);
                var item = ItemsControl.ItemsControlFromItemContainer(youso);
                var yo = ItemsControl.GetItemsOwner(youso);
                var io = ItemsControl.GetItemsOwner(item);
            }
        }
    }


}
