using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Media;
using System.Xml.Linq;

namespace _20260309
{
    public partial class GroupData : Data
    {
        [ObservableProperty] private ObservableCollection<Data> _dataList = [];

        public GroupData() { Name = "GroupData"; }
    }

    public partial class EllipseData : ShapeData { }

    public partial class RectangleData : ShapeData { }


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
        [ObservableProperty] bool _isSelected = false;
    }
}