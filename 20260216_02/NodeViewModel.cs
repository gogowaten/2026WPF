using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace _20260216_02
{
    public partial class NodeViewModel : ObservableObject
    {
        [ObservableProperty] private double _x;
        [ObservableProperty] private double _y;
        [ObservableProperty] private string? _text;


        public ObservableCollection<NodeViewModel> Children { get; } = [];

        public NodeViewModel? Parent { get; private set; }

        public void AddChild(NodeViewModel child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        public void RemoveChild(NodeViewModel child)
        {
            child.Parent = null;
            Children.Remove(child);
        }



    }
}
