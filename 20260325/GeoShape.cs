using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace _20260325
{
    public class GeoShape : Shape
    {
        #region 依存関係プロパティ
        public Pen StrokePen
        {
            get { return (Pen)GetValue(StrokePenProperty); }
            set { SetValue(StrokePenProperty, value); }
        }
        public static readonly DependencyProperty StrokePenProperty =
            DependencyProperty.Register(nameof(StrokePen), typeof(Pen), typeof(GeoShape), new FrameworkPropertyMetadata(null, OnStrokePenChanged));

        private static void OnStrokePenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GeoShape GeoShape)
            {
                GeoShape.UpdateRenderBounds();
            }
        }

        public Rect RenderBounds
        {
            get { return (Rect)GetValue(RenderBoundsProperty); }
            set { SetValue(RenderBoundsProperty, value); }
        }
        public static readonly DependencyProperty RenderBoundsProperty =
            DependencyProperty.Register(nameof(RenderBounds), typeof(Rect), typeof(GeoShape), new FrameworkPropertyMetadata(new Rect(0, 0, 0, 0), FrameworkPropertyMetadataOptions.AffectsRender));

        public PointCollection Points
        {
            get { return (PointCollection)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register(nameof(Points), typeof(PointCollection), typeof(GeoShape), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnStrokePenChanged));

        #endregion 依存関係プロパティ

        protected override Geometry DefiningGeometry
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public GeoShape()
        {
            MultiBinding mb = new() { Converter = new ConvStrokePen() };
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeThicknessProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeMiterLimitProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeEndLineCapProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeStartLineCapProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeLineJoinProperty) });
            SetBinding(StrokePenProperty, mb);

        }
    }
}
