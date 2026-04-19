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

namespace _20260419
{
    public partial class GeoLineData : ShapeData
    {

        public GeoLineData()
        {
            Name = "FromGeoLineData";
        }

        [ObservableProperty] private PointCollection? _myPoints;
        [ObservableProperty] private Pen _strokePen = null!;
        [ObservableProperty] private PenLineCap _endLineCap;
        [ObservableProperty] private PenLineCap _startLineCap;
        [ObservableProperty] private double _miterLimit = 10.0;
        [ObservableProperty] private PenLineJoin _lineJoin;
        [ObservableProperty] private bool _isOffset;
        [ObservableProperty] private bool _isCanDragMove;
        [ObservableProperty] private double _strokeThickness = 1.0;


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
        }

        #endregion StrokePen更新

        public Rect OnUpdateBounds(Geometry? _cachedGeometry)
        {
            if (_cachedGeometry is null)
            {
                MySizeReset();
                return new Rect();
            }

            Rect geoBounds = _cachedGeometry.GetRenderBounds(StrokePen);
            if (geoBounds.IsEmpty || _cachedGeometry is null)
            {
                MySizeReset();
                return geoBounds;
            }


            var diffLeft = geoBounds.Left - Bounds.Left;
            var diffTop = geoBounds.Top - Bounds.Top;
            InternalX += diffLeft;
            InternalY += diffTop;

            BoundsTop = geoBounds.Top;
            BoundsLeft = geoBounds.Left;
            BoundsWidth = geoBounds.Width;
            BoundsHeight = geoBounds.Height;
            Bounds = geoBounds;

            if (IsOffset)
            {
                Width = InternalX + geoBounds.Width;
                Height = InternalY + geoBounds.Height;
            }

            if (geoBounds.Left < 0)
            {
                Width = geoBounds.Width;
                Height = geoBounds.Height;
            }

            Bounds = geoBounds;
            return geoBounds;
        }

        private void MySizeReset()
        {

            Bounds = new();

            BoundsLeft = 0;
            BoundsTop = 0;
            BoundsWidth = 0;
            BoundsHeight = 0;

            Width = 0;
            Height = 0;
        }

    }






    public abstract partial class ShapeData : Data
    {
        [ObservableProperty] private double _internalX;
        [ObservableProperty] private double _internalY;
        #region 確認用
        [ObservableProperty] private double _boundsLeft;
        [ObservableProperty] private double _boundsTop;
        [ObservableProperty] private double _boundsWidth;
        [ObservableProperty] private double _boundsHeight;
        #endregion 確認用

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