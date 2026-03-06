using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace _20260306_DaraTemplateRectangle
{
    public abstract partial class Data : ObservableObject
    {
        [ObservableProperty] private double _width;
        [ObservableProperty] private double _height;
        //[ObservableProperty] private double _x;
        //[ObservableProperty] private double _y;
        //[ObservableProperty] private string _name = string.Empty;
        //[ObservableProperty] bool _isSelected = false;
    }

    public partial class RectangleData : Data
    {
        
        public RectangleData(double width, double height)
        {
            this.Width = width;
            this.Height = height;
        }
    }

}
