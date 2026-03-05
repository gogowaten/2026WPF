using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace _20260305
{
    public partial class GroupVM : ObservableObject
    {

        public GroupVM()
        {
            Data = new GroupData() { X = 0, Y = 0, Name = "GroupVM" };
            Data.Datas.Add(new RectangleData() { X = 0, Y = 0, Width = 100, Height = 50, Name = "Rect1" });
            Data.Datas.Add(new RectangleData() { X = 30, Y = 40, Width = 100, Height = 50, Name = "Rect2" });

        }

        [ObservableProperty] private GroupData _data;
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
