using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace _20260307
{

    //    Selector 本体を実装する
    //ここで 3 つのメソッドをオーバーライドします。
    public class MySelector : ItemsControl
    {
        //protected override void OnItemContainerStyleChanged(Style oldItemContainerStyle, Style newItemContainerStyle)
        //{
        //    base.OnItemContainerStyleChanged(oldItemContainerStyle, newItemContainerStyle);
        //}
        //protected override void OnItemContainerStyleSelectorChanged(StyleSelector oldItemContainerStyleSelector, StyleSelector newItemContainerStyleSelector)
        //{
        //    base.OnItemContainerStyleSelectorChanged(oldItemContainerStyleSelector, newItemContainerStyleSelector);
        //}
        //protected override bool ShouldApplyItemContainerStyle(DependencyObject container, object item)
        //{
        //    return base.ShouldApplyItemContainerStyle(container, item);
        //}
        //protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        //{
        //    base.ClearContainerForItemOverride(element, item);
        //}

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

        // --- 3. コンテナにデータを流し込み、セットアップする ---
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            if (element is MySelectorItem container)
            {
                // マウスクリックイベントをフックして、選択状態を切り替える例
                container.MouseDown += (s, e) => { UpdateSelection(container); };
            }
        }

        private void UpdateSelection(MySelectorItem selectedContainer)
        {
            // 全アイテムをループして、クリックされたものだけ true にする簡易実装
            foreach (var item in Items)
            {
                var container = ItemContainerGenerator.ContainerFromItem(item) as MySelectorItem;
                if (container is not null)
                {
                    container.IsSelected = (container == selectedContainer);
                }
            }
        }

    }
}
