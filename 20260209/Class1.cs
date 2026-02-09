using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace _20260209
{
    class Class1 { }


    // 1. ハンドルの位置を定義
    public enum HandleLocation { TopLeft, TopRight, BottomLeft, BottomRight }


    // 2. ドラッグ移動可能なコントロール
    public class DraggableRectangle : Thumb
    {
        private ResizingAdorner? _resizer; // Adornerを保持

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // 選択状態を管理する依存関係プロパティ
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(DraggableRectangle), new PropertyMetadata(false,OnIsSelectedChanged));

        private static void OnIsSelectedChanged(DependencyObject d,DependencyPropertyChangedEventArgs e)
        {
            var control = d as DraggableRectangle;
            if(control?._resizer != null)
            {
                control._resizer.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public DraggableRectangle()
        {
            this.DragDelta += (s, e) =>
            {
                double left = Canvas.GetLeft(this);
                double top = Canvas.GetTop(this);
                Canvas.SetLeft(this, (double.IsNaN(left) ? 0 : left) + e.HorizontalChange);
                Canvas.SetTop(this, (double.IsNaN(top) ? 0 : top) + e.VerticalChange);
            };

            // ロード時にAdorner(リサイズハンドル)を表示
            this.Loaded += (s, e) =>
            {
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
                if(layer != null)
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
                // 親（Canvasなど）から他のDraggableRectangleを探して非選択にする（簡易的な実装）
                var canvas = VisualTreeHelper.GetParent(this) as Panel;
                if(canvas != null)
                {
                    foreach (var child in canvas.Children.OfType<DraggableRectangle>())
                    {
                        child.IsSelected = false;
                    }
                }
                this.IsSelected = true;
                e.Handled = false; // ドラッグ移動（Thumbの標準挙動）も行いたいのでHandledはfalse
            };

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
            el.Width = newWidth;
            el.Height = newHeight;

            // 位置更新(サイズが実際に変わった分だけ座標をずらす)
            if (dLeft != 0) { Canvas.SetLeft(el, Canvas.GetLeft(el) - actualDWidth); }
            if (dTop != 0) { Canvas.SetTop(el, Canvas.GetTop(el) - actualDHeight); }
        }

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

