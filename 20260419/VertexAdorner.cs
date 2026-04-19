using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace _20260419
{
    public class VertexAdorner : Adorner
    {
        protected override int VisualChildrenCount => _visualChildren.Count;
        protected override Visual GetVisualChild(int index) => _visualChildren[index];

        private readonly VisualCollection _visualChildren;
        private readonly List<Thumb> _handleThumbs = new();
        private readonly GeoLine _adornedElement;
        

        public VertexAdorner(UIElement adornedElement) : base(adornedElement)
        {
            _adornedElement = (GeoLine)adornedElement;
            _visualChildren = new VisualCollection(this);

            for (int i = 0; i < _adornedElement.MyPoints.Count; i++)
            {
                var handle = CreateHandle(i);
                _handleThumbs.Add(handle);
                _visualChildren.Add(handle);
            }
        }

        private Thumb CreateHandle(int index)
        {
            Thumb handle = new()
            {
                Width = 10,
                Height = 10,
                Cursor = Cursors.Hand,
                Tag = index,

            };
            handle.DragDelta += Handle_DragDelta;

            return handle;
        }

        private void Handle_DragDelta(object sender, DragDeltaEventArgs e)
        {
            int id = (int)((Thumb)sender).Tag;
            Point p = _adornedElement.MyPoints[id];
            _adornedElement.MyPoints[id] = new Point(p.X + e.HorizontalChange, p.Y + e.VerticalChange);
            //_adornedElement.InvalidateVisual();
            //InvalidateArrange();
            e.Handled = true;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            for (int i = 0; i < _handleThumbs.Count; i++)
            {
                Point p = _adornedElement.MyPoints[i];
                _handleThumbs[i].Arrange(new Rect(p.X - 5, p.Y - 5, 10, 10));
            }
            return finalSize;
            //return base.ArrangeOverride(finalSize);
        }
    }
}
