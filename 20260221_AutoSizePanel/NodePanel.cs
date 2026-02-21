using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace _20260221_AutoSizePanel
{
    public class NodePanel : Panel
    {


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
                            FrameworkPropertyMetadataOptions.AffectsParentArrange));
        

        public static double GetY(DependencyObject obj)
        {
            return (double)obj.GetValue(YProperty);
        }

        public static void SetY(DependencyObject obj, double value)
        {
            obj.SetValue(YProperty, value);
        }

        // Using a DependencyProperty as the backing store for Y.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty YProperty =
            DependencyProperty.RegisterAttached("Y", typeof(double), typeof(NodePanel),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsParentArrange));


        protected override Size MeasureOverride(Size availableSize)
        {
            double maxX = 0;
            double maxY = 0;
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                double posX = GetX(child);// Canvas.GetLeft(child);
                double posY = GetY(child);// Canvas.GetTop(child);
                //if (double.IsNaN(posX)) { posX = 0; }
                //if (double.IsNaN(posY)) { posY = 0; }
                maxX = Math.Max(maxX, posX + child.DesiredSize.Width);
                maxY = Math.Max(maxY, posY + child.DesiredSize.Height);

            }
            return new Size(maxX, maxY);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                double posX = GetX(child);// Canvas.GetLeft(child);
                double posY = GetY(child);// Canvas.GetTop(child);
                //if (double.IsNaN(posX)) { posX = 0; }
                //if (double.IsNaN(posY)) { posY = 0; }
                child.Arrange(new Rect(new Point(posX, posY), child.DesiredSize));
            }
            return finalSize;
        }
    }

}
