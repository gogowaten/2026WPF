using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Media;

namespace _20260224_DataTemplate_Behavior_CustomCtrl
{
    public partial class MainViewModel : ObservableObject
    {
        public ObservableCollection<Item> RootItems { get; } = [];
        //[ObservableProperty] private bool _canDrag = true; // ドラッグ移動の可否判定用

        public MainViewModel()
        {
            //Items items = new(10, 0);
            //items.Children.Add(new TextBlockItem(10, 20, "Text A"));
            //items.Children.Add(new TextBlockItem(20, 50, "Text B"));
            //RootItems.Add(items);

            RootItems.Add(new TextBlockItem(100, 110, "Text C"));
            //RootItems.Add(new RectangleItem(100, 150, Brushes.Yellow, 100, 40));

        }

        //[RelayCommand]
        //public void AddItem(Item item)
        //{
        //    RootItems.Add(item);
        //}

        //public void RemoveItem(Item item)
        //{
        //    RootItems.Remove(item);
        //}


    }
}
