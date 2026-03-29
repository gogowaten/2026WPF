using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace _20260330
{
    public class FitCanvas : Canvas
    {
        public void UpdateSizeToContent()
        {
            if(InternalChildren.Count == 0)
            {
                this.Width = 0;
                this.Height = 0;
                return;
            }

            double right = double.MinValue;
            double bottom = double.MinValue;
            double top = double.MaxValue;
            double left = double.MaxValue;

            // 子要素全体が収まるサイズを計算
            foreach (UIElement child in InternalChildren)
            {
                // 子要素の座標を取得（未設定の場合は0で計算）
                double x = GetLeft(child);
                if (double.IsNaN(x)) { x = 0; }
                double y = GetTop(child);
                if (double.IsNaN(y)) { y = 0; }

                // 子要素の左端と上端を計算
                left = Math.Min(left, x);
                top = Math.Min(top, y);

                // 子要素のサイズを測定してから
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                // 右端と下端の取得
                right = Math.Max(right, x + child.DesiredSize.Width);
                bottom = Math.Max(bottom, y + child.DesiredSize.Height);
            }

            // 自身のサイズを更新
            this.Width = right - left;
            this.Height = bottom - top;
        }
    }
}
