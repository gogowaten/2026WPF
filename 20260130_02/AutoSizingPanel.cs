using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace _20260130_02
{
    /// <summary>
    /// 子要素の絶対座標(Left/Top)とサイズ(DesiredSize)に基づいて自身のサイズを決め、
    /// 親が Canvas の場合は自身の Canvas.Left/Top を更新して子をラップする Panel。
    /// 子要素には local:AutoSizingPanel.Left / Top を指定します。
    /// </summary>
    public class AutoSizingPanel : Panel
    {
        public static readonly DependencyProperty LeftProperty =
            DependencyProperty.RegisterAttached(
                "Left", typeof(double), typeof(AutoSizingPanel),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange));

        public static readonly DependencyProperty TopProperty =
            DependencyProperty.RegisterAttached(
                "Top", typeof(double), typeof(AutoSizingPanel),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange));

        public static void SetLeft(UIElement element, double value) => element.SetValue(LeftProperty, value);
        public static double GetLeft(UIElement element) => (double)element.GetValue(LeftProperty);

        public static void SetTop(UIElement element, double value) => element.SetValue(TopProperty, value);
        public static double GetTop(UIElement element) => (double)element.GetValue(TopProperty);

        // 最後に計算した最小座標（Arrange で子を相対配置するために使用）
        private double _minX = 0;
        private double _minY = 0;

        protected override Size MeasureOverride(Size availableSize)
        {
            double minX = double.PositiveInfinity;
            double minY = double.PositiveInfinity;
            double maxX = double.NegativeInfinity;
            double maxY = double.NegativeInfinity;

            foreach (UIElement child in InternalChildren)
            {
                if (child == null) continue;

                // 子は自身の DesiredSize を使うために制約を緩く渡す
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double left = GetLeft(child);
                double top = GetTop(child);
                double w = child.DesiredSize.Width;
                double h = child.DesiredSize.Height;

                minX = Math.Min(minX, left);
                minY = Math.Min(minY, top);
                maxX = Math.Max(maxX, left + w);
                maxY = Math.Max(maxY, top + h);
            }

            if (InternalChildren.Count == 0 || double.IsInfinity(minX))
            {
                _minX = 0;
                _minY = 0;
                return new Size(0, 0);
            }

            _minX = minX;
            _minY = minY;

            double desiredWidth = Math.Max(0, maxX - minX);
            double desiredHeight = Math.Max(0, maxY - minY);

            return new Size(desiredWidth, desiredHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                if (child == null) continue;

                double left = GetLeft(child);
                double top = GetTop(child);
                double x = left - _minX;
                double y = top - _minY;
                double w = child.DesiredSize.Width;
                double h = child.DesiredSize.Height;

                child.Arrange(new Rect(x, y, w, h));
            }

            // 親が Canvas の場合、自分自身を最小座標へ移動する（重複更新を避ける）
            var parent = VisualTreeHelper.GetParent(this) as FrameworkElement;
            if (parent is Canvas)
            {
                // Canvas.Left / Top が未設定 (NaN) の場合は 0 として扱う比較を行う
                double currLeft = Canvas.GetLeft(this);
                double currTop = Canvas.GetTop(this);
                if (double.IsNaN(currLeft)) currLeft = 0;
                if (double.IsNaN(currTop)) currTop = 0;

                if (!AreClose(currLeft, _minX))
                    Canvas.SetLeft(this, _minX);
                if (!AreClose(currTop, _minY))
                    Canvas.SetTop(this, _minY);
            }

            return finalSize;
        }

        private static bool AreClose(double a, double b)
            => Math.Abs(a - b) < 0.5; // 微小差分は無視（必要なら調整）
    }
}