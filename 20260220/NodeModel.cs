using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace _20260220
{
    public enum NodeType { Rectangle, Textblock, }
    public partial class NodeModel : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<NodeModel> _children = []; // 子要素
        [ObservableProperty] private double _x; // X座標
        [ObservableProperty] private double _y; // Y座標
        [ObservableProperty] private double _width; // 横サイズ
        [ObservableProperty] private double _height; // 縦サイズ

        public NodeModel() { }
    }
}
