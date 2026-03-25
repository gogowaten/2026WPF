using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace _20260325
{
    public partial class GeoShapeData : ShapeData
    {
        //[ObservableProperty] private ObservableCollection<Point> _points = [];
        [ObservableProperty] private PointCollection _points = [];
        [ObservableProperty] private PenLineCap _strokeEndLineCap = PenLineCap.Flat;
        [ObservableProperty] private PenLineCap _strokeStartLineCap = PenLineCap.Flat;
        [ObservableProperty] private double _strokeMiterLimit = 1.0;

    }
    public partial class EllipseData : ShapeData { }
    public abstract partial class ShapeData : Data
    {
        [ObservableProperty] private Brush? _fill;
        [ObservableProperty] private Brush? _stroke;
        [ObservableProperty] private double _strokeThickness = 1.0;
        [ObservableProperty] private PenLineJoin _strokeLineJoin = PenLineJoin.Miter;
    }
    public abstract partial class Data : ObservableObject
    {
        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private double _left;
        [ObservableProperty] private double _top;
        [ObservableProperty] private int _zIndex;
        [ObservableProperty] private double _width;
        [ObservableProperty] private double _height;
        [ObservableProperty] private Brush? _background = null;
    }
}
