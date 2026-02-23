using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace _20260223_DataTemplate
{
    public partial class MainViewModel : ObservableObject
    {
        public ObservableCollection<Item> RootItems { get; } = [];

        public MainViewModel()
        {
            var root = new Item("メインテーマ", 100, 100);

            var child1 = new Item("アイデア A", 150, -50);
            child1.Children.Add(new Item("詳細 A-1", 100, 0));

            var child2 = new Item("アイデア B", 150, 90);

            root.Children.Add(child1);
            root.Children.Add(child2);

            RootItems.Add(root);
        }
    }
}
