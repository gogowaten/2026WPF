using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace _20260403
{
    public class GeoLine : Shape
    {
        private Geometry? _cachedGeometry;
        public Rect MyGeometryBounds { get; set; }

        protected override Geometry DefiningGeometry
        {
            get
            {
                // キャッシュが在ればそれを返して終わる
                if (_cachedGeometry is not null) { return _cachedGeometry; }

                if (MyPoints is null || MyPoints.Count < 2)
                {
                    _cachedGeometry = null;
                    MyGeometryBounds = new Rect();
                    return Geometry.Empty;
                }

                PathGeometry geo = MakeLineGeometry(MyPoints);
                _cachedGeometry = geo;
                MyGeometryBounds = geo.GetRenderBounds(MyStrokePen);
                return geo;
            }
        }

        #region 依存関係プロパティ

        public bool MyIsOffset
        {
            get { return (bool)GetValue(MyIsOffsetProperty); }
            set { SetValue(MyIsOffsetProperty, value); }
        }
        public static readonly DependencyProperty MyIsOffsetProperty =
            DependencyProperty.Register(nameof(MyIsOffset), typeof(bool), typeof(GeoLine), new PropertyMetadata(false, OnMyIsOffsetChanged));
        private static void OnMyIsOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GeoLine geo)
            {
                geo.InvalidateVisual(); // 再描画
            }
        }

        public Pen MyStrokePen
        {
            get { return (Pen)GetValue(MyStrokePenProperty); }
            set { SetValue(MyStrokePenProperty, value); }
        }
        public static readonly DependencyProperty MyStrokePenProperty =
            DependencyProperty.Register(nameof(MyStrokePen), typeof(Pen), typeof(GeoLine), new PropertyMetadata(null, OnPointCollectionChanged));


        //public ObservableCollection<Point> MyPoints
        //{
        //    get { return (ObservableCollection<Point>)GetValue(MyPointsProperty); }
        //    set { SetValue(MyPointsProperty, value); }
        //}
        //// AffectsMeasure必須
        //public static readonly DependencyProperty MyPointsProperty =
        //    DependencyProperty.Register(nameof(MyPoints), typeof(ObservableCollection<Point>), typeof(GeoLine), new FrameworkPropertyMetadata(null, OnPointCollectionChanged));

        public PointCollection MyPoints
        {
            get { return (PointCollection)GetValue(MyPointsProperty); }
            set { SetValue(MyPointsProperty, value); }
        }
        // AffectsMeasure必須
        public static readonly DependencyProperty MyPointsProperty =
            DependencyProperty.Register(nameof(MyPoints), typeof(PointCollection), typeof(GeoLine), new FrameworkPropertyMetadata(null, OnPointCollectionChanged));

        private static void OnPointCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GeoLine geo)
            {
                if (e.OldValue is PointCollection oldCollection)
                {
                    oldCollection.Changed -= geo.PointCollection_Changed;
                }
                if (e.NewValue is PointCollection newCollection)
                {
                    newCollection.Changed += geo.PointCollection_Changed;
                }

                //if (e.OldValue is ObservableCollection<Point> oldCollection)
                //{
                //    oldCollection.CollectionChanged -= geo.Points_CollectionChanged;
                //}
                //if (e.NewValue is ObservableCollection<Point> newCollection)
                //{
                //    newCollection.CollectionChanged += geo.Points_CollectionChanged;
                //}

                //geoLine.InvalidateVisual();
                //// キャッシュをクリア後にBoundsの更新
                //geoLine._cachedGeometry = null;
                //geoLine.UpdateMyGeomrtryBounds();
            }
        }

        private void PointCollection_Changed(object? sender, EventArgs e)
        {
            // キャッシュクリアしてから再描画
            _cachedGeometry = null;
            InvalidateVisual();
        }

        private void Points_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // キャッシュクリアしてから再描画
            _cachedGeometry = null;
            //InvalidateMeasure();
            InvalidateVisual();
        }
        #endregion 依存関係プロパティ


        #region コンストラクタ
        public GeoLine()
        {
            SetMyBind();
            //Loaded += GeoLine_Loaded;
            //SizeChanged += GeoLine_SizeChanged;
            //
            Loaded += GeoLine_Loaded;
        }

        private void GeoLine_Loaded(object sender, RoutedEventArgs e)
        {
            //MyPoints.CollectionChanged += MyPoints_CollectionChanged;
        }

        //private void MyPoints_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        //{
        //    Debug.WriteLine("GeoLineCollectionChanged");

        //}

        //private void MyPoints_Changed(object? sender, EventArgs e)
        //{

        //}

        //private void GeoLine_Loaded(object sender, RoutedEventArgs e)
        //{
        //    if (DataContext is GeoLine geoLine)
        //    {
        //        MyPoints = geoLine.MyPoints;
        //    }
        //}
        protected override void OnRender(DrawingContext drawingContext)
        {
            // オフセット表示の場合はTranslateTransformで変形したものを描画
            if (MyIsOffset)
            {
                drawingContext.PushTransform(new TranslateTransform(-MyGeometryBounds.Left, -MyGeometryBounds.Top));
            }
            base.OnRender(drawingContext);
        }

        #endregion コンストラクタ

        #region privateメソッド
        private PathGeometry MakeLineGeometry(IEnumerable<Point> pc)
        {
            if (!pc.Any()) { return new PathGeometry(); }

            var seg = new PolyLineSegment(pc, true);
            var fig = new PathFigure(pc.First(), [seg], false);
            //var fig = new PathFigure(pc[0], [seg], false);
            var geo = new PathGeometry([fig]);
            return geo;
        }

        private void SetMyBind()
        {
            MultiBinding mb = new() { Converter = new ConvStrokePen() };
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeThicknessProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeMiterLimitProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeEndLineCapProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeStartLineCapProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeLineJoinProperty) });
            SetBinding(MyStrokePenProperty, mb);
        }
        #endregion privateメソッド

        #region publicメソッド
        public void ChangeOffset()
        {

        }
        #endregion publicメソッド
    }

    public class ConvStrokePen : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var thick = (double)values[0];
            var miter = (double)values[1];
            var end = (PenLineCap)values[2];
            var start = (PenLineCap)values[3];
            var join = (PenLineJoin)values[4];
            Pen pen = new(Brushes.Transparent, thick)
            {
                EndLineCap = end,
                StartLineCap = start,
                LineJoin = join,
                MiterLimit = miter
            };
            return pen;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }





}
