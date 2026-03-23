using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Security.Cryptography.Xml;


namespace _20260321
{
    public class GeoShape : Shape
    {

        public Rect RenderBounds
        {
            get { return (Rect)GetValue(RenderBoundsProperty); }
            set { SetValue(RenderBoundsProperty, value); }
        }
        public static readonly DependencyProperty RenderBoundsProperty =
            DependencyProperty.Register(nameof(RenderBounds), typeof(Rect), typeof(GeoShape), new PropertyMetadata(new Rect()));


        public PointCollection Points
        {
            get { return (PointCollection)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register(nameof(Points), typeof(PointCollection), typeof(GeoShape), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        protected override Geometry DefiningGeometry
        {
            get
            {
                if (Points is null || Points.Count == 0) { return Geometry.Empty; }

                var figure = new PathFigure { StartPoint = Points[0] };

                var segment = new PolyBezierSegment();
                for (int i = 1; i < Points.Count; i++)
                {
                    segment.Points.Add(Points[i]);
                }

                var geometry = new PathGeometry();
                figure.Segments.Add(segment);
                geometry.Figures.Add(figure);

                return geometry;

            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            // 今使っているpenを再現
            Pen pen = new(Brushes.Black, StrokeThickness)
            {
                EndLineCap = StrokeEndLineCap,
                StartLineCap = StrokeStartLineCap,
                LineJoin = StrokeLineJoin,
                MiterLimit = StrokeMiterLimit
            };

            // ジオメトリのBoundsをpenを使って取得
            RenderBounds = DefiningGeometry.GetRenderBounds(pen);

            //// 背景を先に描画
            //Rect bgRenderBounds = new(new Point(), RenderBounds.Size);
            //drawingContext.DrawRectangle(Brushes.LightGray, null, bgRenderBounds);

            //drawingContext.DrawRectangle(Brushes.LightGray, null, RenderBounds);



            Width = RenderBounds.Width;
            Height = RenderBounds.Height;

            TranslateTransform tt = new(-RenderBounds.X, -RenderBounds.Y);
            drawingContext.PushTransform(tt);


            // その後に元の線を描画
            base.OnRender(drawingContext);
        }
    }
}
