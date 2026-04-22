using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace _20260420
{

    public class VertexAdorner2 : Adorner
    {
        protected override int VisualChildrenCount => _visuals.Count;
        protected override Visual GetVisualChild(int index) => _visuals[index];

        private readonly VisualCollection _visuals;
        private readonly GeoLine _adornedElement;
        private readonly Canvas MyCanvas;

        public VertexAdorner2(UIElement adornedElement) : base(adornedElement)
        {
            _adornedElement = (GeoLine)adornedElement;
            _visuals = new(this);
            MyCanvas = new Canvas();
            _visuals.Add(MyCanvas);

            // 頂点の数だけハンドルを作成
            UpdateHandles();
        }

        public void UpdateHandles()
        {
            MyCanvas.Children.Clear();

            var points = _adornedElement.MyPoints;
            if (points == null) { return; }

            for (int i = 0; i < points.Count; i++)
            {
                var thumb = new Thumb()
                {
                    Width = 10,
                    Height = 10,
                    Background = Brushes.Red,
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(1),
                    Cursor = Cursors.Hand,
                    Tag = i // インデックスを保持
                };

                Canvas.SetLeft(thumb, points[i].X - 5);
                Canvas.SetTop(thumb, points[i].Y - 5);

                thumb.DragDelta += Thumb_DragDelta;
                _ = MyCanvas.Children.Add(thumb);
            }
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is Thumb thumb && thumb.Tag is int index)
            {
                var points = _adornedElement.MyPoints;
                if (points != null && index < points.Count)
                {
                    Point p = points[index];
                    // 頂点座標を更新
                    points[index] = new Point(p.X + e.HorizontalChange, p.Y + e.VerticalChange);
                    SyncThumbPosition(index, points[index]);
                    //Canvas.SetLeft(thumb, p.X - 5 + e.HorizontalChange);
                    //Canvas.SetTop(thumb, p.Y - 5 + e.VerticalChange);
                }
                e.Handled = true;
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            MyCanvas.Arrange(new Rect(finalSize));
            return base.ArrangeOverride(finalSize);
        }

        
        private void SyncThumbPosition(int index, Point p)
        {
            var thumb = MyCanvas.Children[index] as Thumb;
            Canvas.SetLeft(thumb, p.X - 5);
            Canvas.SetTop(thumb, p.Y - 5);
        }

    }




    public class VertexAdorner : Adorner
    {
        protected override int VisualChildrenCount => _visuals.Count;
        protected override Visual GetVisualChild(int index) => _visuals[index];

        private readonly VisualCollection _visuals;
        private readonly GeoLine _adornedElement;

        public VertexAdorner(UIElement adornedElement) : base(adornedElement)
        {
            _adornedElement = (GeoLine)adornedElement;
            _visuals = new(this);

            // 頂点の数だけハンドルを作成
            UpdateHandles();
        }

        public void UpdateHandles()
        {
            _visuals.Clear();
            var points = _adornedElement.MyPoints;
            if (points == null) { return; }

            for (int i = 0; i < points.Count; i++)
            {
                var thumb = new Thumb()
                {
                    Width = 10,
                    Height = 10,
                    Background = Brushes.Red,
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(1),
                    Cursor = Cursors.Hand,
                    Tag = i // インデックスを保持
                };

                thumb.DragDelta += Thumb_DragDelta;
                _visuals.Add(thumb);
            }
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is Thumb thumb && thumb.Tag is int index)
            {
                var points = _adornedElement.MyPoints;
                if (points != null && index < points.Count)
                {
                    Point p = points[index];
                    // 頂点座標を更新
                    points[index] = new Point(p.X + e.HorizontalChange, p.Y + e.VerticalChange);
                }
                e.Handled = true;
            }
        }

        // 配置の制御
        protected override Size ArrangeOverride(Size finalSize)
        {
            var points = _adornedElement.MyPoints;
            if (points is null) { return finalSize; }

            for (int i = 0; i < points.Count; i++)
            {
                if (_visuals[i] is Thumb thumb && i < points.Count)
                {
                    // ハンドルの中心が頂点に来るように配置
                    double left = points[i].X - (thumb.Width / 2);
                    double top = points[i].Y - (thumb.Height / 2);
                    thumb.Arrange(new Rect(left, top, thumb.Width, thumb.Height));
                }
            }
            return finalSize;
            //return base.ArrangeOverride(finalSize);
        }
    }
}
