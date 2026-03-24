using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace _20260324
{
    public partial class GeoLineData : Data
    {
        [ObservableProperty] private PointCollection _points = [];
        [ObservableProperty] private Brush _brush = Brushes.Gold;
        [ObservableProperty] private double _strokeThickness = 20;

        public GeoLineData()
        {
            XP = 0;
            YP = 0;
            Points.Add(new Point(50, 70));
            Points.Add(new Point(250, 150));
            Points.Add(new Point(50, 250));
            Points.Add(new Point(50, 200));
        }
    }

    public abstract partial class Data : ObservableObject
    {
        [ObservableProperty] private double _xP;
        [ObservableProperty] private double _yP;
        [ObservableProperty] private Brush _background = Brushes.WhiteSmoke;
    }
}
