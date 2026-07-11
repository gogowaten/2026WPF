using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace _20260710_Template;

public partial class Data : ObservableObject
{
    [ObservableProperty] private double _windowWidth = 400;
    [ObservableProperty] private double _windowHeight = 200;

    //public double WindowWidth { get; set; } = 400;
    //public double WindowHeight { get; set; } = 200;

}
