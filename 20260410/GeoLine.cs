using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace _20260410
{
    public class GeoLine : Shape
    {
        private Geometry? _cachedGeometry;

        protected override Geometry DefiningGeometry
        {
            get
            {
                // キャッシュが在ればそれを返して終わる
                if (_cachedGeometry is not null)
                {
                    return _cachedGeometry;
                }

                if (MyPoints is null || MyPoints.Count < 2)
                {
                    _cachedGeometry = null;

                    //UpdateMySize();
                    MyGeometryBounds = MyData.OnUpdateBounds(_cachedGeometry);
                    //InvalidateVisual();

                    return Geometry.Empty;
                }

                PathGeometry geo = MakeLineGeometry(MyPoints);
                _cachedGeometry = geo;
                //UpdateMySize();
                MyGeometryBounds = MyData.OnUpdateBounds(_cachedGeometry);
                //MyGeometryBounds = _cachedGeometry.GetRenderBounds(MyStrokePen);
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
                //geo.UpdateMySize(); // 必須、描画更新も兼ねている
                geo.MyGeometryBounds = geo.MyData.OnUpdateBounds(geo._cachedGeometry);
            }
        }


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

            }
        }

        private void PointCollection_Changed(object? sender, EventArgs e)
        {
            // キャッシュクリアしてから再描画
            _cachedGeometry = null;
            InvalidateMeasure(); // ほぼ完璧、図形のActualが更新されないけど、使わないので問題ない
            //InvalidateVisual(); // これでは不足、サイズが更新されない
            //InvalidateArrange(); // 全く足りない、図形自体すら再描画されない
            //UpdateLayout(); // 全く足りない、図形自体すら再描画されない
            //InvalidateMeasure();

        }

        #endregion 依存関係プロパティ


        #region コンストラクタ
        public GeoLine()
        {
            //SetMyBind();
            MyData = new();
            Loaded += GeoLine_Loaded;

        }



        private void GeoLine_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is GeoLineData data)
            {
                MyData = data;
                //UpdateMySize();
                MyGeometryBounds = data.OnUpdateBounds(_cachedGeometry);
                InvalidateVisual();
                //
                //data.Width = data.BoundsWidth + data.InternalX - data.X;
                //data.Height = data.BoundsHeight + data.InternalY - data.Y;
            }
        }



        #endregion コンストラクタ

        #region オーバーライド

        protected override void OnRender(DrawingContext drawingContext)
        {
            // オフセット表示の場合はTranslateTransformで変形したものを描画
            if (MyData.IsOffset)
            //if (MyIsOffset)
            {
                //drawingContext.PushTransform(new TranslateTransform(-MyData.Bounds.Left, -MyData.Bounds.Top));
                drawingContext.PushTransform(new TranslateTransform(-MyGeometryBounds.Left, -MyGeometryBounds.Top));
            }
            if (MyData is GeoLineData data && data.Background is not null)
            {
                drawingContext.DrawRectangle(data.Background, null, new Rect(MyData.BoundsLeft, MyData.BoundsTop, MyData.BoundsWidth, MyData.BoundsHeight));
            }
            base.OnRender(drawingContext);
        }
        #endregion オーバーライド


        #region privateメソッド

        private static PathGeometry MakeLineGeometry(IEnumerable<Point> pc)
        {
            if (!pc.Any()) { return new PathGeometry(); }

            var seg = new PolyLineSegment(pc, true);
            var fig = new PathFigure(pc.First(), [seg], false);
            var geo = new PathGeometry([fig]);
            return geo;
        }


        #endregion privateメソッド

        #region publicメソッド


        public Size GetBoundsSize()
        {
            return MyGeometryBounds.Size;
        }

        // 全Pointを左上にオフセット移動させる
        public void OffsetPoints()
        {
            for (int i = 0; i < MyPoints.Count; i++)
            {
                MyPoints[i] = new Point(MyPoints[i].X - MyGeometryBounds.X, MyPoints[i].Y - MyGeometryBounds.Y);
            }
        }
        #endregion publicメソッド
    }



    //public class GeoLine : Shape
    //{
    //    private Geometry? _cachedGeometry;
    //    //public Rect MyGeometryBounds { get; set; }

    //    protected override Geometry DefiningGeometry
    //    {
    //        get
    //        {
    //            // キャッシュが在ればそれを返して終わる
    //            if (_cachedGeometry is not null)
    //            {
    //                return _cachedGeometry;
    //            }

    //            if (MyPoints is null || MyPoints.Count < 2)
    //            {
    //                _cachedGeometry = null;

    //                //UpdateMySize();
    //                MyGeometryBounds = MyData.OnUpdateBounds(_cachedGeometry);
    //                //InvalidateVisual();

    //                return Geometry.Empty;
    //            }

    //            PathGeometry geo = MakeLineGeometry(MyPoints);
    //            _cachedGeometry = geo;
    //            //UpdateMySize();
    //            MyGeometryBounds = MyData.OnUpdateBounds(_cachedGeometry);
    //            //MyGeometryBounds = _cachedGeometry.GetRenderBounds(MyStrokePen);
    //            return geo;
    //        }
    //    }

    //    #region 依存関係プロパティ

    //    public GeoLineData MyData
    //    {
    //        get { return (GeoLineData)GetValue(MyDataProperty); }
    //        set { SetValue(MyDataProperty, value); }
    //    }
    //    public static readonly DependencyProperty MyDataProperty =
    //        DependencyProperty.Register(nameof(MyData), typeof(GeoLineData), typeof(GeoLine), new PropertyMetadata(null));


    //    public Rect MyGeometryBounds
    //    {
    //        get { return (Rect)GetValue(MyGeometryBoundsProperty); }
    //        set { SetValue(MyGeometryBoundsProperty, value); }
    //    }
    //    public static readonly DependencyProperty MyGeometryBoundsProperty =
    //        DependencyProperty.Register(nameof(MyGeometryBounds), typeof(Rect), typeof(GeoLine), new PropertyMetadata(null));

    //    public bool MyIsOffset
    //    {
    //        get { return (bool)GetValue(MyIsOffsetProperty); }
    //        set { SetValue(MyIsOffsetProperty, value); }
    //    }
    //    public static readonly DependencyProperty MyIsOffsetProperty =
    //        DependencyProperty.Register(nameof(MyIsOffset), typeof(bool), typeof(GeoLine), new PropertyMetadata(false, OnMyIsOffsetChanged));
    //    private static void OnMyIsOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        if (d is GeoLine geo)
    //        {
    //            geo.InvalidateVisual(); // 再描画
    //        }
    //    }

    //    public Pen MyStrokePen
    //    {
    //        get { return (Pen)GetValue(MyStrokePenProperty); }
    //        set { SetValue(MyStrokePenProperty, value); }
    //    }
    //    public static readonly DependencyProperty MyStrokePenProperty =
    //        DependencyProperty.Register(nameof(MyStrokePen), typeof(Pen), typeof(GeoLine), new PropertyMetadata(null, OnMyStrokePenChanged));

    //    private static void OnMyStrokePenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        // pen変更時の処理
    //        if (d is GeoLine geo)
    //        {
    //            //geo.UpdateMySize(); // 必須、描画更新も兼ねている
    //            geo.MyGeometryBounds = geo.MyData.OnUpdateBounds(geo._cachedGeometry);
    //        }
    //    }


    //    /// <summary>
    //    /// Pointsの型はPointCollectionでもObservableCollectionのどちらでも同じ結果で
    //    /// ObservableでPointの追加をしても通知はされないのは、依存関係プロパティでのCollectionは
    //    /// Collection自体が変化したときだけ通知される仕様、なのでPoint追加で通知したいときは
    //    /// コールバックの中からCollectionのChangedイベントを購読するようにすればいい
    //    /// コールバックはPoint追加では呼ばれないけど、起動時には必ず呼ばれるから
    //    /// </summary>
    //    public PointCollection MyPoints
    //    {
    //        get { return (PointCollection)GetValue(MyPointsProperty); }
    //        set { SetValue(MyPointsProperty, value); }
    //    }
    //    // AffectsMeasure必須
    //    public static readonly DependencyProperty MyPointsProperty =
    //        DependencyProperty.Register(nameof(MyPoints), typeof(PointCollection), typeof(GeoLine), new FrameworkPropertyMetadata(null, OnPointCollectionChanged));

    //    private static void OnPointCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        if (d is GeoLine geo)
    //        {
    //            // 古いCollectionのイベント購読を解除(メモリリーク防止)
    //            if (e.OldValue is PointCollection oldCollection)
    //            {
    //                oldCollection.Changed -= geo.PointCollection_Changed;
    //            }

    //            // 新しいCollectionのイベントを購読を開始
    //            if (e.NewValue is PointCollection newCollection)
    //            {
    //                newCollection.Changed += geo.PointCollection_Changed;
    //            }

    //        }
    //    }

    //    private void PointCollection_Changed(object? sender, EventArgs e)
    //    {
    //        // キャッシュクリアしてから再描画
    //        _cachedGeometry = null;
    //        InvalidateMeasure(); // ほぼ完璧、図形のActualが更新されないけど、使わないので問題ない
    //        //InvalidateVisual(); // これでは不足、サイズが更新されない
    //        //InvalidateArrange(); // 全く足りない、図形自体すら再描画されない
    //        //UpdateLayout(); // 全く足りない、図形自体すら再描画されない
    //        //InvalidateMeasure();

    //    }

    //    #endregion 依存関係プロパティ


    //    #region コンストラクタ
    //    public GeoLine()
    //    {
    //        //SetMyBind();
    //        MyData = new();
    //        Loaded += GeoLine_Loaded;

    //    }



    //    private void GeoLine_Loaded(object sender, RoutedEventArgs e)
    //    {
    //        if (DataContext is GeoLineData data)
    //        {
    //            MyData = data;
    //            //UpdateMySize();
    //            MyGeometryBounds = data.OnUpdateBounds(_cachedGeometry);
    //            InvalidateVisual();
    //            //
    //            //data.Width = data.BoundsWidth + data.InternalX - data.X;
    //            //data.Height = data.BoundsHeight + data.InternalY - data.Y;
    //        }
    //    }

    //    //private void SetMyBind()
    //    //{
    //    //    MultiBinding mb = new() { Converter = new ConvStrokePen() };
    //    //    mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeThicknessProperty) });
    //    //    mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeMiterLimitProperty) });
    //    //    mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeEndLineCapProperty) });
    //    //    mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeStartLineCapProperty) });
    //    //    mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeLineJoinProperty) });
    //    //    SetBinding(MyStrokePenProperty, mb);

    //    //}

    //    #endregion コンストラクタ

    //    #region オーバーライド

    //    protected override void OnRender(DrawingContext drawingContext)
    //    {
    //        // オフセット表示の場合はTranslateTransformで変形したものを描画
    //        if (MyIsOffset)
    //        {
    //            drawingContext.PushTransform(new TranslateTransform(-MyGeometryBounds.Left, -MyGeometryBounds.Top));
    //        }
    //        if (MyData is GeoLineData data && data.Background is not null)
    //        {
    //            drawingContext.DrawRectangle(data.Background, null, new Rect(MyData.BoundsLeft, MyData.BoundsTop, MyData.BoundsWidth, MyData.BoundsHeight));
    //        }
    //        base.OnRender(drawingContext);
    //    }
    //    #endregion オーバーライド


    //    #region privateメソッド
    //    //private void UpdateMySize()
    //    //{

    //    //    if (_cachedGeometry is null)
    //    //    {
    //    //        MySizeReset();
    //    //        return;
    //    //    }

    //    //    var neko = MyStrokePen;
    //    //    var inu = MyData.StrokePen;
    //    //    Rect bounds = _cachedGeometry.GetRenderBounds(MyStrokePen);
    //    //    //Rect bounds = _cachedGeometry.GetRenderBounds(MyData.StrokePen);
    //    //    if (bounds.IsEmpty || _cachedGeometry is null)
    //    //    {
    //    //        MySizeReset();
    //    //        return;
    //    //    }

    //    //    if (MyData is null) { return; }

    //    //    var diffLeft = bounds.Left - MyGeometryBounds.Left;
    //    //    var diffTop = bounds.Top - MyGeometryBounds.Top;
    //    //    MyData.InternalX += diffLeft;
    //    //    MyData.InternalY += diffTop;
    //    //    //MyData.InternalX += bounds.Left - MyGeometryBounds.Left;
    //    //    //MyData.InternalY += bounds.Top - MyGeometryBounds.Top; 


    //    //    MyData.BoundsTop = bounds.Top;
    //    //    MyData.BoundsLeft = bounds.Left;
    //    //    MyData.BoundsWidth = bounds.Width;
    //    //    MyData.BoundsHeight = bounds.Height;
    //    //    MyData.Bounds = bounds;

    //    //    if (MyData.IsOffset)
    //    //    {
    //    //        //MyData.Width = bounds.Width + MyData.InternalX;
    //    //        //MyData.Height = bounds.Height + MyData.InternalY;
    //    //        MyData.Width = MyData.InternalX + bounds.Width;
    //    //        MyData.Height = MyData.InternalY + bounds.Height;
    //    //    }
    //    //    //MyData.Width = bounds.Width + bounds.Left;
    //    //    //MyData.Height = bounds.Height + bounds.Top;

    //    //    if (bounds.Left < 0)
    //    //    {
    //    //        //MyData.X = bounds.Left;
    //    //        MyData.Width = bounds.Width;// + MyData.InternalX;
    //    //        MyData.Height = bounds.Height;
    //    //    }

    //    //    MyGeometryBounds = bounds;

    //    //    InvalidateVisual(); // あったほうが良い、ないとたまに図形が更新されない時がある
    //    //}

    //    //private void MySizeReset()
    //    //{

    //    //    MyGeometryBounds = new();
    //    //    if (MyData is null) { return; }

    //    //    //MyData.MyActualWidth = 0;
    //    //    //MyData.MyActualHeight = 0;
    //    //    MyData.BoundsLeft = 0;
    //    //    MyData.BoundsTop = 0;
    //    //    MyData.BoundsWidth = 0;
    //    //    MyData.BoundsHeight = 0;
    //    //    //MyData.ActualHeight = ActualHeight;
    //    //    //MyData.ActualWidth = ActualWidth;

    //    //    MyData.Width = 0;
    //    //    MyData.Height = 0;
    //    //}

    //    private static PathGeometry MakeLineGeometry(IEnumerable<Point> pc)
    //    {
    //        if (!pc.Any()) { return new PathGeometry(); }

    //        var seg = new PolyLineSegment(pc, true);
    //        var fig = new PathFigure(pc.First(), [seg], false);
    //        var geo = new PathGeometry([fig]);
    //        return geo;
    //    }


    //    #endregion privateメソッド

    //    #region publicメソッド


    //    public Size GetBoundsSize()
    //    {
    //        return MyGeometryBounds.Size;
    //    }
    //    #endregion publicメソッド
    //}



}