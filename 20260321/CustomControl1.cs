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

namespace _20260321
{

    [ContentProperty(nameof(MyContent))]
    public class CustomThumb : Thumb
    {
        public override string ToString()
        {
            //return base.ToString();
            return MyData.Name;
        }

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
        #endregion 依存関係プロパティ

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

            // 以下はXAMLのほうで処理するように変更した
            // TextBlockなどサイズが確定していない要素を
            // まっさらなRootに追加した直後にRootのサイズを決定するのに使う
            //Loaded += CustomThumb_Loaded; // これは動くけど、DataTemplateからだとクリックのたびに実行される
            //Initialized += CustomThumb_Initialized;// こっちだとまだ描画されていない感じ
            DragDelta += CustomThumb_DragDelta;
            DragCompleted += CustomThumb_DragCompleted;
        }

        private void CustomThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            MyData.RootData?.UpdateSize();
        }

        private void CustomThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            MyData.X += e.HorizontalChange;
            MyData.Y += e.VerticalChange;
        }

        //// 起動時
        //private void CustomThumb_Loaded(object sender, RoutedEventArgs e)
        //{
        //    // TextBlockなどのサイズがNaNの要素が追加された時用
        //    // Dataが追加された時点で親要素のサイズ計測がされるけど、これらの要素はその時点でのサイズは0で
        //    // 正しいサイズが設定されるのは描画後で、それがここなので、ここで親要素のサイズ計測


        //    if (MyData is TextData text && text.RootData is RootData root)
        //    {
        //        if (text.Width != 0 && root.DataList.Count == 1)
        //        {
        //            root.UpdateSize();
        //        }

        //    }
        //    // 起動時に1回だけ実行されればいいので、ここで解除
        //    // とは言ってもDateTemplateで表示しているとクリックのたびに再作成している？から意味ないかも？
        //    Loaded -= CustomThumb_Loaded;
        //}


        //protected override bool HandlesScrolling
        //{
        //    get
        //    {
        //        //return base.HandlesScrolling;
        //        return true;
        //    }
        //}



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



        static AAAItemsCtrl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AAAItemsCtrl), new FrameworkPropertyMetadata(typeof(AAAItemsCtrl)));
        }
        public AAAItemsCtrl()
        {
          
        }


    }


}