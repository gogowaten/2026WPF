using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;


namespace _20260301
{



    public partial class EditorService : ObservableObject
    {
        // 最後に選択された要素
        [ObservableProperty] private Data? _clickedItem;

        // 現在フォーカスされている「主」要素（1つ）
        [ObservableProperty] private Data? _activeItem;

        // 現在「潜り込んで」編集しているグループ（nullならルート）
        [ObservableProperty] private GroupData? _editingGroup;

        // 選択要素のCollection
        public List<Data> SelectedItems { get; } = [];

        // アクティブ(最後に選択された)要素
        //public Data? ClickedItem { get; private set; }
        


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
            ClickedItem = null;
        }
    }
}
