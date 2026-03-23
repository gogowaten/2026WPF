using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20260321
{

    public class GeoLine : Shape
    {
        static GeoLine()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GeoLine), new FrameworkPropertyMetadata(typeof(GeoLine)));
        }
        public GeoLine()
        {

        }


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

            // 背景を先に描画
            Rect bgRenderBounds = new(new Point(), RenderBounds.Size);
            drawingContext.DrawRectangle(Brushes.LightGray, null, bgRenderBounds);

            //drawingContext.DrawRectangle(Brushes.LightGray, null, RenderBounds);



            Width = RenderBounds.Width;
            Height = RenderBounds.Height;

            TranslateTransform tt = new(-RenderBounds.X, -RenderBounds.Y);
            drawingContext.PushTransform(tt);


            // その後に元の線を描画
            base.OnRender(drawingContext);
        }

    }






    [ContentProperty(nameof(MyContent))]
    public class CustomThumb : Thumb
    {
        //public override string ToString()
        //{
        //    //return base.ToString();
        //    return MyData.Name;
        //}

        static CustomThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomThumb), new FrameworkPropertyMetadata(typeof(CustomThumb)));
        }


        #region 依存関係プロパティ


        public Data MyData
        {
            get { return (Data)GetValue(MyDataProperty); }
            set { SetValue(MyDataProperty, value); }
        }
        public static readonly DependencyProperty MyDataProperty =
            DependencyProperty.Register(nameof(MyData), typeof(Data), typeof(CustomThumb), new PropertyMetadata(null));



        public FrameworkElement MyContent
        {
            get { return (FrameworkElement)GetValue(MyContentProperty); }
            set { SetValue(MyContentProperty, value); }
        }
        public static readonly DependencyProperty MyContentProperty =
            DependencyProperty.Register(nameof(MyContent), typeof(FrameworkElement), typeof(CustomThumb), new FrameworkPropertyMetadata(null, OnMyContentChanged));

        private static void OnMyContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CustomThumb ct)
            {
                if (double.IsNaN(Canvas.GetLeft(ct))) { Canvas.SetLeft(ct, 0); }
                if (double.IsNaN(Canvas.GetTop(ct))) { Canvas.SetTop(ct, 0); }
            }
        }
        #endregion 依存関係プロパティ

        //public bool MyIsSelected
        //{
        //    get { return (bool)GetValue(MyIsSelectedProperty); }
        //    set { SetValue(MyIsSelectedProperty, value); }
        //}
        //public static readonly DependencyProperty MyIsSelectedProperty =
        //    DependencyProperty.Register(nameof(MyIsSelected), typeof(bool), typeof(CustomThumb), new PropertyMetadata(false));



        public CustomThumb()
        {
            //this.DataContext = this;

            DragDelta += CustomThumb_DragDelta;
            DragCompleted += CustomThumb_DragCompleted;
        }

        private void CustomThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            //MyData.RootData?.UpdateSize();

        }

        private void CustomThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            //MyData.X += e.HorizontalChange;
            //MyData.Y += e.VerticalChange;

            Canvas.SetLeft(this, Canvas.GetLeft(this) + e.HorizontalChange);
            Canvas.SetTop(this, Canvas.GetTop(this) + e.VerticalChange);

        }




    }











}