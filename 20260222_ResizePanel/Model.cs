using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace _20260222_ResizePanel
{
    public enum NodeType { Node, Rectangle, TextBlock }
    public partial class NodeModel : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<NodeModel> _children = [];
        [ObservableProperty] private NodeType _nodeType;
        [ObservableProperty] private double _x;
        [ObservableProperty] private double _y;
        [ObservableProperty] private double _width;
        [ObservableProperty] private double _height;
        [ObservableProperty] private string _text = string.Empty;


    }
}
