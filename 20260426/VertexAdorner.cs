using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace _20260426
{
    public class VertexAdorner : Adorner
    {
        protected override int VisualChildrenCount => _visuals.Count;
        protected override Visual GetVisualChild(int index) => _visuals[index];

        private readonly VisualCollection _visuals;
        private readonly GeoLine _adornedElement;
        private readonly Canvas MyCanvas;
        private double MyHandleOffset;
        private PointCollection MyGeoPoints;

        public VertexAdorner(UIElement adornedElement) : base(adornedElement)
        {
            this.UseLayoutRounding = true; // ドットに合わせてくっきり表示
            _adornedElement = (GeoLine)adornedElement;
            _visuals = new(this);
            MyCanvas = new Canvas();
            _visuals.Add(MyCanvas);
            MyHandleOffset = MyHandleSize / 2.0;
            if (_adornedElement is GeoLine geo)
            {
                MyGeoPoints = geo.MyPoints;
            }
            else
            {
                throw new InvalidOperationException("図形のPointsが見つからない");
            }

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
                // ハンドルサイズ変更に伴う変更、オフセット、全ハンドルの座標
                ador.MyHandleOffset = (double)e.NewValue / 2.0;
                var points = ador.MyGeoPoints;
                for (int i = 0; i < points.Count; i++)
                {
                    ador.SyncThumbPosition(i, points[i]);
                }
            }
        }

        #endregion プロパティ

        public void UpdateHandles()
        {
            MyCanvas.Children.Clear();

            if (MyGeoPoints == null) { return; }

            for (int i = 0; i < MyGeoPoints.Count; i++)
            {
                var thumb = new FlatHandle()
                {
                    Cursor = Cursors.Hand,
                    Tag = i, // インデックスを保持
                    MyFillBrush = new SolidColorBrush(Color.FromArgb(40, 255, 0, 0))
                };

                thumb.SetBinding(WidthProperty, new Binding() { Source = this, Path = new PropertyPath(MyHandleSizeProperty) });
                thumb.SetBinding(HeightProperty, new Binding() { Source = this, Path = new PropertyPath(MyHandleSizeProperty) });

                thumb.MyLeft = MyGeoPoints[i].X - MyHandleOffset;
                thumb.MyTop = MyGeoPoints[i].Y - MyHandleOffset;

                thumb.DragDelta += Thumb_DragDelta;
                _ = MyCanvas.Children.Add(thumb);
            }
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is Thumb thumb && thumb.Tag is int index)
            {
                //var points = _adornedElement.MyPoints;
                if (MyGeoPoints != null && index < MyGeoPoints.Count)
                {
                    Point p = MyGeoPoints[index];
                    // 頂点座標を更新
                    MyGeoPoints[index] = new Point(p.X + e.HorizontalChange, p.Y + e.VerticalChange);
                    // ハンドル位置更新
                    SyncThumbPosition(index, MyGeoPoints[index]);
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
            if (MyCanvas.Children[index] is FlatHandle thumb)
            {
                thumb.MyLeft = p.X - MyHandleOffset;
                thumb.MyTop = p.Y - MyHandleOffset;
            }

        }
    }



}