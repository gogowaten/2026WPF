using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
