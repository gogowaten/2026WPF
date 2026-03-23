using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml.Linq;
using System.Windows;

namespace _20260321
{



    public partial class PathData : ShapeData
    {
        [ObservableProperty] private double _id;
    }
    public abstract partial class ShapeData : Data
    {
        //[ObservableProperty] private Brush _fill = Brushes.Red;
        //[ObservableProperty] private Brush _stroke = Brushes.Blue;
        //[ObservableProperty] private double _strokeThickness = 1.0;
    }

    public abstract partial class Data : ObservableObject
    {
        //[ObservableProperty] private RootData? _rootData;
        //[ObservableProperty] private GroupData? _parentData;
        //[ObservableProperty] private double _width;
        //[ObservableProperty] private double _height;
        //[ObservableProperty] private double _x;
        //[ObservableProperty] private double _y;
        //[ObservableProperty] private int _z;
        //[ObservableProperty] private string _name = string.Empty;
        //[ObservableProperty] bool _isSelected = false; // 選択状態
        //[ObservableProperty] bool _isSelectable = false; // 選択状態
        //[ObservableProperty] bool _isCurrent = false; // 筆頭
        //[ObservableProperty] bool _isClicked = false; // クリックされた要素

    }
}