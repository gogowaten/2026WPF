using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Collections.Specialized;

namespace _20260224
{
    [JsonDerivedType(typeof(TextBlockItem), nameof(TextBlockItem))]
    [JsonDerivedType(typeof(RectangleItem), nameof(RectangleItem))]
    [JsonDerivedType(typeof(GroupItem), nameof(GroupItem))]
    public abstract partial class Item(double x, double y) : ObservableObject
    {
        [ObservableProperty] private double _x = x;
        [ObservableProperty] private double _y = y;
        internal GroupItem? Parent { get; set; }
    }


    public partial class GroupItem : Item
    {
        public ObservableCollection<Item> Children { get; set; } = [];


        public GroupItem(double x, double y) : base(x, y)
        {
            Children.CollectionChanged += Children_CollectionChanged;
        }

        private void Children_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null && e.NewItems[0] is Item item) { item.Parent = this; }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems != null && e.OldItems[0] is Item item) { item.Parent = null; }
            }
        }

        public void AddChild(Item item)
        {
            // 循環参照にならないようにチェックしてから追加
            // 循環参照になる例：自分自身を追加、自分の親要素を追加
            if (CanAddChild(this, item)) { Children.Add(item); }
        }

        private bool CanAddChild(GroupItem potentialParent, Item targetItem)
        {
            // 1. 直近の自分自身チェック
            if (ReferenceEquals(potentialParent, targetItem)) return false;

            // 2. 先祖を遡ってチェック（targetItemがpotentialParentの親になっていないか）
            Item? current = potentialParent;
            while (current != null)
            {
                if (ReferenceEquals(current, targetItem))
                {
                    // targetItemはpotentialParentの先祖なので、
                    // potentialParentの中に入れると循環参照になる
                    return false;
                }
                // 親を遡る（Parentプロパティが必要）
                current = current?.Parent;
            }

            return true;
        }
    }

    public partial class TextBlockItem(double x, double y, string text) : Item(x, y)
    {
        [ObservableProperty] private string _text = text;

    }

    public partial class RectangleItem : Item
    {
        [ObservableProperty] private double _width;
        [ObservableProperty] private double _height;

        public RectangleItem(double x, double y, double width, double height) : base(x, y)
        {
            _width = width;
            _height = height;
            //Fill.Freeze();
        }

        //[JsonConverter(typeof(SolidColorBrushConverter))] // Brushとstringの自作変換クラスを指定
        //public SolidColorBrush Fill { get; set; } = Brushes.Maroon;

        [ObservableProperty]
        [property: JsonConverter(typeof(SolidColorBrushConverter))]// Brushとstringの自作変換クラスを指定
        private SolidColorBrush _fill = Brushes.Maroon;
    }

}
