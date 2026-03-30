using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;

namespace _20260330
{
    public class ResizeAdorner : Adorner
    {
        // ハンドルとしてのThumb
        private readonly Thumb _buttonRight;
        private readonly VisualCollection _visualChildren;

        public ResizeAdorner(UIElement adornedElement) : base(adornedElement)
        {
            _visualChildren = new VisualCollection(this);
            _buttonRight = new Thumb() { Width = 10, Height = 10, Background = Brushes.Red };

            _buttonRight.DragDelta += OnResize;
            _visualChildren.Add(_buttonRight);
        }

        private void OnResize(object sender, DragDeltaEventArgs e)
        {
            if(AdornedElement is FrameworkElement element)
            {
                // 要素のサイズを更新
                if (element.Width + e.HorizontalChange > 10)
                {
                    element.Width += e.HorizontalChange;
                }

                if (element.Height + e.VerticalChange > 10)
                {
                    element.Height += e.VerticalChange;
                }
            }
        }

        // 配置の決定（Thumbを右下に配置）

        protected override Size ArrangeOverride(Size finalSize)
        {
            // Q 1度の動作に2回ArrangeOverrideが処理されているのはなんで？
            // A 1回目：子要素（Thumb等）の再配置
            // 2回目：子要素の再配置により親要素（AdornerElement）の再配置が必要

            //return base.ArrangeOverride(finalSize);
            _buttonRight.Arrange(new Rect(finalSize.Width - 5, finalSize.Height - 5, 10, 10));
            return finalSize;
        }

        // Visualの子要素をフレームワークに教えるための定型文
        protected override int VisualChildrenCount => _visualChildren.Count;
        protected override Visual GetVisualChild(int index) => _visualChildren[index];

    }
}
