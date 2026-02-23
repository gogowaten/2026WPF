using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace _20260223_DataTemplate
{
    public partial class Item : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<Item> _children = [];
        [ObservableProperty] private string _text = string.Empty;
        [ObservableProperty] private double _x;
        [ObservableProperty] private double _y;

        public Item(string text, double x, double y)
        {
            this.Text = text;
            this.X = x;
            this.Y = y;
        }
    }

}
