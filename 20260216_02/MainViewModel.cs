using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;

namespace _20260216_02
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private NodeViewModel root = new();

        [RelayCommand]
        private void AddNode()
        {
            var child = new NodeViewModel
            {
                X = 30,
                Y = 50,
            };

            Root.Children.Add(child);
        }

        // RelayCommandではオーバーロードはできない
        //[RelayCommand]
        //private void AddNode(double x, double y)
        //{
        //    NodeViewModel child = new() { X = x, Y = y };
        //    Root.Children.Add(child);
        //}

        [RelayCommand]
        private void AddRectangle()
        {
            System.Windows.Shapes.Rectangle r = new() { Height = 50, Width = 50, Fill = Brushes.Red };

        }
    }
}
