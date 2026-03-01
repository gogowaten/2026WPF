using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace _20260301
{
    // サイズ(WidthとHeight)を持つCanvas
    // サイズの更新は半自動
    // 自動で行われる状況は3つあり、子要素の追加、削除、子要素のDesiredSizeが変更された時
    public class ReCanvas : Canvas
    {
        public ReCanvas()
        {

        }

        protected override Size MeasureOverride(Size constraint)
        {
            foreach (var item in InternalChildren)
            {
                Console.WriteLine(item.ToString());
                var cp = (ContentPresenter)item;
                var cs = cp.ContentSource;
                var con = cp.Content;
                var ct = cp.ContentTemplate;
            }
            return base.MeasureOverride(constraint);
        }
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            return base.ArrangeOverride(arrangeSize);
        }

        // 子要素が追加、削除された時
        // 子要素のサイズを測定してから自身のサイズを更新する
        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            if (visualAdded is UIElement addUI) { addUI.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)); }
            if (visualRemoved is UIElement remUI) { remUI.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)); }
            UpdateSize();
        }

        // 子要素のDesiredSizeが変更された時
        // 自身のサイズを更新する
        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            base.OnChildDesiredSizeChanged(child);
            UpdateSize();
        }

        // 自身のサイズを「子要素がすべて収まるサイズ」に更新する、ゼロシフトとクリップは行わない
        // 条件：要素はCanvas.Leftと、Canvas.Topが指定されている
        public void UpdateSize()
        {
            double w = 0, h = 0;
            foreach (UIElement item in InternalChildren.OfType<UIElement>())
            {
                w = Math.Max(w, item.DesiredSize.Width + GetLeft(item));
                h = Math.Max(h, item.DesiredSize.Height + GetTop(item));
            }
            Width = w; Height = h;
        }

    }
}
