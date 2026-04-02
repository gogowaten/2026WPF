using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using System.Xml.Linq;

namespace _20260331
{
    public partial class GeoLineData : ShapeData
    {
        public GeoLineData()
        {
            Name = "FromGeoLineData";
        }

        [ObservableProperty] private PointCollection _points = [];
        //[ObservableProperty] private ObservableCollection<Point> _points = [];
        [ObservableProperty] private PenLineCap _endLineCap;
        [ObservableProperty] private PenLineCap _startLineCap;
        [ObservableProperty] private double _miterLimit = 1.0;
        [ObservableProperty] private PenLineJoin _lineJoin;
        [ObservableProperty] private bool _isOffset;

    }



    public partial class GeoShapeData : ShapeData
    {
        [ObservableProperty] private double _xRender;
        [ObservableProperty] private double _yRender;
        [ObservableProperty] private double _widthRender;
        [ObservableProperty] private double _heightRender;
        [ObservableProperty] private PointCollection _points = [];
        //[ObservableProperty] private ObservableCollection<Point> _points = [];
        [ObservableProperty] private PenLineCap _endLineCap;
        [ObservableProperty] private PenLineCap _startLineCap;
        [ObservableProperty] private double _miterLimit = 1.0;
        [ObservableProperty] private PenLineJoin _lineJoin;
        [ObservableProperty] private bool _isOffset;


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
        // Rectを使うのは良くない、バラしたほうがパフォーマンスも良い
        [ObservableProperty] private Rect _bounds = new();
        [ObservableProperty] private Rect _originBounds = new();
    }
}