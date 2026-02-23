using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Runtime.CompilerServices;
using System.Text;

namespace _20260223
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


    //public abstract partial class Item : ObservableObject
    //{
    //    public ItemType Type { get; private set; }
    //    [ObservableProperty] private ObservableCollection<Item> _children = [];
    //    [ObservableProperty] private double _x;
    //    [ObservableProperty] private double _y;

    //    public Item(ItemType type, double x, double y)
    //    {
    //        Type = type;
    //        _x = x;
    //        _y = y;
    //    }
    //}

    //public partial class TextBlockItem : Item
    //{
    //    [ObservableProperty] private string _text = string.Empty;
    //    public TextBlockItem(double x, double y, string text) : base(ItemType.TextBlock, x, y)
    //    {
    //        _text = text;
    //    }
    //}

    //public partial class RectangleItem : Item
    //{
    //    [ObservableProperty] private Brush _fill = Brushes.Yellow;
    //    [ObservableProperty] private double _width;
    //    [ObservableProperty] private double _height;

    //    public RectangleItem(double x, double y, Brush fill, double w, double h) : base(ItemType.Rectangle, x, y)
    //    {
    //        _fill = fill;
    //        _width = w;
    //        _height = h;
    //    }
    //}





}
