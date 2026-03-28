using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Collections.ObjectModel;

namespace _20260327
{
    public partial class GeoShapeData : ShapeData
    {
        public GeoShapeData()
        {
            Name = "GeoShapeData";
            //X = 10;
            //Y = 20;
            //IsOffset = true;
            Test();
        }

        private void Test()
        {
            Background = Brushes.DeepSkyBlue;
            //Fill = Brushes.DeepSkyBlue;
            Stroke = Brushes.Gold;
            StrokeThickness = 20;
            Points.Add(new Point(50, 70));
            Points.Add(new Point(250, 150));
            Points.Add(new Point(50, 250));
            Points.Add(new Point(50, 200));
        }

        [ObservableProperty] private PointCollection _points = [];
        //[ObservableProperty] private ObservableCollection<Point> _points = [];
        [ObservableProperty] private PenLineCap _endLineCap;
        [ObservableProperty] private PenLineCap _startLineCap;
        [ObservableProperty] private double _miterLimit = 1.0;
        [ObservableProperty] private PenLineJoin _lineJoin;
        [ObservableProperty] private bool _isOffset;
        partial void OnIsOffsetChanged(bool value)
        {
            if (value)
            {
                X += OriginBounds.X;
                Y += OriginBounds.Y;
            }
            else
            {
                X -= OriginBounds.X;
                Y -= OriginBounds.Y;
            }
        }
    }
    public abstract partial class ShapeData : Data
    {
        [ObservableProperty] private Brush? _fill;
        [ObservableProperty] private double _strokeThickness = 1.0;
        [ObservableProperty] private Brush? _stroke;
    }
    public abstract partial class Data : ObservableObject
    {
        [ObservableProperty] private double _x;
        [ObservableProperty] private double _y;
        [ObservableProperty] private double _width;
        [ObservableProperty] private double _height;
        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private Brush? _background;
        [ObservableProperty] private Rect _bounds = new();
        [ObservableProperty] private Rect _originBounds = new();
    }
}
