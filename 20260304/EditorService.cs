using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace _20260304
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


        // ActiveItemが変更されたときに実行されるメソッド (CommunityToolkit.Mvvmの機能)
        partial void OnActiveItemChanged(Data? oldValue, Data? newValue)
        {
            if (oldValue is not null) { oldValue.IsActive = false; }
            if (newValue is not null) { newValue.IsActive = true; }
        }


        // 要素の選択
        public void Select(Data target, bool isControlPressed)
        {
            if (target.Parent != EditingGroup) { return; }

            // Ctrlが押されていない時
            if (!isControlPressed)
            {
                // 一旦全解除してから、選択状態にする
                ClearSelection();
                target.IsSelected = true;
                SelectedItems.Add(target);
            }
            // Ctrlが押されている時
            else
            {
                // フラグをトグル切り替え
                target.IsSelected = !target.IsSelected;

                // フラグに従ってCollectionを更新
                if (target.IsSelected)
                { SelectedItems.Add(target); }
                else { _ = SelectedItems.Remove(target); }
            }
            // ActiveItemを決定
            // LastOrDefault最後の要素を取得する。要素が存在しない場合、型の既定値（例えば、null や 0）
            ActiveItem = target.IsSelected ? target : SelectedItems.LastOrDefault();
        }

        // 全選択解除
        public void ClearSelection()
        {
            foreach (var item in SelectedItems) { item.IsSelected = false; }
            SelectedItems.Clear();
            ActiveItem = null;
        }

        // 階層を潜る(編集モード開始)
        public void EnterGroup(GroupData group)
        {
            ClearSelection();
            EditingGroup = group;
        }

        // 階層を抜ける(編集モード終了)
        public void EscapeGroup()
        {
            ClearSelection();
            EditingGroup = EditingGroup?.Parent;
        }
    }
}
