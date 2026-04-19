using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;

namespace _20260419
{
    public class VertexAdorner : Adorner
    {
        private readonly VisualCollection _visualChildren;
        private readonly List<Thumb> _thumbs = new();
        private readonly GeoLine _adornedElement;

        public VertexAdorner(UIElement adornedElement) : base(adornedElement)
        {
            _adornedElement = (GeoLine)adornedElement;
            _visualChildren = new VisualCollection(this);

        }
    }
}
