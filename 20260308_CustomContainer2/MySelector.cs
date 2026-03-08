using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Runtime.CompilerServices;

namespace _20260308_CustomContainer2
{
    public class MySelector : ItemsControl
    {

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(MySelector), new FrameworkPropertyMetadata(null, OnSelectedItemChanged));

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MySelector selector && e.NewValue is UIElement element)
            {
                selector.UpdateVisualSelection(element);
            }
        }

        private void UpdateVisualSelection(object selectedItem)
        {
            foreach (var item in Items)
            {
                if (ItemContainerGenerator.ContainerFromItem(item) is MySelectorItem container)
                {
                    container.IsSelected = (item == selectedItem);
                }
            }
        }


        // --- 1. アイテムが既にコンテナかチェック ---
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            //return base.IsItemItsOwnContainerOverride(item);
            return item is MySelectorItem;
        }

        // --- 2. 新しいコンテナ（器）を作成 ---
        protected override DependencyObject GetContainerForItemOverride()
        {
            //return base.GetContainerForItemOverride();
            return new MySelectorItem();
        }





        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            if (element is MySelectorItem container)
            {
                container.IsSelected = (item == SelectedItem);
                container.PreviewMouseDown += OnContainerMouseDown;
            }
        }

        private void OnContainerMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is MySelectorItem container)
            {
                SelectedItem = ItemContainerGenerator.ItemFromContainer(container);
            }
        }

        // コンテナの後処理、購読解除と状態のリセット
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            if (element is MySelectorItem container)
            {
                container.PreviewMouseDown -= OnContainerMouseDown;
                container.IsSelected = false;
            }

            base.ClearContainerForItemOverride(element, item);
        }
    }
}
