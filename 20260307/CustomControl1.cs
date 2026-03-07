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

namespace _20260307
{
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
