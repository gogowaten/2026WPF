using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;

namespace _20260316
{
    public partial class TextBlockData : TextData
    {
        public TextBlockData()
        {
            FontSize = 30;

        }
    }
    public abstract partial class TextData : Data
    {
        [ObservableProperty] private string _text = string.Empty;
        [ObservableProperty] private string _fontName = SystemFonts.MessageBoxFont!.ToString();
        [ObservableProperty] private double _fontSize = SystemFonts.MessageBoxFont.Size;

    }

    public abstract partial class Data : ObservableObject
    {
        [ObservableProperty] private double _width;
        [ObservableProperty] private double _height;
        [ObservableProperty] private double _x;
        [ObservableProperty] private double _y;
        [ObservableProperty] private string _name = string.Empty;


    }
}
