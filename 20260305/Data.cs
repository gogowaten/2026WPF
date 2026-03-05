using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Media;

namespace _20260305
{
    public partial class GroupData : Data
    {
        [ObservableProperty] private ObservableCollection<Data> _datas = [];

        public GroupData()
        {
            X = 0; Y = 0; Name = "GroupData";
            Datas.CollectionChanged += Datas_CollectionChanged;
        }

        private void Datas_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            double right = 0, bottom = 0;
            bottom = Datas.Max(n => n.Height + n.Y);
            right = Datas.Max(n => n.Width + n.X);
            Width = right; Height = bottom;
        }
    }


    public partial class RectangleGroupData : Data
    {
        [ObservableProperty] private ObservableCollection<RectangleData> _datas = [];

        public RectangleGroupData()
        {
            X = 0; Y = 0; Name = "RectangleGroupData";
            Datas.CollectionChanged += Datas_CollectionChanged;
        }

        private void Datas_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            double right = 0, bottom = 0;
            bottom = Datas.Max(n => n.Height + n.Y);
            right = Datas.Max(n => n.Width + n.X);
            Width = right; Height = bottom;
        }
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

    public partial class RectangleData : Data
    {

    }
}
