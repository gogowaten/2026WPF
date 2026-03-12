using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Collections.Specialized;


namespace _20260311
{
    //public partial class DataService : ObservableObject
    //{

    //    public RootData MyRoot
    //    {
    //        get { return (RootData)GetValue(MyRootProperty); }
    //        set { SetValue(MyRootProperty, value); }
    //    }
    //    public static readonly DependencyProperty MyRootProperty =
    //        DependencyProperty.Register(nameof(MyRoot), typeof(RootData), typeof(DataService), new PropertyMetadata(null));

    //    [ObservableProperty] private Data? _currentItem;
    //    [ObservableProperty] private ObservableCollection<Data> _selectedItems = [];
    //    [ObservableProperty] private GroupData? _editingGroup;

    //    public DataService()
    //    {
    //        SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
    //    }

    //    // 選択全部解除
    //    public void ClearSelection()
    //    {
    //        foreach (var item in SelectedItems) { item.IsSelected = false; }
    //        SelectedItems.Clear();
    //        CurrentItem = null;
    //    }

    //    // 編集モード開始
    //    public void EditStart(GroupData group)
    //    {

    //    }

        
    //    #region プロパティ変更時の動作
        
    //    private void SelectedItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    //    {
    //        if (e.Action == NotifyCollectionChangedAction.Add)
    //        {
    //            if (e.NewItems?[0] is Data newData) { newData.IsSelected = true; }
    //        }
    //        else if (e.Action == NotifyCollectionChangedAction.Remove)
    //        {
    //            if (e.OldItems?[0] is Data oldData) { oldData.IsSelected = false; }
    //        }
    //    }

    //    partial void OnEditingGroupChanged(GroupData? oldValue, GroupData? newValue)
    //    {
    //        if (oldValue is not null) { oldValue.IsEditing = false; }
    //        if (newValue is not null) { newValue.IsEditing = true; }
    //    }

    //    partial void OnCurrentItemChanged(Data? oldValue, Data? newValue)
    //    {
    //        if (oldValue is not null) { oldValue.IsCurrent = false; }
    //        if (newValue is not null) { newValue.IsCurrent = true; }
    //    }
    //    #endregion プロパティ変更時の動作



    //}

}
