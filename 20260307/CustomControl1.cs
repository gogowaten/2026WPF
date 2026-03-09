using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


// 独自のコンテナに IsSelected プロパティを持たせます。
namespace _20260307
{
    public class MySelectorItem : ContentControl
    {
        // 選択状態を保持する依存関係プロパティ
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(MySelectorItem), new PropertyMetadata(false));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        // 静的コンストラクタで、デフォルトのスタイル（Generic.xaml）を適用
        static MySelectorItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MySelectorItem), new FrameworkPropertyMetadata(typeof(MySelectorItem)));
        }
    }


    public class Waku : ItemsControl
    {
        static Waku()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Waku), new FrameworkPropertyMetadata(typeof(Waku)));
        }
        public Waku()
        {

        }

        // 必要に応じてItemsPanelを変更
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            //return base.IsItemItsOwnContainerOverride(item);
            return item is UIElement;
        }
        protected override DependencyObject GetContainerForItemOverride()
        {
            //return base.GetContainerForItemOverride();
            // 各アイテムをContentPresenterでラップ
            return new ContentPresenter();
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
