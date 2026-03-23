using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace _20260321
{
    public class BezierAdorner : Adorner
    {
        private readonly VisualCollection _visualChildren;
        private readonly List<Thumb> _thumbs = new();
        private readonly BezierThumb _adornedElement;

        public BezierAdorner(UIElement adornedElement) : base(adornedElement)
        {
            _adornedElement = (BezierThumb)adornedElement;
            _visualChildren = new VisualCollection(this);

            // Pointsの数だけ操作用Thumbを作成
            for (int i = 0; i < _adornedElement.Points.Count; i++)
            {
                var thumb = CreateHandle(i);
                _thumbs.Add(thumb);
                _visualChildren.Add(thumb);
            }
        }

        private Thumb CreateHandle(int index)
        {
            var thumb = new Thumb
            {
                Width = 10,
                Height = 10,
                Cursor = Cursors.Hand,
                Background = Brushes.Red,
                Tag = index, // インデックスを保持
                Template = CreateThumbTemplate()
            };

            thumb.DragDelta += (s, e) =>
            {
                int idx = (int)((Thumb)s).Tag;
                Point p = _adornedElement.Points[idx];
                // 座標を更新
                _adornedElement.Points[idx] = new Point(p.X + e.HorizontalChange, p.Y + e.VerticalChange);
                // 親の再描画とアドーナ自身の再配置
                _adornedElement.InvalidateVisual();
                InvalidateArrange();
            };
            return thumb;
        }

        // ハンドルの見た目を丸くする
        private ControlTemplate CreateThumbTemplate()
        {
            var ellipseFactory = new FrameworkElementFactory(typeof(Ellipse));

            // SetValue メソッドを使用してプロパティをセットします
            ellipseFactory.SetValue(Shape.FillProperty, Brushes.Red);
            ellipseFactory.SetValue(Shape.StrokeProperty, Brushes.White);
            ellipseFactory.SetValue(Shape.StrokeThicknessProperty, 1.0);

            return new ControlTemplate(typeof(Thumb))
            {
                VisualTree = ellipseFactory
            };
        }

        // 子要素（Thumb）の配置
        protected override Size ArrangeOverride(Size finalSize)
        {
            for (int i = 0; i < _thumbs.Count; i++)
            {
                Point p = _adornedElement.Points[i];
                // ターゲット（MyBezierThumb）内の相対座標に配置
                _thumbs[i].Arrange(new Rect(p.X - 5, p.Y - 5, 10, 10));
            }
            return finalSize;
        }

        protected override int VisualChildrenCount => _visualChildren.Count;
        protected override Visual GetVisualChild(int index) => _visualChildren[index];
    }
}