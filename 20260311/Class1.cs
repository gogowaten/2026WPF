using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

        public Rect RenderBounds
        {
            get { return (Rect)GetValue(RenderBoundsProperty); }
            set { SetValue(RenderBoundsProperty, value); }
        }
        public static readonly DependencyProperty RenderBoundsProperty =
            DependencyProperty.Register(nameof(RenderBounds), typeof(Rect), typeof(GeoLine), new PropertyMetadata(new Rect(0, 0, 0, 0)));

        public PointCollection Points
        {
            get { return (PointCollection)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register(nameof(Points), typeof(PointCollection), typeof(GeoLine), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

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

                //SetRenderBounds();
                
                return geometry;
            }
        }

     

        protected override void OnRender(DrawingContext drawingContext)
        {
            //UpdateRenderBounds();
            //if (RenderBounds.Width == 0) { return; }
            //// 背景を先に描画
            //Rect bgRenderBounds = new(new Point(), RenderBounds.Size);
            //drawingContext.DrawRectangle(Brushes.LightGray, null, bgRenderBounds);


            //Width = RenderBounds.Width;
            //Height = RenderBounds.Height;

            //// 描画位置をオフセット
            //TranslateTransform tt = new(-RenderBounds.X, -RenderBounds.Y);
            //drawingContext.PushTransform(tt);


            // 最後に元の線を描画
            base.OnRender(drawingContext);
        }

        [RelayCommand]
        private void UpdateRenderBounds()
        {
            // 今使っているpenを再現
            Pen pen = new(Brushes.Black, StrokeThickness)
            {
                EndLineCap = StrokeEndLineCap,
                StartLineCap = StrokeStartLineCap,
                LineJoin = StrokeLineJoin,
                MiterLimit = StrokeMiterLimit
            };

            // 見た目上のBoundsをpenを使って取得
            var temp = DefiningGeometry.GetRenderBounds(pen);
            if (temp == Rect.Empty)
            {
                return;
            }
            else
            {
                RenderBounds = temp;
            }

        }

    }






    internal class Class1
    {
    }
}
