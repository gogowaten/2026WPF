using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace _20260210_tuduki_mvvm
{
    internal class Class1 { }

    // 1. ハンドルの位置を定義
    public enum HandleLocation { TopLeft, TopRight, BottomLeft, BottomRight }

    // Viewになるみたい

    // Thumb を継承したカスタムコントロールです。UIイベントをModelに伝えるだけに徹します。
    // 2. ドラッグ移動可能なコントロール
    public class DraggableRectangle : Thumb
    {
        #region プロパティ

        private ResizingAdorner? _resizer; // Adornerを保持

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // 選択状態を管理する依存関係プロパティ
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(DraggableRectangle), new PropertyMetadata(false, OnIsSelectedChanged));

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as DraggableRectangle;
            if (control?._resizer != null)
            {
                control._resizer.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        #endregion プロパティ

        public DraggableRectangle()
        {
            // ドラッグ移動中の処理
            this.DragDelta += (s, e) =>
            {
                if (this.DataContext is RectModel model)
                {
                    model.RequestMove(e.HorizontalChange, e.VerticalChange);
                }
            };

            // ロード時にAdorner(リサイズハンドル)を表示
            this.Loaded += (s, e) =>
            {
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
                if (layer != null)
                {
                    _resizer = new ResizingAdorner(this)
                    {
                        Visibility = Visibility.Collapsed // 初期状態は非表示
                    };
                    layer.Add(_resizer);
                }
            };

            // 左クリックで選択状態にする
            this.PreviewMouseDown += (s, e) =>
            {
                //// Keyboard.Modifiers 修飾キーの状態
                //// ModifierKeys.Control Ctrlキーの定数,内部的には特定のビットが 1 になっている数値
                //// この2つをビット演算、AND

                //// ?? はnull合体演算子
                //// ざっくり言うと、**「左側が null だったら、右側の値を使う」**という予備の値を指定するためのルールです。
                //// Enumerable.Empty<DraggableRectangle>()は空のリストのようなものを返します。

                //// Ctrlキーが押されていない場合、かつ自分がまだ選択されていない場合のみ他の選択を解除する
                //if ((Keyboard.Modifiers & ModifierKeys.Control) == 0 && !this.IsSelected)
                //{
                //    var canvas = GetParentCanvas(this);
                //    foreach (var child in canvas?.Children.OfType<DraggableRectangle>() ?? Enumerable.Empty<DraggableRectangle>())
                //    {
                //        child.IsSelected = false;
                //    }
                //}
                //this.issele

                //this.IsSelected = true; // 自分を選択状態にする
                //e.Handled = false; // ドラッグ移動（Thumbの標準挙動）も行いたいのでHandledはfalse

                if(this.DataContext is RectModel model)
                {
                    // Ctrlキーが押されていない場合、かつ自分がまだ選択されていない場合のみ他の選択を解除する
                    if ((Keyboard.Modifiers & ModifierKeys.Control) == 0 && !model.IsSelected)
                    {
                        
                    }

                        model.IsSelected = true;
                    e.Handled = false;
                }
            };


        }

        private int GetMaxZIndex(Panel parent)
        {
            var zIndices = parent.Children.Cast<UIElement>().Select(Panel.GetZIndex);
            return zIndices.Any() ? zIndices.Max() : 0;

        }

        // ヘルパーメソッド：VisualTreeを遡ってcanvasを探す
        private Canvas GetParentCanvas(DependencyObject child)
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject is Canvas canvas) { return canvas; }
            else { return GetParentCanvas(parentObject); }
        }

    }



    // リサイズハンドル用のAdorner
    public class ResizingAdorner : Adorner
    {
        private VisualCollection _visuals;
        protected override int VisualChildrenCount => _visuals.Count;
        protected override Visual GetVisualChild(int index) => _visuals[index];

        public ResizingAdorner(UIElement adornerElement) : base(adornerElement)
        {
            _visuals = new VisualCollection(this);

            // 各角のハンドルを生成
            foreach (HandleLocation loc in Enum.GetValues(typeof(HandleLocation)))
            {
                var thumb = new Thumb
                {
                    Tag = loc,
                    Width = 10,
                    Height = 10,
                    Background = Brushes.White,
                    BorderBrush = Brushes.Blue,
                    BorderThickness = new Thickness(1),
                    Cursor = GetCursor(loc)
                };
                thumb.DragDelta += OnResizeDragDelta;
                _visuals.Add(thumb);
            }
        }

        // ドラッグ移動
        private void OnResizeDragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = (Thumb)sender;
            var el = (FrameworkElement)AdornedElement;
            var loc = (HandleLocation)thumb.Tag;

            double dLeft = 0, dTop = 0, dWidth = 0, dHeight = 0;

            switch (loc)
            {
                case HandleLocation.TopLeft:
                    dWidth = -e.HorizontalChange; dHeight = -e.VerticalChange;
                    dLeft = e.HorizontalChange; dTop = e.VerticalChange;
                    break;
                case HandleLocation.TopRight:
                    dWidth = e.HorizontalChange; dHeight = -e.VerticalChange;
                    dTop = e.VerticalChange;
                    break;
                case HandleLocation.BottomLeft:
                    dWidth = -e.HorizontalChange; dHeight = e.VerticalChange;
                    dLeft = e.HorizontalChange;
                    break;
                case HandleLocation.BottomRight:
                    dWidth = e.HorizontalChange; dHeight = e.VerticalChange;
                    break;
            }
            // サイズ変更
            double newWidth = Math.Max(el.ActualWidth + dWidth, 20);
            double actualDWidth = newWidth - el.ActualWidth;
            double newHeight = Math.Max(el.ActualHeight + dHeight, 20);
            double actualDHeight = newHeight - el.ActualHeight;
            //el.Width = newWidth;
            //el.Height = newHeight;

            //// 位置更新(サイズが実際に変わった分だけ座標をずらす)
            //if (dLeft != 0) { Canvas.SetLeft(el, Canvas.GetLeft(el) - actualDWidth); }
            //if (dTop != 0) { Canvas.SetTop(el, Canvas.GetTop(el) - actualDHeight); }
            // 位置更新(サイズが実際に変わった分だけ座標をずらす)
            if (DataContext is MainViewModel viewModel)
            {

                //if (viewModel.re is RectModel model)
                //{
                //    model.X -= actualDWidth;
                //    model.Y -= actualDHeight;
                //}
            }
        }

        // 子要素の再配置
        protected override Size ArrangeOverride(Size finalSize)
        {
            double w = AdornedElement.RenderSize.Width;
            double h = AdornedElement.RenderSize.Height;
            foreach (Thumb thumb in _visuals.Cast<Thumb>())
            {
                var loc = (HandleLocation)thumb.Tag;
                double x = (loc == HandleLocation.TopLeft || loc == HandleLocation.BottomLeft) ? -5 : w - 5;
                double y = (loc == HandleLocation.TopLeft || loc == HandleLocation.TopRight) ? -5 : h - 5;
                thumb.Arrange(new Rect(x, y, 10, 10));
            }
            return finalSize;
            //return base.ArrangeOverride(finalSize);
        }

        private Cursor GetCursor(HandleLocation loc) => loc switch
        {
            HandleLocation.TopLeft or HandleLocation.BottomRight => Cursors.SizeNWSE,
            _ => Cursors.SizeNESW
        };

        // いずれかのハンドルがドラッグ移動中の判定
        public bool IsDragging()
        {
            foreach (Thumb thumb in _visuals)
            {
                if (thumb.IsDragging) return true;
            }
            return false;
        }
    }


}

