using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace _20260224
{
    [JsonDerivedType(typeof(TextBlockItem), nameof(TextBlockItem))]
    [JsonDerivedType(typeof(RectangleItem), nameof(RectangleItem))]
    [JsonDerivedType(typeof(Items), nameof(Items))]
    public abstract partial class Item(double x, double y) : ObservableObject
    {
        [ObservableProperty] private double _x = x;
        [ObservableProperty] private double _y = y;
        [ObservableProperty] private double _width = 0;
        [ObservableProperty] private double _height = 0;
        [ObservableProperty][NotifyPropertyChangedFor(nameof(Background))] private byte _backgroundA = 0;
        [ObservableProperty][NotifyPropertyChangedFor(nameof(Background))] private byte _backgroundR = 0;
        [ObservableProperty][NotifyPropertyChangedFor(nameof(Background))] private byte _backgroundG = 0;
        [ObservableProperty][NotifyPropertyChangedFor(nameof(Background))] private byte _backgroundB = 0;

        // View用のBrushをARGBから生成
        [JsonIgnore]
        public SolidColorBrush Background
        {
            get
            {
                return new(Color.FromArgb(BackgroundA, BackgroundR, BackgroundG, BackgroundB));
            }
            set
            {
                BackgroundA = value.Color.A;
                BackgroundR = value.Color.R;
                BackgroundG = value.Color.G;
                BackgroundB = value.Color.B;
            }
        }

    


        partial void OnWidthChanged(double value) => Parent?.UpdateBounds();
        partial void OnHeightChanged(double value) => Parent?.UpdateBounds();
        partial void OnXChanged(double value) => Parent?.UpdateBounds();
        partial void OnYChanged(double value) => Parent?.UpdateBounds();
        [JsonIgnore] public virtual double Right => X + Width;
        [JsonIgnore] public virtual double Bottom => Y + Height;

        internal Items? Parent { get; set; }
    }


    public partial class Items : Item
    {
        [ObservableProperty] private double _totalWidth;
        [ObservableProperty] private double _totalHeight;

        public ObservableCollection<Item> Children { get; set; } = [];


        // DiagramBoard が直接受け取るデータソースとしての Items クラスに、サイズ計算を集約している
        public Items(double x, double y) : base(x, y)
        {
            Children.CollectionChanged += Children_CollectionChanged;
            Background = Brushes.Orange;
        }

        // 子要素追加削除時
        private void Children_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // 追加時
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // Parent設定
                if (e.NewItems != null && e.NewItems[0] is Item item)
                {
                    item.Parent = this;
                    UpdateBounds();
                }

            }
            // 削除時
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                // Parent削除
                if (e.OldItems != null && e.OldItems[0] is Item item) { item.Parent = null; UpdateBounds(); }
            }
            //UpdateBounds();

        }
        public void UpdateBounds()
        {
            if (Children.Count == 0) return;
            var (w, h) = BoundsCalculator.GetTotalSize(Children);
            //TotalWidth = w;
            //TotalHeight = h;
            Width = w;
            Height = h;
            Parent?.UpdateBounds();
        }

        // グループ自体のサイズ
        public override double Right => X + TotalWidth;
        public override double Bottom => Y + TotalHeight;
        //public void UpdateBounds()
        //{
        //    if (Children.Count == 0) { TotalWidth = 0; TotalHeight = 0; return; }

        //    double maxX = 0;
        //    double maxY = 0;

        //    foreach (var child in Children)
        //    {
        //        double w = 0, h = 0;
        //        if (child is RectangleItem r) { w = r.Width; h = r.Height; }
        //        else if (child is TextBlockItem t) { w = t.Width; h = t.Height; } // Textは概算か、ActualWidthが必要

        //        maxX = Math.Max(maxX, child.X + w);
        //        maxY = Math.Max(maxY, child.Y + h);
        //    }

        //    TotalWidth = maxX;
        //    TotalHeight = maxY;
        //}

        public void AddChild(Item item)
        {
            // 循環参照にならないようにチェックしてから追加
            // 循環参照になる例：自分自身を追加、自分の親要素を追加
            if (CanAddChild(this, item)) { Children.Add(item); }
        }

        private bool CanAddChild(Items potentialParent, Item targetItem)
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
        //[ObservableProperty] private double _width;
        //[ObservableProperty] private double _height;
    }

    public partial class RectangleItem : Item
    {
        //[ObservableProperty] private double _width;
        //[ObservableProperty] private double _height;

        public RectangleItem(double x, double y, double width, double height) : base(x, y)
        {
            //_width = width;
            //_height = height;
            Width= width;
            Height= height;
            BackgroundA = 255;
        }

        // [ObservableProperty]が必要なければ以下で良い
        //[JsonConverter(typeof(SolidColorBrushConverter))] // Brushとstringの自作変換クラスを指定
        //public SolidColorBrush Fill { get; set; } = Brushes.Maroon;

        //[ObservableProperty]
        //[property: JsonConverter(typeof(SolidColorBrushConverter))]// Brushとstringの自作変換クラスを指定
        //private SolidColorBrush _fill = Brushes.Maroon;

        // BrushそのものじゃなくてARGBに分ける場合
        // [NotifyPropertyChangedFor(nameof(FillBrush))]は変更通知

        // 下記は各プロパティに[NotifyPropertyChangedFor(nameof(FillBrush))]をつければ必要ない
        // ARGBいずれかの値が変化したときは、Brushの変更をViewに通知する
        //partial void OnAChanged(byte value) => OnPropertyChanged(nameof(A));
        //partial void OnRChanged(byte value) => OnPropertyChanged(nameof(R));
        //partial void OnGChanged(byte value) => OnPropertyChanged(nameof(G));
        //partial void OnBChanged(byte value) => OnPropertyChanged(nameof(B));
    }

}
