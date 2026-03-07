using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace _20260307
{
    //public partial class DataVM : GroupData
    //{
    //    public DataVM()
    //    {
    //        Name = "DataVM"; X = 0; Y = 0;
    //    }

    //    [RelayCommand]
    //    private void AddGroup(GroupData data)
    //    {
    //        DataList.Add(data);
    //    }

    //}

    //public partial class Root : Data
    //{
    //    [ObservableProperty] private ObservableCollection<Layer> _layers = [];
    //}
    //public partial class Layer : Data
    //{
    //    [ObservableProperty] private ObservableCollection<GroupData> _groupList = [];
    //}
    public partial class GroupData : Data
    {
        [ObservableProperty] private ObservableCollection<Data> _dataList = [];

        public GroupData() { Name = "GroupData"; }
    }

    public partial class EllipseData : ShapeData { public EllipseData() { Width = 50; Height = 50; } }

    public partial class RectangleData : ShapeData
    {
        public RectangleData()
        {
            Width = 100; Height = 50; X = 200; Y = 0;
            Fill = new SolidColorBrush(Color.FromArgb(50, 0, 200, 255));
        }
    }

    public abstract partial class ShapeData : Data
    {
        [ObservableProperty] private Brush _fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0));
    }


    public abstract partial class Data : ObservableObject
    {
        [ObservableProperty] private double _width;
        [ObservableProperty] private double _height;
        [ObservableProperty] private double _x;
        [ObservableProperty] private double _y;
        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private bool _isSelected = false;
        [ObservableProperty] private GroupData? _parent;
        [ObservableProperty] private GroupData? _root;
    }
}