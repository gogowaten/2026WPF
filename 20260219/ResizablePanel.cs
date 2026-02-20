using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace _20260219
{
    // 子要素のサイズ変更には追随してリサイズするけど、移動は無視される

    /*    public class ResizablePanel : Canvas
        {


            public bool IsAutoSize
            {
                get { return (bool)GetValue(IsAutoSizeProperty); }
                set { SetValue(IsAutoSizeProperty, value); }
            }

            public static readonly DependencyProperty IsAutoSizeProperty =
                DependencyProperty.Register(nameof(IsAutoSize), typeof(bool), typeof(ResizablePanel), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure));

            public ResizablePanel()
            {
                // 背景色がないとヒットテスト(クリック)ガ反応しないので指定
                Background ??= Brushes.Transparent;
            }

            *//*        protected override Size MeasureOverride(Size constraint)
                    {
                        // 子要素にサイズ計測を実行
                        base.MeasureOverride(new Size(double.PositiveInfinity, double.PositiveInfinity));

                        if (IsAutoSize)
                        {
                            double maxWidth = 0;
                            double maxHeight = 0;

                            foreach (UIElement child in InternalChildren)
                            {
                                // Canvas上の位置 ＋ 要素のサイズ
                                double x = GetLeft(child);
                                double y = GetTop(child);

                                // NaNの場合は0として扱う
                                x = double.IsNaN(x) ? 0 : x;
                                y = double.IsNaN(y) ? 0 : y;

                                maxWidth = Math.Max(maxWidth, x) + child.DesiredSize.Width;
                                MaxHeight = Math.Max(maxHeight, y) + child.DesiredSize.Height;
                            }
                            return new Size(maxWidth, MaxHeight);
                        }
                        // 手動リサイズの場合は、現在のサイズを維持
                        return new Size(double.IsNaN(Width) ? 0 : Width, double.IsNaN(Height) ? 0 : Height);
                        //return base.MeasureOverride(constraint);
                    }
            */
    /*


        
        protected override Size MeasureOverride(Size constraint)
        {
            // 1. まず全子要素を計測（これをしないと子要素が表示されません）
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            }

            if (IsAutoSize)
            {
                double maxWidth = 0;
                double maxHeight = 0;

                foreach (UIElement child in InternalChildren)
                {
                    double x = GetLeft(child);
                    double y = GetTop(child);
                    x = double.IsNaN(x) ? 0 : x;
                    y = double.IsNaN(y) ? 0 : y;

                    // 子要素の右端と下端を計算
                    maxWidth = Math.Max(maxWidth, x + child.DesiredSize.Width);
                    maxHeight = Math.Max(maxHeight, y + child.DesiredSize.Height);
                }
                return new Size(maxWidth, maxHeight);
            }

            // --- 修正箇所：手動リサイズ時 ---
            // Width/Height が指定されていない（NaN）場合は、現在のサイズまたは0を返す
            double finalWidth = double.IsNaN(Width) ? ActualWidth : Width;
            double finalHeight = double.IsNaN(Height) ? ActualHeight : Height;

            // もしそれでも0やNaNなら、最小限のサイズを返す（Infinityを避ける）
            return new Size(Math.Max(0, finalWidth), Math.Max(0, finalHeight));
        }

    }
*/


    public class ResizablePanel : Canvas
    {
        public static readonly DependencyProperty IsAutoSizeProperty =
            DependencyProperty.Register("IsAutoSize", typeof(bool), typeof(ResizablePanel),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public bool IsAutoSize
        {
            get => (bool)GetValue(IsAutoSizeProperty);
            set => SetValue(IsAutoSizeProperty, value);
        }

        // 子要素が追加・削除されたときに呼ばれる
        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            if (visualAdded is UIElement element)
            {
                // Canvas.LeftプロパティとCanvas.Topプロパティの変化を監視する
                var leftDescriptor = DependencyPropertyDescriptor.FromProperty(Canvas.LeftProperty, typeof(Canvas));
                var topDescriptor = DependencyPropertyDescriptor.FromProperty(Canvas.TopProperty, typeof(Canvas));

                leftDescriptor.AddValueChanged(element, OnChildLocationChanged);
                topDescriptor.AddValueChanged(element, OnChildLocationChanged);
            }

            if (visualRemoved is UIElement oldElement)
            {
                var leftDescriptor = DependencyPropertyDescriptor.FromProperty(Canvas.LeftProperty, typeof(Canvas));
                var topDescriptor = DependencyPropertyDescriptor.FromProperty(Canvas.TopProperty, typeof(Canvas));

                leftDescriptor.RemoveValueChanged(oldElement, OnChildLocationChanged);
                topDescriptor.RemoveValueChanged(oldElement, OnChildLocationChanged);
            }
        }

        // 座標が変わったら再計測を要求する
        private void OnChildLocationChanged(object sender, EventArgs e)
        {
            if (IsAutoSize)
            {
                // これを呼ぶことで MeasureOverride が再度実行される
                InvalidateMeasure();
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            // 子要素のサイズを計測
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            }

            if (IsAutoSize)
            {
                double maxWidth = 0;
                double maxHeight = 0;

                foreach (UIElement child in InternalChildren)
                {
                    double x = GetLeft(child);
                    double y = GetTop(child);
                    x = double.IsNaN(x) ? 0 : x;
                    y = double.IsNaN(y) ? 0 : y;

                    maxWidth = Math.Max(maxWidth, x + child.DesiredSize.Width);
                    maxHeight = Math.Max(maxHeight, y + child.DesiredSize.Height);
                }
                // 自動リサイズ時は Width/Height を一旦クリア（Auto状態にする）
                this.Width = double.NaN;
                this.Height = double.NaN;

                return new Size(maxWidth, maxHeight);
            }

            // 手動モード時は指定されたサイズを維持
            double finalWidth = double.IsNaN(Width) ? ActualWidth : Width;
            double finalHeight = double.IsNaN(Height) ? ActualHeight : Height;
            return new Size(Math.Max(0, finalWidth), Math.Max(0, finalHeight));
        }
    }


}
