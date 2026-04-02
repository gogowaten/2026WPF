using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace _20260331
{
    public class GeoLine : Shape
    {
        // Geometryのキャッシュを持つことで余計な更新処理を省くことができる、
        // けど、更新のタイミングを制御する必要が出てくる
        private Geometry? _cachedGeometry;
        public Rect MyGeometryBounds;

        #region テスト中依存関係プロパティ

        /// <summary>
        /// オフセット表示の切り替え時は
        /// InvalidateVisualで再描画する
        /// </summary>
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
        #endregion テスト中依存関係プロパティ
        #region 依存関係プロパティ

        //public Rect MyGeometryBounds
        //{
        //    get { return (Rect)GetValue(MyGeometryBoundsProperty); }
        //    set { SetValue(MyGeometryBoundsProperty, value); }
        //}
        //public static readonly DependencyProperty MyGeometryBoundsProperty =
        //    DependencyProperty.Register(nameof(MyGeometryBounds), typeof(Rect), typeof(GeoLine), new PropertyMetadata(null));

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
        //    DependencyProperty.Register(nameof(MyPoints), typeof(ObservableCollection<Point>), typeof(GeoLine), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnPointCollectionChanged));

        public PointCollection MyPoints
        {
            get { return (PointCollection)GetValue(MyPointsProperty); }
            set { SetValue(MyPointsProperty, value); }
        }
        // AffectsMeasure必須
        public static readonly DependencyProperty MyPointsProperty =
            DependencyProperty.Register(nameof(MyPoints), typeof(PointCollection), typeof(GeoLine), new FrameworkPropertyMetadata(new PointCollection(), FrameworkPropertyMetadataOptions.AffectsMeasure, OnPointCollectionChanged));

        private static void OnPointCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //if (d is GeoLine geoLine)
            //{
            //    // キャッシュをクリア後にBoundsの更新
            //    geoLine._cachedGeometry = null;
            //    geoLine.UpdateMyGeomrtryBounds();
            //}
        }


        #endregion 依存関係プロパティ

        #region コンストラクタ系
        public GeoLine()
        {
            SetMyBind();
            Loaded += GeoLine_Loaded;
        }

        private void MyPoints_Changed(object? sender, EventArgs e)
        {
            _cachedGeometry = null;
            UpdateMyGeomrtryBounds();
            InvalidateVisual();
        }

        private void GeoLine_Loaded(object sender, RoutedEventArgs e)
        {
            MyPoints.Changed += MyPoints_Changed;
            //UpdateMyGeomrtryBounds();
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
        #endregion コンストラクタ系

        protected override Geometry DefiningGeometry
        {
            get
            {
                // キャッシュが在ればそれを返して終わる
                if (_cachedGeometry is not null) { return _cachedGeometry; }

                if (MyPoints is null || MyPoints.Count < 2)
                {
                    _cachedGeometry = new PathGeometry();
                    MyGeometryBounds = new Rect();
                    return Geometry.Empty;
                }

                PathGeometry geo = MakeLineGeometry(MyPoints);
                _cachedGeometry = geo;
                MyGeometryBounds = geo.GetRenderBounds(MyStrokePen);
                return geo;
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            // オフセット表示の場合はTranslateTransformで変形したものを描画
            if (MyIsOffset)
            {
                drawingContext.PushTransform(new TranslateTransform(-MyGeometryBounds.Left, -MyGeometryBounds.Top));
            }
            base.OnRender(drawingContext);
        }


        private PathGeometry MakeLineGeometry(IEnumerable<Point> pc)
        {
            if (!pc.Any()) { return new PathGeometry(); }
            
            var seg = new PolyLineSegment(pc, true);
            var fig = new PathFigure(pc.First(), [seg], false);
            //var fig = new PathFigure(pc[0], [seg], false);
            var geo = new PathGeometry([fig]);
            return geo;
        }

        //private PathGeometry MakeLineGeometry(PointCollection pc)
        //{
        //    if (pc.Count == 0) { return new PathGeometry(); }

        //    var seg = new PolyLineSegment(pc, true);
        //    var fig = new PathFigure(pc[0], [seg], false);
        //    var geo = new PathGeometry([fig]);
        //    return geo;
        //}

        public void UpdateMyGeomrtryBounds()
        {

            MyGeometryBounds = DefiningGeometry.GetRenderBounds(MyStrokePen);
        }
    }





}
