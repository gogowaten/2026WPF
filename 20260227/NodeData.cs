using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Transactions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Collections.Specialized;

namespace _20260227
{
    public abstract partial class Data : ObservableObject
    {
        [ObservableProperty] private double _x;
        [ObservableProperty] private double _y;
        [ObservableProperty] private double _z;
        [ObservableProperty] private double _width;
        [ObservableProperty] private double _height;
        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private Rect _externalRect;
        [ObservableProperty] private bool _isSelected;
        [ObservableProperty] private bool _isEditing;
        //[ObservableProperty] private bool _isDragging;
        //[ObservableProperty] private bool _isMoving;
        //[ObservableProperty] private bool _isUpdating;
        [ObservableProperty] private Rect _contentBounds;



    }

    public partial class Datas : Data
    {
        public ObservableCollection<Data> Nodes { get; set; } = [];
        public Datas(double x, double y)
        {
            this.X = x;
            this.Y = y;

            Nodes.CollectionChanged += OnNodesCollectionChanged;
        }

        // 要素の追加削除時
        private void OnNodesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // 追加時、プロパティ変更を購読
            if (e.NewItems != null)
            {
                foreach (Data node in e.NewItems)
                {
                    node.PropertyChanged += OnNodePropertyChanged;
                }
            }

            // 削除時、購読解除
            if (e.OldItems != null)
            {
                foreach (Data node in e.OldItems)
                {
                    node.PropertyChanged -= OnNodePropertyChanged;
                }
            }
            UpdateBounds();
        }

        private void OnNodePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // どんなプロパティが変わっても、とりあえずログを出す
            System.Diagnostics.Debug.WriteLine($"Property Changed: {sender?.GetType().Name} -> {e.PropertyName}");


            // 子要素の X, Y, Width, Height が変わったら自分を更新
            // ※ e.PropertyName が null または空文字の場合も、全プロパティ更新通知なので反応するようにする
            if (string.IsNullOrEmpty(e.PropertyName) ||
                e.PropertyName is nameof(X) or nameof(Y) or nameof(Width) or nameof(Height))
            {
                UpdateBounds();

                // デバッグ用：出力ウィンドウを確認してください
                System.Diagnostics.Debug.WriteLine($"UpdateBounds: {this.GetType().Name} resized to {Width}x{Height}");
            }

        }

        // 子要素が動くたびにこれを呼ぶ
        public void UpdateBounds()
        {
            if (Nodes.Count == 0)
            {
                ContentBounds = Rect.Empty;
                this.Width = 0;
                this.Height = 0;
                return;
            }

            // 子要素全体が収まるRect計算
            double minX = Nodes.Min(x => x.X);
            double minY = Nodes.Min(y => y.Y);
            double maxX = Nodes.Max(x => x.X + x.Width);
            double maxY = Nodes.Max(y => y.Y + y.Height);
            ContentBounds = new Rect(minX, minY, maxX - minX, maxY - minY);

            // 自身のサイズも更新
            this.Width = ContentBounds.Width;
            this.Height = ContentBounds.Height;

            // ここで ExternalRect も再計算する（回転などを考慮して）
            // UpdateExternalRect(); 
        }

        public void AddNodeData(Data node)
        {
            Nodes.Add(node);
        }
    }

    public partial class TextBlockData : Data
    {
        [ObservableProperty] private string _text = string.Empty;
        public TextBlockData(double x, double y, string text)
        {
            this.X = x;
            this.Y = y;
            this.Text = text;
            //this.Width = 100;
            //this.Height = 100;
        }
    }

    public partial class RectangleData : Data
    {
        public RectangleData(double x, double y, double width, double height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }
    }



}
