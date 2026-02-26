using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20260224
{
    // ItemsControlを継承
    /*    public class DiagramCanvas : ItemsControl
        {
            static DiagramCanvas()
            {
                DefaultStyleKeyProperty.OverrideMetadata(typeof(DiagramCanvas), new FrameworkPropertyMetadata(typeof(DiagramCanvas)));
            }

            public DiagramCanvas()
            {
                // ItemsSource が変わったときに通知を受け取る
                var dpd = DependencyPropertyDescriptor.FromProperty(ItemsSourceProperty, typeof(DiagramCanvas));
                dpd.AddValueChanged(this, OnItemsSourceChanged);

            }


            private void OnItemsSourceChanged(object? sender, EventArgs e)
            {
                if (ItemsSource is INotifyCollectionChanged collection)
                {
                    // アイテムが追加・削除されたら再計算
                    collection.CollectionChanged -= OnItemsListChanged;
                    collection.CollectionChanged += OnItemsListChanged;
                }
                UpdateSize();
            }

            private void OnItemsListChanged(object? sender, NotifyCollectionChangedEventArgs e)
            {
                // 1. 新しく追加されたアイテムの移動（X, Y）を監視する
                if (e.NewItems != null)
                {
                    foreach (Item item in e.NewItems)
                        item.PropertyChanged += OnItemPropertyChanged;
                }

                // 2. 削除されたアイテムの監視を外す（メモリリーク防止）
                if (e.OldItems != null)
                {
                    foreach (Item item in e.OldItems)
                        item.PropertyChanged -= OnItemPropertyChanged;
                }

                UpdateSize();
            }
            private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                // X, Y, Width, Height が変わったらサイズを再計算
                if (e.PropertyName == "X" || e.PropertyName == "Y" ||
                    e.PropertyName == "Width" || e.PropertyName == "Height")
                {
                    UpdateSize();
                }
            }

            private void UpdateSize()
            {
                if (ItemsSource == null) return;

                var items = ItemsSource.Cast<Item>();
                var size = BoundsCalculator.GetTotalSize(items);

                // 自分自身の Width / Height を更新
                this.Width = size.Width;
                this.Height = size.Height;
            }



        }
    */


    // Canvas継承よりItemsControl継承が正義
    public class DiagramBoard : ItemsControl
    {
        static DiagramBoard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DiagramBoard),
                new FrameworkPropertyMetadata(typeof(DiagramBoard)));
        }

        // ここで直接 Items(Model) を受け取る
        public Items? TargetItems
        {
            get => (Items)GetValue(TargetItemsProperty);
            set => SetValue(TargetItemsProperty, value);
        }

        public static readonly DependencyProperty TargetItemsProperty =
            DependencyProperty.Register(nameof(TargetItems), typeof(Items), typeof(DiagramBoard),
                new PropertyMetadata(null, OnTargetItemsChanged));

        private static void OnTargetItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var board = (DiagramBoard)d;
            if (e.NewValue is Items newItems)
            {
                // データの紐付け
                board.ItemsSource = newItems.Children;

                // サイズの紐付け（Modelの計算結果がそのままBoardのサイズになる）
                board.SetBinding(WidthProperty, new Binding("Width") { Source = newItems });
                board.SetBinding(HeightProperty, new Binding("Height") { Source = newItems});
            }
        }
    }





}
