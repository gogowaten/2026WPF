using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Windows.Media.Media3D;

namespace _20260305_Re0301
{
    public abstract partial class Data : ObservableObject
    {
        [ObservableProperty] private double _x;
        [ObservableProperty] private double _y;
        [ObservableProperty] private double _z;
        [ObservableProperty] private double _width;
        [ObservableProperty] private double _height;
        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private bool _isSelected;
        [ObservableProperty] private bool _isActive;


        //[ObservableProperty] private Rect _externalBounds;
        //[ObservableProperty] private Rect _contentBounds;

        [ObservableProperty] private GroupData? _parent;

    }


    public partial class GroupData : Data
    {
        [ObservableProperty] private bool _isEditing;
        [ObservableProperty] private ObservableCollection<Data> _children = [];


        public GroupData()
        {
            Children.CollectionChanged += Children_CollectionChanged;
        }

        private void Children_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null && e.NewItems[0] is Data item)
                {
                    item.Parent = this;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems != null && e.OldItems[0] is Data item)
                {
                    item.Parent = null;
                }
            }
            UpdateBounds();
        }

        public void UpdateBounds()
        {
            if (Children.Count == 0)
            {
                Width = 0;
                Height = 0;
                return;
            }

            // 全ての子要素を囲う最小矩形を計算
            double minX = Children.Min(d => d.X);
            double minY = Children.Min(d => d.Y);
            double maxX = Children.Max(d => d.X + d.Width);
            double maxY = Children.Max(d => d.Y + d.Height);

            // GroupData自体のサイズを更新
            // 注: 子要素の座標を相対的に維持しつつ枠を広げる計算
            Width = maxX;
            Height = maxY;
            this.Parent?.UpdateBounds();

        }
    }


    //public partial class RootData : Data
    //{
    //    [ObservableProperty] private ObservableCollection<GroupData> _layers = [];
    //    [ObservableProperty] private Data? _active;


    //    public RootData()
    //    {

    //    }
    //    public void AddLayer(GroupData layer)
    //    {
    //        Layers.Add(layer);
    //    }

    //    public void RemoveLayer(GroupData layer)
    //    {
    //        Layers.Remove(layer);
    //    }
    //}

    //public partial class TextBlockData : Data
    //{
    //    [ObservableProperty] private string _text = string.Empty;
    //    public TextBlockData(double x, double y, string text)
    //    {
    //        this.X = x;
    //        this.Y = y;
    //        this.Text = text;

    //    }
    //}

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
