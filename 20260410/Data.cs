using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace _20260410
{
    public partial class GeoLineData : ShapeData
    {
        //private Geometry? _cachedGeometry;
        //public Rect MyGeometryBounds { get; set; }

        public GeoLineData()
        {
            Name = "FromGeoLineData";
        }

        //[ObservableProperty] private Geometry _myGeometry = Geometry.Empty;
        //[ObservableProperty] private Geometry _myOffGeometry = Geometry.Empty;
        [ObservableProperty] private PointCollection? _myPoints;
        //[ObservableProperty] private PointCollection? _myOffsetPoints;
        [ObservableProperty] private Pen _strokePen = null!;
        [ObservableProperty] private PenLineCap _endLineCap;
        [ObservableProperty] private PenLineCap _startLineCap;
        [ObservableProperty] private double _miterLimit = 10.0;
        [ObservableProperty] private PenLineJoin _lineJoin;
        [ObservableProperty] private bool _isOffset;
        [ObservableProperty] private bool _isCanDragMove;
        [ObservableProperty] private double _strokeThickness = 1.0;
        //[ObservableProperty] private Thumb? _parentPath;

        // 確認用
        //[ObservableProperty] private Rect _myGeometryBounds = Rect.Empty;


        #region StrokePen更新

        partial void OnEndLineCapChanged(PenLineCap value)
        {
            UpdatePen();
        }
        partial void OnStartLineCapChanged(PenLineCap value)
        {
            UpdatePen();
        }
        partial void OnMiterLimitChanged(double value)
        {
            UpdatePen();
        }
        partial void OnLineJoinChanged(PenLineJoin value)
        {
            UpdatePen();
        }
        partial void OnStrokeThicknessChanged(double value)
        {
            UpdatePen();
        }

        public void UpdatePen()
        {
            StrokePen = new Pen(Stroke, StrokeThickness)
            {
                EndLineCap = EndLineCap,
                StartLineCap = StartLineCap,
                MiterLimit = MiterLimit,
                LineJoin = LineJoin,

            };
            //UpdateGeometry();
        }

        #endregion StrokePen更新

        //partial void OnMyGeometryChanged(Geometry? oldValue, Geometry newValue)
        //{
        //    TranslateTransform tt = new(-MyGeometryBounds.X, -MyGeometryBounds.Y);
        //    MyOffGeometry = MyGeometry.Clone();
        //    MyOffGeometry.Transform = tt;
        //}

        //partial void OnMyPointsChanged(PointCollection? oldValue, PointCollection? newValue)
        //{
        //    oldValue?.Changed -= MyPoints_Changed;
        //    newValue?.Changed += MyPoints_Changed;
        //}

        //private void MyPoints_Changed(object? sender, EventArgs e)
        //{
        //    UpdateGeometry();
        //}

        //private void UpdateGeometry()
        //{
        //    if (_cachedGeometry is not null)
        //    {

        //    }

        //    if (MyPoints is null || MyPoints.Count < 2)
        //    {
        //        _cachedGeometry = null;
        //        MyGeometry = Geometry.Empty;
        //        UpdateMySize();
        //        return;
        //    }

        //    PathGeometry geo = MakeLineGeometry(MyPoints);
        //    _cachedGeometry = geo;
        //    UpdateMySize();
        //    MyGeometry = geo;

        //}

        //private void UpdateMySize()
        //{

        //    if (_cachedGeometry is null)
        //    {
        //        MySizeReset();
        //        return;
        //    }

        //    //if (MyData is null)
        //    //{
        //    //    return;
        //    //}

        //    //Rect bounds = _cachedGeometry.GetRenderBounds(MyStrokePen);
        //    Rect bounds = _cachedGeometry.GetRenderBounds(StrokePen);
        //    if (bounds.IsEmpty || _cachedGeometry is null)
        //    {
        //        MySizeReset();
        //        return;
        //    }

        //    //if (MyData is null) { return; }

        //    var diffLeft = bounds.Left - MyGeometryBounds.Left;
        //    MyGeometryBounds = bounds;
        //    //double w = bounds.Width;
        //    //if (bounds.Left < 0) { w -= bounds.Left; }
        //    //MyData.MyActualWidth = w;
        //    //double h = bounds.Height;
        //    //if (bounds.Top < 0) { h -= bounds.Top; }
        //    //MyData.MyActualHeight = h;

        //    BoundsTop = bounds.Top;
        //    BoundsLeft = bounds.Left;
        //    BoundsWidth = bounds.Width;
        //    BoundsHeight = bounds.Height;

        //    if (IsOffset)
        //    {
        //        Width = bounds.Width + InternalX;
        //        Height = bounds.Height + InternalY;
        //    }

        //    //MyData.Width = bounds.Width + bounds.Left;
        //    //MyData.InternalX += diffLeft;

        //    //if (bounds.Left < 0)
        //    //{
        //    //    MyData.Width = bounds.Width;
        //    //}

        //    //InvalidateVisual(); // あったほうが良い、ないとたまに図形が更新されない時がある

        //}

        //private void MySizeReset()
        //{

        //    MyGeometryBounds = new();

        //    //if (MyData is null) { return; }

        //    BoundsLeft = 0;
        //    BoundsTop = 0;
        //    BoundsWidth = 0;
        //    BoundsHeight = 0;
        //    Width = 0;
        //    Height = 0;
        //}

        //private static PathGeometry MakeLineGeometry(IEnumerable<Point> pc)
        //{
        //    if (!pc.Any()) { return new PathGeometry(); }

        //    var seg = new PolyLineSegment(pc, true);
        //    var fig = new PathFigure(pc.First(), [seg], false);
        //    //var fig = new PathFigure(pc[0], [seg], false);
        //    var geo = new PathGeometry([fig]);
        //    return geo;
        //}

        //[RelayCommand]
        //public void Offset()
        //{
        //    if (MyPoints is null) { return; }
        //    //if (IsOffset)
        //    //{
        //    //    IsOffset = false;
        //    //    for (int i = 0; i < MyPoints.Count; i++)
        //    //    {
        //    //        MyPoints[i] = new Point(MyPoints[i].X - MyGeometryBounds.Left, MyPoints[i].Y - MyGeometryBounds.Top);
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    IsOffset = true;
        //    //    for (int i = 0; i < MyPoints.Count; i++)
        //    //    {
        //    //        MyPoints[i] = new Point(MyPoints[i].X + MyGeometryBounds.Left, MyPoints[i].Y + MyGeometryBounds.Top);
        //    //    }
        //    //}

        //    //for (int i = 0; i < MyPoints.Count; i++)
        //    //{
        //    //    MyPoints[i] = new Point(MyPoints[i].X - MyGeometryBounds.Left, MyPoints[i].Y - MyGeometryBounds.Top);
        //    //}

        //    if (IsOffset) { 
        //    MyGeometry = MyOffGeometry;
        //    }
        //    else
        //    {

        //    }

        //}
    }




    public abstract partial class ShapeData : Data
    {
        [ObservableProperty] private double _internalX;
        [ObservableProperty] private double _internalY;
        [ObservableProperty] private double _boundsLeft;
        [ObservableProperty] private double _boundsTop;
        [ObservableProperty] private double _boundsWidth;
        [ObservableProperty] private double _boundsHeight;

        [ObservableProperty] private Brush? _fill;
        [ObservableProperty] private double _strokeThickness = 1.0;
        [ObservableProperty] private Brush? _stroke;

        [ObservableProperty] private Rect _bounds;

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