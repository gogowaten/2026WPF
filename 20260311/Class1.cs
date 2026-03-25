using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace _20260311
{

    public partial class GeoLine : Shape
    {

        public Pen StrokePen
        {
            get { return (Pen)GetValue(StrokePenProperty); }
            set { SetValue(StrokePenProperty, value); }
        }
        public static readonly DependencyProperty StrokePenProperty =
            DependencyProperty.Register(nameof(StrokePen), typeof(Pen), typeof(GeoLine), new FrameworkPropertyMetadata(null, OnStrokePenChanged));

        private static void OnStrokePenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GeoLine geoLine)
            {
                geoLine.UpdateRenderBounds();
            }
        }

        public Rect RenderBounds
        {
            get { return (Rect)GetValue(RenderBoundsProperty); }
            set { SetValue(RenderBoundsProperty, value); }
        }
        public static readonly DependencyProperty RenderBoundsProperty =
            DependencyProperty.Register(nameof(RenderBounds), typeof(Rect), typeof(GeoLine), new FrameworkPropertyMetadata(new Rect(0, 0, 0, 0), FrameworkPropertyMetadataOptions.AffectsRender));

        public PointCollection Points
        {
            get { return (PointCollection)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register(nameof(Points), typeof(PointCollection), typeof(GeoLine), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnStrokePenChanged));

        public GeoLine()
        {
            MultiBinding mb = new() { Converter = new ConvStrokePen() };
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeThicknessProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeMiterLimitProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeEndLineCapProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeStartLineCapProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeLineJoinProperty) });
            SetBinding(StrokePenProperty, mb);

        }
        private void DrawBezier(StreamGeometryContext context, Point begin, Point end, bool isFill, bool isClose, bool isSmoothJoin)
        {
            context.BeginFigure(begin, isFill, isClose);
            var bezier = Points.Clone();
            bezier.RemoveAt(0);

            //var bezier = MySegmentPoints.Clone();
            ////            List<Point> bezier = MyPoints.Skip(1).Take(MyPoints.Count - 2).ToList();
            //bezier.Add(end);
            context.PolyBezierTo(bezier, true, isSmoothJoin);

        }
        protected override Geometry DefiningGeometry
        {
            get
            {
                if (Points is null || Points.Count == 0) { return Geometry.Empty; }

                StreamGeometry geo = new();
                using (var context = geo.Open())
                {
                    DrawBezier(context, Points[0], Points[^1], false, false, false);
                }

                geo.Freeze();
                return geo;
            }
            //get
            //{
            //    if (Points is null || Points.Count == 0) { return Geometry.Empty; }

            //    var figure = new PathFigure { StartPoint = Points[0] };

            //    var segment = new PolyBezierSegment();
            //    for (int i = 1; i < Points.Count; i++)
            //    {
            //        segment.Points.Add(Points[i]);
            //    }

            //    var geometry = new PathGeometry();
            //    figure.Segments.Add(segment);
            //    geometry.Figures.Add(figure);

            //    return geometry;
            //}

        }



        protected override void OnRender(DrawingContext drawingContext)
        {
            //UpdateRenderBounds();
            //if (RenderBounds.Width == 0) { return; }

            // 背景を先に描画
            Rect bgRenderBounds = new(new Point(), RenderBounds.Size);
            drawingContext.DrawRectangle(Brushes.LightGray, null, bgRenderBounds);

            // 描画位置をオフセット
            TranslateTransform tt = new(-RenderBounds.X, -RenderBounds.Y);
            drawingContext.PushTransform(tt);



            // 最後に元の線を描画
            base.OnRender(drawingContext);

        }

        [RelayCommand]
        public void UpdateRenderBounds()
        {
            if (Points is null || Points.Count == 0)
            {
                RenderBounds = new Rect();
                return;
            }

            // 見た目上のBoundsをpenを使って取得
            Rect bounds = DefiningGeometry.GetRenderBounds(StrokePen);
            Width = bounds.Width;
            Height = bounds.Height;
            RenderBounds = bounds;
            //InvalidateVisual();
        }

        //[RelayCommand]
        //public void UpdateRenderBounds()
        //{
        //    if (Points is null || Points.Count == 0)
        //    {
        //        RenderBounds = new Rect();
        //        return;
        //    }

        //    //InvalidateMeasure();
        //    // 今使っているpenを再現
        //    Pen pen = new(Brushes.Black, StrokeThickness)
        //    {
        //        EndLineCap = StrokeEndLineCap,
        //        StartLineCap = StrokeStartLineCap,
        //        LineJoin = StrokeLineJoin,
        //        MiterLimit = StrokeMiterLimit
        //    };

        //    // 見た目上のBoundsをpenを使って取得
        //    var temp = DefiningGeometry.GetRenderBounds(pen);
        //    Width = temp.Width;
        //    Height = temp.Height;
        //    RenderBounds = temp;
        //    //InvalidateVisual();
        //}



    }






    internal class Class1
    {
    }
}
