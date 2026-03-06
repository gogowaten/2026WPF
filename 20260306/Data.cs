using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using System.Windows.Media;

namespace _20260306
{
    public partial class EllipseData : ShapeData { public EllipseData() { Width = 50; Height = 50; } }

    public partial class RectangleData : ShapeData
    {
        public RectangleData()
        {
            Width = 100; Height = 50;X = 200; Y = 0;
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
        [ObservableProperty] bool _isSelected = false;
    }
}
