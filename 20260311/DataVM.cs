using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Media;
using System.Collections.Specialized;


namespace _20260311
{
    public partial class RootData : GroupData
    {
        [ObservableProperty] private Data? _currentItem;
        [ObservableProperty] private ObservableCollection<Data> _selectedItems = [];
        [ObservableProperty] private GroupData? _editingGroup;

        //[ObservableProperty] private GroupData _datas = new();
        //public DataService MyService { get; } = new();


        public RootData()
        {
            EditingGroup = this;
            MyInit();
            //SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
        }

        private void SelectedItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems?[0] is Data newData) { newData.IsSelected = true; }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems?[0] is Data oldData) { oldData.IsSelected = false; }
            }
        }

        partial void OnEditingGroupChanged(GroupData? oldValue, GroupData? newValue)
        {
            if (oldValue is not null)
            {
                oldValue.IsEditing = false;
                foreach (var item in oldValue.DataList) { item.IsSelectable = false; }
            }
            if (newValue is not null)
            {
                newValue.IsEditing = true;
                foreach (var item in newValue.DataList) { item.IsSelectable = true; }
            }
        }

        public void AddSelect(Data data)
        {
            // 二重登録禁止
            if (SelectedItems.Contains(data)) return;

            SelectedItems.Add(data);
        }

        public void RemoveSelect(Data data)
        {
            SelectedItems.Remove(data);
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

        private void MyInit()
        {
            this.RootData = this; // 自身をRootにしておく
            this.IsEditing = true; // 起動時は自身が編集状態グループ

            RectangleData rRed = new() { Name = "RedRect", X = 0, Y = 0, Width = 60, Height = 60, Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)) };
            RectangleData rBlue = new() { Name = "BlueRect", X = 20, Y = 20, Width = 60, Height = 60, Fill = new SolidColorBrush(Color.FromArgb(50, 0, 0, 255)) };
            EllipseData maruRed = new() { Name = "RedEllipse", X = 0, Y = 0, Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)) };
            EllipseData maruBlue = new() { Name = "BlueEllipse", X = 20, Y = 20, Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromArgb(50, 0, 0, 255)) };
            EllipseData maruGreen = new() { Name = "GreenEllipse", X = 40, Y = 140, Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromArgb(50, 0, 255, 0)) };

            GroupData groupRect = new() { RootData = this, Name = "GropuA", X = 0, Y = 0 };
            //groupRect.RootData.AddData(rRed);
            //groupRect.RootData.AddData(rBlue);
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
