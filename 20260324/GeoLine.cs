using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Data;

namespace _20260324
{
    public class GeoLine : Shape
    {
        #region 依存関係プロパティ

        public Pen LinePen
        {
            get { return (Pen)GetValue(LinePenProperty); }
            set { SetValue(LinePenProperty, value); }
        }
        public static readonly DependencyProperty LinePenProperty =
            DependencyProperty.Register(nameof(LinePen), typeof(Pen), typeof(GeoLine), new FrameworkPropertyMetadata(null, OnLinePenChanged));
        private static void OnLinePenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GeoLine geoLine)
            {
                geoLine.UpdateRenderBoundsSize();
            }
        }

        public PointCollection Points
        {
            get { return (PointCollection)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register(nameof(Points), typeof(PointCollection), typeof(GeoLine), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnLinePenChanged));

        public double BoundsHeight
        {
            get { return (double)GetValue(BoundsHeightProperty); }
            set { SetValue(BoundsHeightProperty, value); }
        }
        public static readonly DependencyProperty BoundsHeightProperty =
            DependencyProperty.Register(nameof(BoundsHeight), typeof(double), typeof(GeoLine), new PropertyMetadata(0.0));

        public double BoundsWidth
        {
            get { return (double)GetValue(BoundsWidthProperty); }
            set { SetValue(BoundsWidthProperty, value); }
        }
        public static readonly DependencyProperty BoundsWidthProperty =
            DependencyProperty.Register(nameof(BoundsWidth), typeof(double), typeof(GeoLine), new PropertyMetadata(0.0));
        #endregion 依存関係プロパティ

        protected override Geometry DefiningGeometry
        {
            get
            {
                if (Points is null || Points.Count == 0) { return Geometry.Empty; }

                PathFigure figure = new() { StartPoint = Points[0] };
                PolyBezierSegment segment = new();
                for (int i = 1; i < Points.Count; i++)
                {
                    segment.Points.Add(Points[i]);
                }

                figure.Segments.Add(segment);
                PathGeometry geometry = new();
                geometry.Figures.Add(figure);

                return geometry;
            }
        }

        public GeoLine()
        {
            MultiBinding mb = new() { Converter = new ConvPen() };
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeThicknessProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeMiterLimitProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeEndLineCapProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeStartLineCapProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeLineJoinProperty) });
            SetBinding(LinePenProperty, mb);

        }

        public void UpdateRenderBoundsSize()
        {
            Rect bounds = DefiningGeometry.GetRenderBounds(LinePen);
            BoundsWidth = bounds.Width;
            BoundsHeight = bounds.Height;
        }
    }
}
