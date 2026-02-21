using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace _20260220
{
    public class NodePanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            double maxX = 0;
            double maxY = 0;
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                double posX = Canvas.GetLeft(child);
                var posY = Canvas.GetTop(child);
                if (double.IsNaN(posX)) { posX = 0; }
                if (double.IsNaN(posY)) { posY = 0; }
                maxX = Math.Max(maxX, posX + child.DesiredSize.Width);
                maxY = Math.Max(maxY, posY + child.DesiredSize.Height);

            }
            return new Size(maxX, maxY);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                var posX = Canvas.GetLeft(child);
                var posY = Canvas.GetTop(child);
                if (double.IsNaN(posX)) { posX = 0; }
                if (double.IsNaN(posY)) { posY = 0; }
                child.Arrange(new Rect(new Point(posX, posY), child.DesiredSize));
            }
            return finalSize;
        }
    }
}
