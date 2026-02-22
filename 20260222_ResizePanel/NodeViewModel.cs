using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace _20260222_ResizePanel
{
    public partial class NodeViewModel : ObservableObject
    {
        public NodeModel Model { get; }

        public NodeViewModel(NodeModel model)
        {
            Model = model;
            Children = new ObservableCollection<NodeViewModel>(
                model.Children.Select(child => new NodeViewModel(child))
            );
        }

        public ObservableCollection<NodeViewModel> Children { get; }

        public NodeType NodeType => Model.NodeType;

        public double X
        {
            get => Model.X;
            set => SetProperty(Model.X, value, Model, (m, v) => m.X = v);
        }

        public double Y
        {
            get => Model.Y;
            set => SetProperty(Model.Y, value, Model, (m, v) => m.Y = v);
        }

        public double Width
        {
            get => Model.Width;
            set => SetProperty(Model.Width, value, Model, (m, v) => m.Width = v);
        }

        public double Height
        {
            get => Model.Height;
            set => SetProperty(Model.Height, value, Model, (m, v) => m.Height = v);
        }

        public string Text
        {
            get => Model.Text;
            set => SetProperty(Model.Text, value, Model, (m, v) => m.Text = v);
        }
    }

}
