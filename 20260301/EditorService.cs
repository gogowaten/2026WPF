using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;


namespace _20260301
{
    public static class EditorBehavior
    {
        // CanvasElementControlなど、クリック対象にセットする添付プロパティ
        public static readonly DependencyProperty IsSelectableProperty =
            DependencyProperty.RegisterAttached("IsSelectable", typeof(bool), typeof(EditorBehavior), new PropertyMetadata(false));

        public static bool GetIsSelectable(DependencyObject obj) => (bool)obj.GetValue(IsSelectableProperty);

        public static void SetIsSelectable(DependencyObject obj, bool value) => obj.SetValue(IsSelectableProperty, value);

    }



    public class EditorService
    {
        // 選択要素のCollection
        public List<Data> SelectedItems { get; } = [];

        // アクティブ(最後に選択された)要素
        public Data? ActiveItem { get; private set; }

        public void Select(Data target, bool isControlPressed)
        {
            if (!isControlPressed)
            {
                // Ctrlが押されていなければ、すべての選択を解除
                ClearSelection();

                // 目標を選択状態にする
                target.IsSelected = true;
                SelectedItems.Add(target);
            }
            else
            {
                // Ctrlが押されていないときは、フラグをトグル切り替え
                target.IsSelected = !target.IsSelected;

                // フラグに従ってCollectionを更新
                if (target.IsSelected)
                { SelectedItems.Add(target); }
                else
                { _ = SelectedItems.Remove(target); }
            }
        }

        // 全選択解除
        public void ClearSelection()
        {
            foreach (var item in SelectedItems)
            {
                item.IsSelected = false;
            }
            SelectedItems.Clear();
            ActiveItem = null;
        }
    }
}
