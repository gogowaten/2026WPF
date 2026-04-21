using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml.Linq;

namespace _20260420_01_CanvasGeoThumbResize
{
    public partial class GeoLineData : ShapeData
    {

        public GeoLineData()
        {
            Name = "FromGeoLineData";
#if DEBUG
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.ReflectedType?.Name}__{MethodBase.GetCurrentMethod()?.Name}");
#endif
        }

        [ObservableProperty] private PointCollection? _myPoints;
        [ObservableProperty] private PenLineCap _endLineCap;
        [ObservableProperty] private PenLineCap _startLineCap;
        [ObservableProperty] private double _miterLimit = 10.0;
        [ObservableProperty] private PenLineJoin _lineJoin;
        [ObservableProperty] private bool _isCanDragMove;
        [ObservableProperty] private double _strokeThickness = 1.0;





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

    public class ConvRect : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var left = (double)values[0];
            var top = (double)values[1];
            var width = (double)values[2];
            var height = (double)values[3];
            return new Rect(left, top, width, height);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            Rect r = (Rect)value;
            return [r.Left, r.Top, r.Width, r.Height];
        }
    }


}