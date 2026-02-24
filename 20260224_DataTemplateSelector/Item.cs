using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace _20260224_Json
{
    // この全部入りパターンは良くないとAIの判断、
    // 理由は、DataTemplateSelectorの自作が必須、使わないプロパティが増える
    /*    public enum ItemType { Items, TextBlock, Rectangle }
        public partial class Item(double x, double y) : ObservableObject
        {
            public ItemType ItemType { get; private set; }
            [ObservableProperty] private double _x = x;
            [ObservableProperty] private double _y = y;
            [ObservableProperty] private double _width;
            [ObservableProperty] private double _height;
            [ObservableProperty] private string _text = string.Empty;
            [ObservableProperty] private Brush _fill = Brushes.Black;
        }*/



    public enum ItemType { Items=0, TextBlock, Rectangle, Image }


    /*継承パターン*/
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")] // JSON上の識別子名。既存のTypeプロパティと被る場合は名前を変えるか調整
    [JsonDerivedType(typeof(Items), typeDiscriminator: nameof(Items))]
    [JsonDerivedType(typeof(TextBlockItem), typeDiscriminator: nameof(TextBlockItem))]
    [JsonDerivedType(typeof(RectangleItem), typeDiscriminator: nameof(RectangleItem))]
    public abstract partial class Item(double x, double y) : ObservableObject
    {
        [ObservableProperty] private bool _canDrag = true; // ドラッグ移動の可否判定用

        [JsonIgnore]
        public abstract ItemType ItemType { get; }
        [ObservableProperty] private double _x = x;
        [ObservableProperty] private double _y = y;
    }


    public partial class Items(double x, double y) : Item(x, y)
    {
        public override ItemType ItemType => ItemType.Items;

        // [JsonInclude]はデシリアライズで必要になる、効果はprivate set;のプロパティでもデシリアライズの時だけはsetを許可してデシリアライズを正しく処理する
        [JsonInclude]
        public ObservableCollection<Item> Children { get; private set; } = [];
    }

    //public partial class ImageItem(double x, double y, BitmapSource bmp) : Item(ItemType.Image, x, y)
    //{
    //    [ObservableProperty] private BitmapSource _image = bmp;
    //}

    // シリアライズするためには、コンストラクタの引数名はプロパティ名と完全に一致させる必要がある
    public partial class TextBlockItem(double x, double y, string text) : Item(x, y)
    {
        [ObservableProperty] private string _text = text; // 完全に一致させる

        public override ItemType ItemType => ItemType.TextBlock;
    }

    public partial class RectangleItem(double x, double y, string fill, double width, double height) : Item(x, y)
    {
        [ObservableProperty] private string _fill = fill.ToString();
        [ObservableProperty] private double _width = width;
        [ObservableProperty] private double _height = height;

        public override ItemType ItemType => ItemType.Rectangle;
    }

}
