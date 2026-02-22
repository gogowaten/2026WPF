using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace _20260222_ResizePanel
{
    public class ResizePanel : Panel
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

        //public static readonly DependencyProperty XProperty =
        //            DependencyProperty.RegisterAttached("X", typeof(double), typeof(ResizePanel),
        //                new PropertyMetadata(0.0
        //                     ));
        //// FrameworkPropertyMetadataOptions.AffectsParentMeasure
        //// 値の変更があったときに親要素のinvalidMeasureを実行する

        public static readonly DependencyProperty XProperty =
                    DependencyProperty.RegisterAttached("X", typeof(double), typeof(ResizePanel),
                        new FrameworkPropertyMetadata(0.0,
                             FrameworkPropertyMetadataOptions.AffectsParentArrange));

        //public static readonly DependencyProperty XProperty =
        //            DependencyProperty.RegisterAttached("X", typeof(double), typeof(ResizePanel),
        //                new FrameworkPropertyMetadata(0.0,
        //                     FrameworkPropertyMetadataOptions.AffectsParentMeasure));
        //// FrameworkPropertyMetadataOptions.AffectsParentMeasure
        //// 値の変更があったときに親要素のinvalidMeasureを実行する


        public static double GetY(DependencyObject obj)
        {
            return (double)obj.GetValue(YProperty);
        }

        public static void SetY(DependencyObject obj, double value)
        {
            obj.SetValue(YProperty, value);
        }

        public static readonly DependencyProperty YProperty =
            DependencyProperty.RegisterAttached("Y", typeof(double), typeof(ResizePanel),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsParentMeasure));


        #endregion 添付プロパティ

        private Point GetTopLeft()
        {
            double left = double.MaxValue;
            double top = double.MaxValue;
            foreach (UIElement child in InternalChildren)
            {
                double posX = GetX(child);// Canvas.GetLeft(child);
                double posY = GetY(child);
                left = Math.Min(left, posX);
                top = Math.Min(top, posY);
            }
            return new Point(left, top);
        }

        // すべての子要素が収まるサイズを測定
        // 左上座標が0以外だった場合は0になるように全体を移動後のサイズ
        protected override Size MeasureOverride(Size availableSize)
        {
            //var pos = GetTopLeft();
            //foreach (UIElement child in InternalChildren)
            //{
            //    SetX(child, GetX(child) - pos.X);
            //    SetY(child, GetY(child) - pos.Y);
            //}
            //double maxX = 0;
            //double maxY = 0;
            //foreach (UIElement child in InternalChildren)
            //{
            //    // 子要素のサイズを測定、これで子要素のDesiredSizeが更新される
            //    child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            //    // 子要素に添付した添付プロパティから、XY座標取得
            //    double posX = GetX(child);
            //    double posY = GetY(child);
            //    maxX = Math.Max(maxX, posX + child.DesiredSize.Width);
            //    maxY = Math.Max(maxY, posY + child.DesiredSize.Height);
            //}
            ////return new Size(maxX - left, maxY - top);// このSizeはDesiredSizeに適用される
            //return new Size(maxX, maxY);// このSizeはDesiredSizeに適用される


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

            return new Size(maxX, maxY);// このSizeはDesiredSizeに適用される
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

        public void Resize()
        {
            var pos = GetTopLeft();
            foreach (UIElement child in InternalChildren)
            {
                SetX(child, GetX(child) - pos.X);
                SetY(child, GetY(child) - pos.Y);
            }

            InvalidateMeasure(); // これは実行しなくてもMeasureOverrideが実行されると思ったけど縮小時は実行されない
        }
    }
}
