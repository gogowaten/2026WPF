using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

// 自動リサイズだけど、ActualWidthとActualHeightが自動なだけで
// WidthとHeightはNaNのままなので
// GridやScrollViewerに配置する場合は、
// HorizontalAlignmentとVerticalAlignmentをStretch以外に指定する必要がある、
// もししなかった場合は全体に広がる
// Canvasに配置した場合は普通に表示される
namespace _20260221_AutoSizePanel
{
    // 自動リサイズパネル
    public class NodePanel : Panel
    {
        #region 添付プロパティ
        
        // X座標
        public static double GetX(DependencyObject obj)
        {
            return (double)obj.GetValue(XProperty);
        }

        public static void SetX(DependencyObject obj, double value)
        {
            obj.SetValue(XProperty, value);
        }

        public static readonly DependencyProperty XProperty =
                    DependencyProperty.RegisterAttached("X", typeof(double), typeof(NodePanel),
                        new FrameworkPropertyMetadata(0.0,
                            FrameworkPropertyMetadataOptions.AffectsParentMeasure));
        // FrameworkPropertyMetadataOptions.AffectsParentMeasure
        // 値の変更があったときに親要素のinvalidMeasureを実行する


        public static double GetY(DependencyObject obj)
        {
            return (double)obj.GetValue(YProperty);
        }

        public static void SetY(DependencyObject obj, double value)
        {
            obj.SetValue(YProperty, value);
        }

        public static readonly DependencyProperty YProperty =
            DependencyProperty.RegisterAttached("Y", typeof(double), typeof(NodePanel),
                new FrameworkPropertyMetadata(0.0, 
                    FrameworkPropertyMetadataOptions.AffectsParentMeasure));
        #endregion 添付プロパティ

        
        // すべての子要素が収まるサイズを測定
        protected override Size MeasureOverride(Size availableSize)
        {
            double maxX = 0;
            double maxY = 0;
            foreach (UIElement child in InternalChildren)
            {
                // 子要素のサイズを測定、これで子要素のDesiredSizeが更新される
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                // 子要素に添付した添付プロパティから、XY座標取得
                double posX = GetX(child);// Canvas.GetLeft(child);
                double posY = GetY(child);
                //if (double.IsNaN(posX)) { posX = 0; }
                maxX = Math.Max(maxX, posX + child.DesiredSize.Width);
                maxY = Math.Max(maxY, posY + child.DesiredSize.Height);
            }
            return new Size(maxX, maxY);
        }

        // すべての子要素を再配置する
        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                double posX = GetX(child);
                double posY = GetY(child);
                child.Arrange(new Rect(new Point(posX, posY), child.DesiredSize));
            }
            return finalSize;
        }
    }

}
