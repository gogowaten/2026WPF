using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace _20260216_02
{
    // レイアウト用
    public class NodePanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            double maxX = 0, maxY = 0;
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                var pos = NodeProps.GetPosition(child);
                maxX = Math.Max(maxX, pos.X + child.DesiredSize.Width);
                maxY = Math.Max(maxY, pos.Y + child.DesiredSize.Height);
            }
            return new Size(maxX, maxY);
            //return base.MeasureOverride(availableSize);
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                var pos = NodeProps.GetPosition(child);
                child.Arrange(new Rect(pos, child.DesiredSize));
            }
            return finalSize;
            //return base.ArrangeOverride(finalSize);
        }

    }

}
