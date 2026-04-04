using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace _20260403
{
    public class GeoLine : Shape
    {
        private Geometry? _cachedGeometry;
        //public Rect MyGeometryBounds { get; set; }

        protected override Geometry DefiningGeometry
        {
            get
            {
                // キャッシュが在ればそれを返して終わる
                if (_cachedGeometry is not null) { return _cachedGeometry; }

                if (MyPoints is null || MyPoints.Count < 2)
                {
                    _cachedGeometry = null;

                    //MyGeometryBounds = new Rect();
                    //MyActualSize = new Size();
                    //MyActualWidth = 0.0;
                    UpdateMySize();

                    return Geometry.Empty;
                }

                PathGeometry geo = MakeLineGeometry(MyPoints);
                _cachedGeometry = geo;
                //MyGeometryBounds = geo.GetRenderBounds(MyStrokePen);
                //MyActualWidth = MyGeometryBounds.Width;
                //MyActualSize = MyGeometryBounds.Size;
                UpdateMySize();
                return geo;
            }
        }

        #region 依存関係プロパティ

        public GeoLineData MyData
        {
            get { return (GeoLineData)GetValue(MyDataProperty); }
            set { SetValue(MyDataProperty, value); }
        }
        public static readonly DependencyProperty MyDataProperty =
            DependencyProperty.Register(nameof(MyData), typeof(GeoLineData), typeof(GeoLine), new PropertyMetadata(null));

        //public double MyActualHeight
        //{
        //    get { return (double)GetValue(MyActualHeightProperty); }
        //    set { SetValue(MyActualHeightProperty, value); }
        //}
        //public static readonly DependencyProperty MyActualHeightProperty =
        //    DependencyProperty.Register(nameof(MyActualHeight), typeof(double), typeof(GeoLine), new PropertyMetadata(0.0));

        //public double MyActualWidth
        //{
        //    get { return (double)GetValue(MyActualWidthProperty); }
        //    set { SetValue(MyActualWidthProperty, value); }
        //}
        //public static readonly DependencyProperty MyActualWidthProperty =
        //    DependencyProperty.Register(nameof(MyActualWidth), typeof(double), typeof(GeoLine), new PropertyMetadata(0.0));

        public Size MyActualSize
        {
            get { return (Size)GetValue(MyActualSizeProperty); }
            set { SetValue(MyActualSizeProperty, value); }
        }
        public static readonly DependencyProperty MyActualSizeProperty =
            DependencyProperty.Register(nameof(MyActualSize), typeof(Size), typeof(GeoLine), new PropertyMetadata(null));

        public Rect MyGeometryBounds
        {
            get { return (Rect)GetValue(MyGeometryBoundsProperty); }
            set { SetValue(MyGeometryBoundsProperty, value); }
        }
        public static readonly DependencyProperty MyGeometryBoundsProperty =
            DependencyProperty.Register(nameof(MyGeometryBounds), typeof(Rect), typeof(GeoLine), new PropertyMetadata(null));

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
            DependencyProperty.Register(nameof(MyStrokePen), typeof(Pen), typeof(GeoLine), new PropertyMetadata(null, OnMyStrokePenChanged));

        private static void OnMyStrokePenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // pen変更時の処理
            if (d is GeoLine geo)
            {

                // オフセット表示のときにBoundsも変更すると、常に左上が(0,0)になるけど、
                // 見た目上の頂点座標が変化してしまうことになるので、一長一短
                geo.MyGeometryBounds = geo.DefiningGeometry.GetRenderBounds(geo.MyStrokePen);
                
                geo.UpdateMySize();

                //if (geo.MyGeometryBounds.IsEmpty)
                //{
                //    geo.MyActualWidth = 0;
                //    geo.MyActualSize = new Size();
                //    ShapeData.SetMyAAA(geo, 0);
                //}
                //else
                //{
                //    geo.MyActualWidth = geo.MyGeometryBounds.Width;
                //    geo.MyActualSize = geo.MyGeometryBounds.Size;

                //    geo.MyData.MyActualWidth = geo.MyActualWidth;
                //    geo.MyData.MyActualHeight = geo.MyGeometryBounds.Width;

                //}

            }
        }


        //public ObservableCollection<Point> MyPoints
        //{
        //    get { return (ObservableCollection<Point>)GetValue(MyPointsProperty); }
        //    set { SetValue(MyPointsProperty, value); }
        //}
        //// AffectsMeasure必須
        //public static readonly DependencyProperty MyPointsProperty =
        //    DependencyProperty.Register(nameof(MyPoints), typeof(ObservableCollection<Point>), typeof(GeoLine), new FrameworkPropertyMetadata(null, OnPointCollectionChanged));

        /// <summary>
        /// Pointsの型はPointCollectionでもObservableCollectionのどちらでも同じ結果で
        /// ObservableでPointの追加をしても通知はされないのは、依存関係プロパティでのCollectionは
        /// Collection自体が変化したときだけ通知される仕様、なのでPoint追加で通知したいときは
        /// コールバックの中からCollectionのChangedイベントを購読するようにすればいい
        /// コールバックはPoint追加では呼ばれないけど、起動時には必ず呼ばれるから
        /// </summary>
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
                // 古いCollectionのイベント購読を解除(メモリリーク防止)
                if (e.OldValue is PointCollection oldCollection)
                {
                    oldCollection.Changed -= geo.PointCollection_Changed;
                }

                // 新しいCollectionのイベントを購読を開始
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
            MyData = new();
            Loaded += GeoLine_Loaded;
        }

        private void GeoLine_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is GeoLineData data)
            {
                MyData = data;
            }
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

            //SetBinding(MyActualSizeProperty, new Binding() { Source = this, Path = new PropertyPath(MyGeometryBoundsProperty), Converter = new ConvBoundsSize() });

            //SetBinding(MyActualWidthProperty, new Binding() { Source = this, Path = new PropertyPath(MyGeometryBoundsProperty), Converter = new ConvBoundsWidth() });
        }

        #endregion コンストラクタ

        #region オーバーライド

        protected override void OnRender(DrawingContext drawingContext)
        {
            // オフセット表示の場合はTranslateTransformで変形したものを描画
            if (MyIsOffset)
            {
                drawingContext.PushTransform(new TranslateTransform(-MyGeometryBounds.Left, -MyGeometryBounds.Top));
            }
            base.OnRender(drawingContext);
        }
        #endregion オーバーライド

        #region privateメソッド
        private void UpdateMySize()
        {
            //Rect bounds = DefiningGeometry.GetRenderBounds(MyStrokePen);
            if(_cachedGeometry is null)
            {
                MySizeReset();
                return;
            }
            Rect bounds = _cachedGeometry.GetRenderBounds(MyStrokePen);
            if (bounds.IsEmpty || _cachedGeometry is null)
            {
                MySizeReset();
                return;
            }

            if (MyData is null) { return; }

            MyGeometryBounds = bounds;
            double w = bounds.Width;
            if (bounds.Left < 0) { w -= bounds.Left; }
            MyData.MyActualWidth = w;
            double h = bounds.Height;
            if (bounds.Top < 0) { h -= bounds.Top; }
            MyData.MyActualHeight = h;

            MyData.BoundsTop = bounds.Top;
            MyData.BoundsLeft = bounds.Left;
        }

        private void MySizeReset()
        {

            MyGeometryBounds = new();
            MyActualSize = new();
            if(MyData is null) { return; }

            MyData.ActualSize = new();
            MyData.MyActualWidth = 0;
            MyData.MyActualHeight = 0;
            MyData.BoundsLeft = 0;
            MyData.BoundsTop = 0;
        }

        private static PathGeometry MakeLineGeometry(IEnumerable<Point> pc)
        {
            if (!pc.Any()) { return new PathGeometry(); }

            var seg = new PolyLineSegment(pc, true);
            var fig = new PathFigure(pc.First(), [seg], false);
            //var fig = new PathFigure(pc[0], [seg], false);
            var geo = new PathGeometry([fig]);
            return geo;
        }


        #endregion privateメソッド

        #region publicメソッド


        public Size GetBoundsSize()
        {
            return MyGeometryBounds.Size;
        }
        #endregion publicメソッド
    }


    //public class ConvBoundsWidth : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        var r = (Rect)value;
    //        if (r == Rect.Empty) { return new Size(); }

    //        double w = r.Right; if (r.Left < 0) { w -= r.Left; }
    //        return w;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //public class ConvBoundsSize : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        var r = (Rect)value;
    //        if (r == Rect.Empty) { return new Size(); }

    //        double w = r.Right; if (r.Left < 0) { w -= r.Left; }
    //        double h = r.Bottom; if (r.Top < 0) { h -= r.Top; }
    //        Size s = new(w, h);
    //        return s;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

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
