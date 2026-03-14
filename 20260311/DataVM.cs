using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Media;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;


namespace _20260311
{
    public partial class RootData : GroupData
    {
        [ObservableProperty] private Data? _currentItem; // 筆頭
        [ObservableProperty] private Data? _clickedItem; // 大抵は最後にクリックしたItem
        [ObservableProperty] private ObservableCollection<Data> _selectedItems = [];
        [ObservableProperty] private GroupData? _editingGroup;


        //[ObservableProperty] private GroupData _datas = new();
        //public DataService MyService { get; } = new();


        public RootData()
        {
            Name = "RootDataです";
            EditingGroup = this;
            MyInit();
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
        }
        

        #region On～プロパティの変更時


        partial void OnClickedItemChanged(Data? oldValue, Data? newValue)
        {
            if (oldValue is not null)
            {
                oldValue.IsClicked = false;
            }
            if (newValue is not null)
            {
                newValue.IsClicked = true;
            }
        }


        // 筆頭変更後
        partial void OnCurrentItemChanged(Data? oldValue, Data? newValue)
        {
            if (oldValue is not null) { oldValue.IsCurrent = false; }
            if (newValue is not null) { newValue.IsCurrent = true; }
        }

        // 編集Group変更時
        partial void OnEditingGroupChanged(GroupData? oldValue, GroupData? newValue)
        {
            ClearSelectedItems();
            CurrentItem = null;
            ClickedItem = null;

            if (oldValue is not null)
            {
                oldValue.IsEditing = false;
                foreach (var item in oldValue.DataList)
                {
                    item.IsSelectable = false;
                }

            }
            if (newValue is not null)
            {
                newValue.IsEditing = true;
                foreach (var item in newValue.DataList)
                {
                    item.IsSelectable = true;
                }
            }
        }

        #endregion On～プロパティの変更時

        // 全選択解除
        [RelayCommand]
        public void ClearSelectedItems()
        {
            foreach (var item in SelectedItems)
            {
                item.IsSelected = false;
                item.IsCurrent = false;
            }
            SelectedItems.Clear();
            CurrentItem = null;
        }

        // 選択状態のData変更時
        private void SelectedItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems?[0] is Data newData) { newData.IsSelected = true; }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems?[0] is Data oldData)
                {
                    oldData.IsSelected = false;
                    oldData.IsCurrent = false;
                }
            }
        }

        public void ChangeCurrentItem(Data data) { CurrentItem = data; }

        // 指定グループを編集モードにする
        public void MigrateEditingGroup(GroupData group) { EditingGroup = group; }

        // 編集モードを今の1個上に移行する
        public void MigrateEditingGroupUpper()
        {
            if (EditingGroup?.ParentData is GroupData upper)
            {
                EditingGroup = upper;
            }
        }

        // Currentを編集モードにする
        public void MigrateEditingGroupCurrent()
        {
            if (CurrentItem is GroupData group) { EditingGroup = group; }
        }



        public void AddSelect(Data data)
        {
            // 二重登録禁止
            if (SelectedItems.Contains(data)) return;
            if (data.IsSelectable == false) { return; }

            SelectedItems.Add(data);
            CurrentItem = data;
        }

        public void RemoveSelect(Data data)
        {
            var dataIndex = SelectedItems.IndexOf(data) - 1;
            SelectedItems.Remove(data);
            // 筆頭Itemを更新
            // 一個前を筆頭にする、一個前がなければ一個後を筆頭にする
            if (dataIndex < 0) { dataIndex++; }
            CurrentItem = SelectedItems[dataIndex];
        }


        public void AddData(Data data)
        {
            if (data.RootData is null) { data.RootData = this; }
            EditingGroup?.DataList.Add(data);
        }

        public void RemoveData(Data data)
        {
            EditingGroup?.DataList.Remove(data);
        }


        public void UpdateSize(GroupData group)
        {
            double right = 0;
            double bottom = 0;
            double mx = double.MaxValue;
            double my = double.MaxValue;
            foreach (var item in group.DataList)
            {
                mx = Math.Min(mx, item.X);
                my = Math.Min(my, item.Y);
                right = Math.Max(right, item.X + item.Width);
                bottom = Math.Max(bottom, item.Y + item.Height);
            }
            //group.X = mx; group.Y = my;
            group.Width = right; group.Height = bottom;
        }



        private void MyInit()
        {
            this.RootData = this; // 自身をRootにしておく
            this.CurrentItem = this; // 自身を筆頭にしておく
            this.IsEditing = true; // 起動時は自身が編集状態グループ

            RectangleData rRed = new() { Name = "赤四角", X = 0, Y = 0, Width = 60, Height = 60, Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)) };
            RectangleData rBlue = new() { Name = "青四角", X = 20, Y = 20, Width = 60, Height = 60, Fill = new SolidColorBrush(Color.FromArgb(50, 0, 0, 255)) };
            EllipseData maruRed = new() { Name = "黄玉", X = 0, Y = 0, Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromArgb(50, 255, 200, 0)) };
            EllipseData maruBlue = new() { Name = "水玉", X = 120, Y = 20, Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromArgb(50, 0, 200, 255)) };
            EllipseData maruGreen = new() { Name = "翠玉", X = 40, Y = 140, Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromArgb(50, 0, 255, 0)) };

            GroupData groupRect = new() { RootData = this, Name = "GropuA", X = 0, Y = 0 };
            groupRect.DataList.Add(rRed);
            groupRect.DataList.Add(rBlue);

            GroupData groupEllipse = new() { RootData = this, Name = "GropuB", X = 100, Y = 0 };
            groupEllipse.DataList.Add(maruRed);
            groupEllipse.DataList.Add(maruBlue);

            DataList.Add(groupRect);
            DataList.Add(groupEllipse);
            DataList.Add(maruGreen);

            // 直下のItemのIsSelectableをtrueにする
            foreach (var item in DataList)
            {
                item.IsSelectable = true;
            }

            DataSyokika(this);
        }

        // 起動時のDataの辻褄合わせ
        // すべてのItemのRootDataを自身にする
        private void DataSyokika(GroupData data)
        {
            foreach (var item in data.DataList)
            {
                item.RootData = this;
                if (item is GroupData group) { DataSyokika(group); }
            }
        }


    }
}
