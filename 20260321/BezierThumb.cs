using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace _20260321
{



    public class ControlPointThumb : Thumb
    {
        public int Index { get; }

        public ControlPointThumb(int index)
        {
            this.Index = index;
            Width = 10;
            Height = 10;
            Background = Brushes.Red;

            Template = new ControlTemplate(typeof(Thumb))
            {
                VisualTree = new FrameworkElementFactory(typeof(Ellipse)) { Name = "ellipse" }
            };
            Template.VisualTree.SetValue(Shape.FillProperty, Brushes.Red);

        }
    }


    public class BezierThumb : Thumb
    {

        // PointCollectionプロパティ（変更時に再描画を促す）
        public PointCollection Points
        {
            get { return (PointCollection)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register(nameof(Points), typeof(PointCollection), typeof(BezierThumb), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));




        public BezierThumb()
        {
            // テンプレートを空にする
            Template = new ControlTemplate(typeof(Thumb));
            DragDelta += OnDragDelta;

            // ロード時にAdorner表示
            Loaded += (s, e) =>
            {
                var layer = AdornerLayer.GetAdornerLayer(this);
                if (layer is not null) { layer.Add(new BezierAdorner(this)); }
            };


            Canvas.SetLeft(this, 0);
            Canvas.SetTop(this, 0);


        }




        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            Canvas.SetLeft(this, Canvas.GetLeft(this) + e.HorizontalChange);
            Canvas.SetTop(this, Canvas.GetTop(this) + e.VerticalChange);
        }


        // ベジェ曲線の形状定義
        private Geometry DefiningGeometry
        {
            get
            {
                if (Points == null || Points.Count < 4) return Geometry.Empty;
                var figure = new PathFigure { StartPoint = Points[0] };
                var segment = new PolyBezierSegment();
                for (int i = 1; i < Points.Count; i++) segment.Points.Add(Points[i]);
                figure.Segments.Add(segment);
                return new PathGeometry(new[] { figure });
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            // 1. ペンの作成（Strokeプロパティ等がないため、固定または独自プロパティにする）
            Pen pen = new Pen(Brushes.Black, 3);

            // 2. 範囲取得と背景描画
            Rect bounds = DefiningGeometry.GetRenderBounds(pen);
            drawingContext.DrawRectangle(Brushes.LightGray, null, bounds);

            // 3. 曲線を描画（base.OnRenderはThumbでは何もしないので直接描く）
            drawingContext.DrawGeometry(null, pen, DefiningGeometry);

            //Width = bounds.Width;
            //Height = bounds.Height;
            //Canvas.SetLeft(this, Canvas.GetLeft(this)- bounds.Left);
            //Canvas.SetTop(this, Canvas.GetTop(this)- bounds.Top);
        }

      
     
    }
}
