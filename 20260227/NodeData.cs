using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;


namespace _20260227
{
    public abstract partial class Data : ObservableObject
    {
        [ObservableProperty] private double _x;
        [ObservableProperty] private double _y;
        [ObservableProperty] private double _z;
        [ObservableProperty] private double _width;
        [ObservableProperty] private double _height;
        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private Rect _externalRect;
        [ObservableProperty] private bool _isSelected;

    }

    public partial class Datas : Data
    {
        public ObservableCollection<Data> Nodes { get; set; } = [];
        public Datas(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
        public void AddNodeData(Data node)
        {
            Nodes.Add(node);
        }
    }

    public partial class TextBlockData : Data
    {
        [ObservableProperty] private string _text = string.Empty;
        public TextBlockData(double x, double y, string text)
        {
            this.X = x;
            this.Y = y;
            this.Text = text;
        }
    }

    public partial class RectangleData : Data
    {
        public RectangleData(double x, double y, double width, double height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }
    }



}
