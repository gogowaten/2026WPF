using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Windows.Media;
using System.Xml.Linq;

namespace _20260311_DataTemplate_SelectedItems
{
    public partial class GroupData : Data
    {
        [ObservableProperty] private bool _isEditing; // 編集状態
        [ObservableProperty] private ObservableCollection<Data> _dataList = [];

        public GroupData()
        {
            Name = "GroupData";
            DataList.CollectionChanged += DataList_CollectionChanged;
        }

        private void DataList_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems?[0] is Data newData) { newData.ParentData = this; }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems?[0] is Data oldData) { oldData.ParentData = null; }
            }
        }
    }


    public partial class EllipseData : ShapeData { }

    public partial class RectangleData : ShapeData { }


    public abstract partial class ShapeData : Data
    {
        [ObservableProperty] private Brush _fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0));
    }


    public abstract partial class Data : ObservableObject
    {
        [ObservableProperty] private RootData? _rootData;
        [ObservableProperty] private GroupData? _parentData;
        [ObservableProperty] private double _width;
        [ObservableProperty] private double _height;
        [ObservableProperty] private double _x;
        [ObservableProperty] private double _y;
        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] bool _isSelected = false; // 選択状態
        [ObservableProperty] bool _isSelectable = false; // 選択状態
        [ObservableProperty] bool _isCurrent = false; // 筆頭
        [ObservableProperty] bool _isClicked = false; // クリックされた要素

    }
}