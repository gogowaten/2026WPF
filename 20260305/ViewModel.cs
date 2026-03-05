using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace _20260305
{
    public partial class GroupVM : ObservableObject
    {
        [ObservableProperty] private GroupData _datas = new();

    }

    public partial class RectangleGroupVM : ObservableObject
    {
        [ObservableProperty] private RectangleGroupData _datas = new();

        public RectangleGroupVM()
        {
            Datas.Datas.Add(new RectangleData() { X = 0, Y = 0, Height = 50, Width = 100 });
            Datas.Datas.Add(new RectangleData() { X = 10, Y = 30, Height = 50, Width = 100 });
        }

    }


    public partial class RectangleVM : ObservableObject
    {
        [ObservableProperty] private RectangleData _data;

        public RectangleVM()
        {
            Data = new()
            {
                Width = 100,
                Height = 100
            };
        }
    }


}
