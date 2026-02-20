using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;


namespace _20260219
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
        [ObservableProperty] private double _fontSize;
        [ObservableProperty] private double _text;
    }

    public class Node : Panel
    {
        //public ObservableCollection<Node> Children = [];
        public NodeModel NodeModel { get; set; }
        public Node(NodeModel nodeModel)
        {
            this.NodeModel = nodeModel;
            DataContext = NodeModel;
        }
    }
}
