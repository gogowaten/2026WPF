using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Text;
using System.Windows;

namespace _20260216
{
    public static class NodeProps
    {


        public static Point GetPosition(UIElement element)
        {
            return (Point)element.GetValue(PositionProperty);
        }

        public static void SetPosition(UIElement element, Point value)
        {
            element.SetValue(PositionProperty, value);
        }

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.RegisterAttached(
                "Position", typeof(Point), typeof(NodeProps),
                new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.AffectsParentArrange));



    }
}
