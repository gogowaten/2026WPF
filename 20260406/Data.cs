using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Xml.Linq;

namespace _20260406
{
    public partial class GeoLineData : ShapeData
    {
        public GeoLineData()
        {
            Name = "FromGeoLineData";
        }


        private void Points_Changed(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        [ObservableProperty] private PointCollection _points = [];
        //[ObservableProperty] private ObservableCollection<Point> _points = [];
        [ObservableProperty] private PenLineCap _endLineCap;
        [ObservableProperty] private PenLineCap _startLineCap;
        [ObservableProperty] private double _miterLimit = 1.0;
        [ObservableProperty] private PenLineJoin _lineJoin;
        [ObservableProperty] private bool _isOffset;
        [ObservableProperty] private bool _isCanDragMove;

    }




    public abstract partial class ShapeData : Data
    {
        [ObservableProperty] private double _internalX;
        [ObservableProperty] private double _internalY;
        [ObservableProperty] private double _boundsLeft;
        [ObservableProperty] private double _boundsTop;
        [ObservableProperty] private double _boundsWidth;
        [ObservableProperty] private double _boundsHeight;
        [ObservableProperty] private double _myActualWidth;
        [ObservableProperty] private double _myActualHeight;
        [ObservableProperty] private double _actualWidth;
        [ObservableProperty] private double _actualHeight;

        // 確認用
        [ObservableProperty] private double _geoLeft;
        [ObservableProperty] private double _geoTop;
        [ObservableProperty] private double _geoWidth;
        [ObservableProperty] private double _geoHeight;
        // 確認用

        //[ObservableProperty] private Size _actualSize;
        [ObservableProperty] private double _xRender;
        [ObservableProperty] private double _yRender;

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
        //// Rectを使うのは良くない、バラしたほうがパフォーマンスも良い
        //[ObservableProperty] private Rect _bounds = new();
        //[ObservableProperty] private Rect _originBounds = new();
    }
}