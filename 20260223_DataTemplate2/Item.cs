using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;

namespace _20260223_DataTemplate2
{
    public enum ItemType { Items, TextBlock, Rectangle }


    public partial class Item(ItemType itemType, double x, double y) : ObservableObject
    {
        public ItemType Type { get; private set; } = itemType;
        [ObservableProperty] private double _x = x;
        [ObservableProperty] private double _y = y;
    }

    //public partial class Item : ObservableObject
    //{
    //    public ItemType Type { get; private set; }

    //    [ObservableProperty] private double _x;
    //    [ObservableProperty] private double _y;

    //    public Item(ItemType itemType) { this.Type = itemType; }
    //}

    public partial class Items(double x, double y) : Item(ItemType.Items, x, y)
    {
        public ObservableCollection<Item> Children { get; private set; } = [];
    }


    public partial class TextBlockItem(double x, double y, string text) : Item(ItemType.TextBlock, x, y)
    {
        [ObservableProperty] private string _text = text;
    }

    public partial class RectangleItem(double x, double y, Brush fill, double w, double h) : Item(ItemType.Rectangle, x, y)
    {
        [ObservableProperty] private Brush _fill = fill;
        [ObservableProperty] private double _width = w;
        [ObservableProperty] private double _height = h;
    }
}
