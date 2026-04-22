using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace _20260420
{

    public class VertexAdorner : Adorner
    {
        protected override int VisualChildrenCount => _visuals.Count;
        protected override Visual GetVisualChild(int index) => _visuals[index];

        private readonly VisualCollection _visuals;
        private readonly GeoLine _adornedElement;
        private readonly Canvas MyCanvas;
        private double MyHandleOffset;

        public VertexAdorner(UIElement adornedElement) : base(adornedElement)
        {
            _adornedElement = (GeoLine)adornedElement;
            _visuals = new(this);
            MyCanvas = new Canvas();
            _visuals.Add(MyCanvas);
            MyHandleOffset = MyHandleSize / 2.0;

            // 頂点の数だけハンドルを作成
            UpdateHandles();
        }

        

        #region プロパティ

        public double MyHandleSize
        {
            get { return (double)GetValue(MyHandleSizeProperty); }
            set { SetValue(MyHandleSizeProperty, value); }
        }
        public static readonly DependencyProperty MyHandleSizeProperty =
            DependencyProperty.Register(nameof(MyHandleSize), typeof(double), typeof(VertexAdorner), new PropertyMetadata(20.0, OnMyHandleSizeChanged));

        private static void OnMyHandleSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VertexAdorner ador)
            {
                ador.MyHandleOffset = (double)e.NewValue / 2.0;
            }
        }

        #endregion プロパティ

        public void UpdateHandles()
        {
            MyCanvas.Children.Clear();

            var points = _adornedElement.MyPoints;
            if (points == null) { return; }

            for (int i = 0; i < points.Count; i++)
            {
                var thumb = new Thumb()
                {
                    Background = Brushes.Red,
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(1),
                    Cursor = Cursors.Hand,
                    Tag = i // インデックスを保持
                };

                thumb.SetBinding(WidthProperty, new Binding() { Source = this, Path = new PropertyPath(MyHandleSizeProperty) });
                thumb.SetBinding(HeightProperty, new Binding() { Source = this, Path = new PropertyPath(MyHandleSizeProperty) });


                Canvas.SetLeft(thumb, points[i].X - MyHandleOffset);
                Canvas.SetTop(thumb, points[i].Y - MyHandleOffset);

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
                    // ハンドル位置更新
                    SyncThumbPosition(index, points[index]);
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
            Canvas.SetLeft(thumb, p.X - MyHandleOffset);
            Canvas.SetTop(thumb, p.Y - MyHandleOffset);
        }


    }




}
