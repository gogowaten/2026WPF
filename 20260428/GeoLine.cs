using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace _20260428
{

    public class GeoLineEXforData : GeoLineEX
    {
        //private new VertexAdornerForData? MyVertexAdorner;

        public GeoLineEXforData()
        {
            //MyUpdateSurfaceBounds += GeoLineEXforData_MyUpdateSurfaceBounds;
            Loaded += GeoLineEXforData_Loaded;
        }

        private void GeoLineEXforData_Loaded(object sender, RoutedEventArgs e)
        {
            // 図形のBoundsの調整
            // 元の座標を取得しておく
            var x = MyData.X;
            var y = MyData.Y;
            // Pointsの左上寄せと、Offset
            PointsTopLeftZeroFixWithOffset();

            // Offsetで変更されたのを元の位置に戻す
            MyData.X = x;
            MyData.Y = y;

            // 頂点ハンドル表示
            if (IsVertexHandle) { ShowVertexAdorner(); }

            // 基底クラスのMyStrokePenプロパティ変更時に実行するコールバックへの設定
            MyStrokePenProperty.OverrideMetadata(typeof(GeoLineEXforData), new PropertyMetadata(null, OnMyStrokePenChanged));

            //MyPointsProperty.OverrideMetadata(typeof(GeoLineEXforData), new PropertyMetadata(null, OnMyPointsPropertyChanged));
        }

        //private static void OnMyPointsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    if (d is GeoLineEXforData geo)
        //    {
        //        var n = e.NewValue;
        //        var o = e.OldValue;
        //        geo.PointsTopLeftZeroFixWithOffset();
        //    }
        //}


        #region プロパティ
        // 基底クラスのMyStrokePenプロパティ変更時に実行するコールバック
        private static void OnMyStrokePenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GeoLineEXforData geo)
            {
                geo.PointsTopLeftZeroFixWithOffset();
            }
        }


        // 頂点ハンドルの表示非表示の切り替え用
        public new bool IsVertexHandle
        {
            get { return (bool)GetValue(IsVertexHandleProperty); }
            set { SetValue(IsVertexHandleProperty, value); }
        }
        public new static readonly DependencyProperty IsVertexHandleProperty =
            DependencyProperty.Register(nameof(IsVertexHandle), typeof(bool), typeof(GeoLineEXforData), new PropertyMetadata(false, OnIsVertexHandleChanged));
        private static void OnIsVertexHandleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GeoLineEXforData geo && geo.MyAdornerLayer is not null)
            {
                if ((bool)e.NewValue)
                {
                    geo.ShowVertexAdorner();
                }
                else
                {
                    geo.HideVertexAdorner();
                }
            }
        }

        public GeoLineData MyData
        {
            get { return (GeoLineData)GetValue(MyDataProperty); }
            set { SetValue(MyDataProperty, value); }
        }
        public static readonly DependencyProperty MyDataProperty =
            DependencyProperty.Register(nameof(MyData), typeof(GeoLineData), typeof(GeoLineEXforData), new PropertyMetadata(null));
        #endregion プロパティ

        public override void PointCollection_Changed(object? sender, EventArgs e)
        {
            base.PointCollection_Changed(sender, e);
            PointsTopLeftZeroFixWithOffset();
        }

        /// <summary>
        /// Points全体を左上(0,0)に寄せる + 図形のOffset + 図形のサイズ更新 + ハンドルの位置調整
        /// </summary>
        /// <remarks>
        /// 使用先：ハンドル移動後の処理、pen更新時
        /// </remarks>
        public void PointsTopLeftZeroFixWithOffset()
        {
            // 左上寄せする前に、今のBounds取得しておく
            var bounds = GetRenderBoundsWithPen();

            // Bounds座標が(0,0)に近いときは何もしないで終了
            if (Math.Abs(bounds.Left) + Math.Abs(bounds.Top) < 0.01)
            {
                if (Math.Abs(Width - bounds.Width) + Math.Abs(Height - bounds.Height) < 0.01)
                {
                    return;
                }
            }

            // 全Pointを左上寄せ(全座標変換)
            PointsOffset(MyPoints, -bounds.X, -bounds.Y);

            MyData.Width = bounds.Width;
            MyData.Height = bounds.Height;
            MyData.X += bounds.X; // 図形座標のOffset
            MyData.Y += bounds.Y;

            // 頂点ハンドルのOffset
            MyVertexAdorner?.SyncAllThumbPoition();
        }


        // Points全座標のオフセット
        public static void PointsOffset(PointCollection points, double offsetX, double offsetY)
        {
            // 全座標変換
            for (int i = 0; i < points.Count; i++)
            {
                var p = points[i];
                points[i] = new Point(p.X + offsetX, p.Y + offsetY);
            }
        }



        #region パブリックメソッド
        // 頂点ハンドルの更新
        // 頂点の追加や削除時に使う
        //public new void UpdateVertexHandles()
        //{
        //    //base.UpdateVertexHandles();
        //    if (IsVertexHandle) { MyVertexAdorner?.UpdateHandles(); }
        //}

        // 頂点ハンドル表示(再作成)
        public override void ShowVertexAdorner()
        {
            base.ShowVertexAdorner();

            // ハンドル移動後イベントを購読と、ハンドル移動後の処理
            MyVertexAdorner?.MyDragCompleted += (s, e) => { PointsTopLeftZeroFixWithOffset(); };
        }


        //// 頂点ハンドル非表示(削除)
        public override void HideVertexAdorner()
        {
            base.HideVertexAdorner();
        }

        #endregion パブリックメソッド
    }




    /// <summary>
    /// 頂点ハンドルを備えた拡張線形状を提供します。
    /// </summary>
    /// <remarks>GeoLineEXは、頂点ハンドルの表示と操作を可能にすることでGeoLineを拡張します。
    /// ユーザーは実行時に線の頂点をインタラクティブに調整できます。これは、線形状を直接操作する必要があるグラフィカルな
    /// 編集シナリオで特に役立ちます。IsVertexHandleプロパティは、
    /// これらのハンドルの表示/非表示を制御します。このクラスは、ビジュアルツリーにAdornerLayerが存在することを前提としています。存在しない場合、
    /// ロード時にInvalidOperationExceptionがスローされます。</remarks>
    public class GeoLineEX : GeoLineBG
    {
        internal VertexAdorner? MyVertexAdorner; // 頂点移動用ハンドル
        internal AdornerLayer MyAdornerLayer = null!;

        public GeoLineEX()
        {
            Loaded += GeoLineEX_Loaded;
        }

        private void GeoLineEX_Loaded(object sender, RoutedEventArgs e)
        {
            if (AdornerLayer.GetAdornerLayer(this) is AdornerLayer layer)
            {
                MyAdornerLayer = layer;
                if (IsVertexHandle) { ShowVertexAdorner(); }
            }
            else
            {
                throw new InvalidOperationException("AdornerLayerが見つからなかった");
            }
        }

        #region 依存関係プロパティ

        // 頂点ハンドル色

        public Brush VertexHandleFillBrush
        {
            get { return (Brush)GetValue(VertexHandleFillBrushProperty); }
            set { SetValue(VertexHandleFillBrushProperty, value); }
        }
        public static readonly DependencyProperty VertexHandleFillBrushProperty =
            DependencyProperty.Register(nameof(VertexHandleFillBrush), typeof(Brush), typeof(GeoLineEX), new PropertyMetadata(Brushes.Transparent));

        // 頂点ハンドルサイズ
        public double VertexHandleSize
        {
            get { return (double)GetValue(VertexHandleSizeProperty); }
            set { SetValue(VertexHandleSizeProperty, value); }
        }
        public static readonly DependencyProperty VertexHandleSizeProperty =
            DependencyProperty.Register(nameof(VertexHandleSize), typeof(double), typeof(GeoLineEX), new FrameworkPropertyMetadata(20.0));


        // 頂点ハンドルの表示非表示の切り替え用
        public bool IsVertexHandle
        {
            get { return (bool)GetValue(IsVertexHandleProperty); }
            set { SetValue(IsVertexHandleProperty, value); }
        }
        public static readonly DependencyProperty IsVertexHandleProperty =
            DependencyProperty.Register(nameof(IsVertexHandle), typeof(bool), typeof(GeoLineEX), new PropertyMetadata(false, OnIsVertexHandleChanged));
        private static void OnIsVertexHandleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GeoLineEX geo && geo.MyAdornerLayer is not null)
            {
                if ((bool)e.NewValue)
                {
                    geo.ShowVertexAdorner();
                }
                else
                {
                    geo.HideVertexAdorner();
                }
            }
        }
        #endregion 依存関係プロパティ

        #region パブリックメソッド

        // 頂点ハンドルの更新
        // 頂点の追加や削除時に使う
        public virtual void UpdateVertexHandles()
        {
            if (IsVertexHandle)
            {
                MyVertexAdorner?.UpdateHandles();
            }
        }


        // 頂点ハンドル表示
        public virtual void ShowVertexAdorner()
        {
            // 頂点ハンドルを一旦削除して作り直す
            HideVertexAdorner();
            MyVertexAdorner = new VertexAdorner(this);
            MyAdornerLayer.Add(MyVertexAdorner);
        }


        // 頂点ハンドル非表示(削除)
        public virtual void HideVertexAdorner()
        {
            if (MyVertexAdorner is not null)
            {
                MyAdornerLayer.Remove(MyVertexAdorner);
                MyVertexAdorner = null;
            }
        }
        #endregion パブリックメソッド

    }




    /// <summary>
    /// オプションの背景塗りつぶしを持つ線を表します。背景色と表示/非表示を制御するプロパティを提供します。
    ///

    /// </summary>
    /// <remarks>GeoLineBG は GeoLine を拡張し、線の背後に背景を描画できるようにします。背景は、
    /// IsBackgroundDraw が <see langword="true"/> に設定されている場合にのみレンダリングされます。このクラスは通常、幾何学的線​​の背後に強調表示または着色された背景が必要なカスタム描画シナリオで使用されます。</remarks>
    public class GeoLineBG : GeoLine
    {
        public GeoLineBG()
        {
            SetMyBind();
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


        #region 依存関係プロパティ

        //// 表示位置の調整、trueでCanvasLeftやTopに合わせる。falseは調整なし
        //public bool IsReverseOffsetDraw
        //{
        //    get { return (bool)GetValue(IsReverseOffsetDrawProperty); }
        //    set { SetValue(IsReverseOffsetDrawProperty, value); }
        //}
        //public static readonly DependencyProperty IsReverseOffsetDrawProperty =
        //    DependencyProperty.Register(nameof(IsReverseOffsetDraw), typeof(bool), typeof(GeoLineBG), new PropertyMetadata(false));

        // 背景色
        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }
        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(GeoLineBG), new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender));

        // 背景色の有無
        public bool IsBackgroundDraw
        {
            get { return (bool)GetValue(IsBackgroundDrawProperty); }
            set { SetValue(IsBackgroundDrawProperty, value); }
        }
        public static readonly DependencyProperty IsBackgroundDrawProperty =
            DependencyProperty.Register(nameof(IsBackgroundDraw), typeof(bool), typeof(GeoLineBG), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));
        // penだけど、指定はしない読み取り専用
        public Pen MyStrokePen
        {
            get { return (Pen)GetValue(MyStrokePenProperty); }
            set { SetValue(MyStrokePenProperty, value); }
        }
        public static readonly DependencyProperty MyStrokePenProperty =
            DependencyProperty.Register(nameof(MyStrokePen), typeof(Pen), typeof(GeoLineBG), new PropertyMetadata(null));

        #endregion 依存関係プロパティ

        // 図形がピッタリ収まるRectを返す
        // 内部的な計算なので見た目とは位置が異なる
        public Rect GetRenderBoundsWithPen()
        {
            if (DefiningGeometry is null || DefiningGeometry == Geometry.Empty)
            {
                return Rect.Empty;
            }
            else
            {
                return DefiningGeometry.GetRenderBounds(MyStrokePen);
            }
        }

        // 使わない？OnRender実行になる
        public void RedBG()
        {
            this.InvalidateVisual();
        }

        // 描画、背景色
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (IsBackgroundDraw)
            {
                var bounds = GetRenderBoundsWithPen();
                drawingContext.DrawRectangle(Background, null, bounds);
            }
            base.OnRender(drawingContext);
        }
    }




    // 独立している

    public class GeoLine : Shape
    {

        private Geometry? _cachedGeometry;

        protected override Geometry DefiningGeometry
        {
            get
            {
                //#if DEBUG
                //                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.ReflectedType?.Name}__{MethodBase.GetCurrentMethod()?.Name}");
                //#endif

                // キャッシュが在ればそれを返して終わる
                if (_cachedGeometry is not null)
                {
                    return _cachedGeometry;
                }

                if (MyPoints is null || MyPoints.Count < 2)
                {
                    _cachedGeometry = null;
                    return Geometry.Empty;
                }

                PathGeometry geo = MakeLineGeometry(MyPoints);
                _cachedGeometry = geo;
                return geo;
            }
        }

        #region テスト依存関係プロパティ

        //public double MyOffsetLeft
        //{
        //    get { return (double)GetValue(MyOffsetLeftProperty); }
        //    set { SetValue(MyOffsetLeftProperty, value); }
        //}
        //public static readonly DependencyProperty MyOffsetLeftProperty =
        //    DependencyProperty.Register(nameof(MyOffsetLeft), typeof(double), typeof(GeoLine), new PropertyMetadata(0.0));

        //public double MyOffsetTop
        //{
        //    get { return (double)GetValue(MyOffsetTopProperty); }
        //    set { SetValue(MyOffsetTopProperty, value); }
        //}
        //public static readonly DependencyProperty MyOffsetTopProperty =
        //    DependencyProperty.Register(nameof(MyOffsetTop), typeof(double), typeof(GeoLine), new PropertyMetadata(0.0));

        //public double MySurfaceWidth
        //{
        //    get { return (double)GetValue(MySurfaceWidthProperty); }
        //    set { SetValue(MySurfaceWidthProperty, value); }
        //}
        //public static readonly DependencyProperty MySurfaceWidthProperty =
        //    DependencyProperty.Register(nameof(MySurfaceWidth), typeof(double), typeof(GeoLine), new PropertyMetadata(0.0));

        //public double MySurfaceHeight
        //{
        //    get { return (double)GetValue(MySurfaceHeightProperty); }
        //    set { SetValue(MySurfaceHeightProperty, value); }
        //}
        //public static readonly DependencyProperty MySurfaceHeightProperty =
        //    DependencyProperty.Register(nameof(MySurfaceHeight), typeof(double), typeof(GeoLine), new PropertyMetadata(0.0));

        //public double MySurfaceLeft
        //{
        //    get { return (double)GetValue(MySurfaceLeftProperty); }
        //    set { SetValue(MySurfaceLeftProperty, value); }
        //}
        //public static readonly DependencyProperty MySurfaceLeftProperty =
        //    DependencyProperty.Register(nameof(MySurfaceLeft), typeof(double), typeof(GeoLine), new PropertyMetadata(0.0));

        //public double MySurfaceTop
        //{
        //    get { return (double)GetValue(MySurfaceTopProperty); }
        //    set { SetValue(MySurfaceTopProperty, value); }
        //}
        //public static readonly DependencyProperty MySurfaceTopProperty =
        //    DependencyProperty.Register(nameof(MySurfaceTop), typeof(double), typeof(GeoLine), new PropertyMetadata(0.0));

        #endregion テスト依存関係プロパティ

        #region 依存関係プロパティ

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

        public virtual void PointCollection_Changed(object? sender, EventArgs e)
        {
            // キャッシュクリアしてから再描画
            _cachedGeometry = null;
            InvalidateVisual(); // 再描画？これだけでは不足、サイズが更新されない、図形によっては再描画にならない
            InvalidateMeasure(); // サイズ更新、図形のActualが更新されないけど、使わないので問題ない

            //InvalidateArrange(); // 全く足りない、図形自体すら再描画されない
            //UpdateLayout(); // 全く足りない、図形自体すら再描画されない

            // 頂点移動用ハンドルの配置更新
            //_vertexAdorner?.UpdateHandles(); // これはあかん
        }

        #endregion 依存関係プロパティ


        #region コンストラクタ
        public GeoLine()
        {
            //SetMyBind();
            Loaded += GeoLine_Loaded;
        }

        private void GeoLine_Loaded(object sender, RoutedEventArgs e)
        {
            //UpdateSurfaceBounds(); // Surface(見た目上の位置とサイズ)の更新
        }

        //private void SetMyBind()
        //{
        //    MultiBinding mb = new() { Converter = new ConvStrokePen() };
        //    mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeThicknessProperty) });
        //    mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeMiterLimitProperty) });
        //    mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeEndLineCapProperty) });
        //    mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeStartLineCapProperty) });
        //    mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeLineJoinProperty) });
        //    SetBinding(MyStrokePenProperty, mb);
        //}

        #endregion コンストラクタ

        #region publicメソッド

        //public event EventHandler<Rect>? MyUpdateSurfaceBounds;
        //// Surface(見た目上の位置とサイズ)の更新
        //public void UpdateSurfaceBounds()
        //{            
        //    var bounds = GetRenderBounds();
        //    if(bounds == Rect.Empty) { return; }

        //    MySurfaceHeight = bounds.Height;
        //    MySurfaceLeft = bounds.Left;
        //    MySurfaceTop = bounds.Top;
        //    MySurfaceWidth = bounds.Width;
        //    //MyOffsetLeft = bounds.Left;
        //    //MyOffsetTop = bounds.Top;
        //    MyUpdateSurfaceBounds?.Invoke(this, bounds);// 通知
        //}

        //// 図形がピッタリ収まるRectを返す
        //// 図形の見た目上の位置とサイズのRectを返す
        //public Rect GetSurfaceBounds()
        //{
        //    var bounds = GetRenderBounds();
        //    if(bounds == Rect.Empty) { return bounds; }
        //    var left = Canvas.GetLeft(this);
        //    if (double.IsNaN(left)) { left = 0; }
        //    var top = Canvas.GetTop(this);
        //    if (double.IsNaN(top)) { top = 0; }
        //    Rect surface = new(left + bounds.Left, top + bounds.Top, bounds.Width, bounds.Height);
        //    return surface;
        //}

        //// 図形がピッタリ収まるRectを返す
        //// 内部的な計算なので見た目とは位置が異なる
        //public Rect GetRenderBounds()
        //{
        //    if (_cachedGeometry is null || _cachedGeometry == Geometry.Empty)
        //    {
        //        return Rect.Empty;
        //    }
        //    else
        //    {
        //        return _cachedGeometry.GetRenderBounds(MyStrokePen);
        //    }
        //}

        #endregion publicメソッド


        #region privateメソッド

        // 図形のGeometryをPointsから作成
        private static PathGeometry MakeLineGeometry(IEnumerable<Point> pc)
        {
            if (!pc.Any()) { return new PathGeometry(); }

            var seg = new PolyLineSegment(pc, true);
            var fig = new PathFigure(pc.First(), [seg], false);
            var geo = new PathGeometry([fig]);
            return geo;
        }


        #endregion privateメソッド

    }



    // 各種プロパティからpenを作成
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
















    //public class VertexAdornerForData : VertexAdorner
    //{
    //    private readonly GeoLineEXforData MyTargetElement;
    //    private readonly GeoLineData MyTargetData;

    //    public VertexAdornerForData(GeoLineEXforData adornedElement) : base(adornedElement)
    //    {
    //        MyTargetElement = adornedElement;
    //        MyTargetData = MyTargetElement.MyData;
    //        Loaded += VertexAdornerForData_Loaded;
    //    }

    //    private void VertexAdornerForData_Loaded(object sender, RoutedEventArgs e)
    //    {
    //        //AddCompletedEventForHandleThumb();
    //    }

    //    //#region イベント        
    //    //public event EventHandler? MyDragCompleted;
    //    //#endregion   イベント

    //    // 移動終了時、通知を出す
    //    private void HandleThumb_DragCompleted(object sender, DragCompletedEventArgs e)
    //    {
    //        PointsTopLeftZeroFixWithOffset();
    //    }

    //    //private void AddCompletedEventForHandleThumb()
    //    //{
    //    //    foreach (var item in this.MyCanvas.Children.OfType<Thumb>())
    //    //    {
    //    //        item.DragCompleted += HandleThumb_DragCompleted;
    //    //    }
    //    //}


    //    // Points全体を左上(0,0)に寄せる + 図形のOffset
    //    public void PointsTopLeftZeroFixWithOffset()
    //    {
    //        var bounds = MyTargetElement.GetRenderBoundsWithPen(); // Offset用に元のBounds取得しておく
    //        // Bounds座標が(0,0)に近いときは何もしないで終了
    //        if (Math.Abs(bounds.X) < 0.01 && Math.Abs(bounds.Y) < 0.01) { return; }

    //        // 全座標変換
    //        //PointsOffset(-bounds.X, -bounds.Y);
    //        PointsOffset(MyTargetData.Points, -bounds.X, -bounds.Y);


    //        MyTargetData.Width = bounds.Width;
    //        MyTargetData.Height = bounds.Height;
    //        MyTargetData.X += bounds.X; // 図形座標のOffset
    //        MyTargetData.Y += bounds.Y;

    //        // 頂点ハンドルのOffset
    //        //UpdateVertexHandles();
    //        SyncAllThumbPoition();
    //    }

    //    public static void PointsOffset(PointCollection points, double offsetX, double offsetY)
    //    {
    //        // 全座標変換
    //        for (int i = 0; i < points.Count; i++)
    //        {
    //            var p = points[i];
    //            points[i] = new Point(p.X + offsetX, p.Y + offsetY);
    //        }
    //    }

    //    public override void UpdateHandles()
    //    {
    //        base.UpdateHandles();
    //        foreach (var item in MyCanvas.Children.OfType<Thumb>())
    //        {
    //            item.DragCompleted += HandleThumb_DragCompleted;
    //        }
    //    }

    //    //// Points全体を左上(0,0)に寄せる + 図形のOffset
    //    //public void PointsTopLeftZeroFixWithOffset(GeoLineEXforData exData)
    //    //{
    //    //    var bounds = exData.GetRenderBoundsWithPen(); // Offset用に元のBounds取得しておく
    //    //    // Bounds座標が(0,0)に近いときは何もしないで終了
    //    //    if (Math.Abs(bounds.X) < 0.01 && Math.Abs(bounds.Y) < 0.01) { return; }

    //    //    // 全座標変換
    //    //    //PointsOffset(-bounds.X, -bounds.Y);
    //    //    exData.PointsOffset(exData.MyPoints, -bounds.X, -bounds.Y);


    //    //    MyData.Width = bounds.Width;
    //    //    MyData.Height = bounds.Height;
    //    //    MyData.X += bounds.X; // 図形座標のOffset
    //    //    MyData.Y += bounds.Y;

    //    //    // 頂点ハンドルのOffset
    //    //    UpdateVertexHandles();
    //    //}
    //}








    // 頂点ハンドルのアドーナー、GeoLineEX専用
    public class VertexAdorner : Adorner
    {
        protected override int VisualChildrenCount => _visuals.Count;
        protected override Visual GetVisualChild(int index) => _visuals[index];

        private readonly VisualCollection _visuals;
        private readonly GeoLineEX _adornedElement;
        internal readonly Canvas MyCanvas;
        private double MyHandleSizeHalfOffset;
        private readonly PointCollection MyGeoPoints;

        public VertexAdorner(GeoLineEX adornedElement) : base(adornedElement)
        {
            _adornedElement = adornedElement;
            _visuals = new(this);
            MyCanvas = new Canvas();
            MyGeoPoints = _adornedElement.MyPoints;

            MyInit();

            // 頂点の数だけハンドルを作成
            UpdateHandles();
        }

        private void MyInit()
        {
            this.UseLayoutRounding = true; // ドットに合わせてくっきり表示
            _visuals.Add(MyCanvas);
            MyHandleSizeHalfOffset = MyHandleSize / 2.0;
            SetBinding(MyHandleSizeProperty, new Binding() { Source = _adornedElement, Path = new PropertyPath(GeoLineEX.VertexHandleSizeProperty) });
            SetBinding(MyHandleFillBrushProperty, new Binding() { Source = _adornedElement, Path = new PropertyPath(GeoLineEX.VertexHandleFillBrushProperty) });
        }

        #region プロパティ

        // 頂点ハンドル色
        public Brush MyHandleFillBrush
        {
            get { return (Brush)GetValue(MyHandleFillBrushProperty); }
            set { SetValue(MyHandleFillBrushProperty, value); }
        }
        public static readonly DependencyProperty MyHandleFillBrushProperty =
            DependencyProperty.Register(nameof(MyHandleFillBrush), typeof(Brush), typeof(VertexAdorner), new PropertyMetadata(Brushes.Transparent));


        // 頂点ハンドルサイズ
        public double MyHandleSize
        {
            get { return (double)GetValue(MyHandleSizeProperty); }
            set { SetValue(MyHandleSizeProperty, value); }
        }
        public static readonly DependencyProperty MyHandleSizeProperty =
            DependencyProperty.Register(nameof(MyHandleSize), typeof(double), typeof(VertexAdorner), new PropertyMetadata(20.0, OnMyHandleSizeChanged));

        private static void OnMyHandleSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VertexAdorner ador)
            {
                // ハンドルサイズ変更に伴う変更、オフセット、全ハンドルの座標
                ador.MyHandleSizeHalfOffset = (double)e.NewValue / 2.0;

                var points = ador.MyGeoPoints;
                for (int i = 0; i < points.Count; i++)
                {
                    ador.SyncThumbPosition(i, points[i]);
                }
            }
        }

        #endregion プロパティ

        public virtual void UpdateHandles()
        {
            MyCanvas.Children.Clear();

            if (MyGeoPoints == null) { return; }

            for (int i = 0; i < MyGeoPoints.Count; i++)
            {
                var thumb = new FlatHandle()
                {
                    Cursor = Cursors.Hand,
                    Tag = i, // インデックスを保持
                };

                thumb.SetBinding(WidthProperty, new Binding() { Source = this, Path = new PropertyPath(MyHandleSizeProperty) });
                thumb.SetBinding(HeightProperty, new Binding() { Source = this, Path = new PropertyPath(MyHandleSizeProperty) });
                thumb.SetBinding(FlatHandle.MyFillBrushProperty, new Binding() { Source = this, Path = new PropertyPath(MyHandleFillBrushProperty) });


                thumb.MyLeft = MyGeoPoints[i].X - MyHandleSizeHalfOffset;
                thumb.MyTop = MyGeoPoints[i].Y - MyHandleSizeHalfOffset;

                thumb.DragDelta += Thumb_DragDelta;
                thumb.DragCompleted += Thumb_DragCompleted;

                _ = MyCanvas.Children.Add(thumb);
            }
        }

        #region イベント
        // ハンドル移動終了通知用
        public event EventHandler? MyDragCompleted;
        #endregion   イベント

        // ハンドル移動終了時に通知を出す
        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            MyDragCompleted?.Invoke(this, e);
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is Thumb thumb && thumb.Tag is int index)
            {
                //var points = _adornedElement.MyPoints;
                if (MyGeoPoints != null && index < MyGeoPoints.Count)
                {
                    Point p = MyGeoPoints[index];
                    // 頂点座標を更新
                    MyGeoPoints[index] = new Point(p.X + e.HorizontalChange, p.Y + e.VerticalChange);
                    // ハンドル位置更新
                    SyncThumbPosition(index, MyGeoPoints[index]);
                }
                e.Handled = true;
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            MyCanvas.Arrange(new Rect(finalSize));
            return base.ArrangeOverride(finalSize);
        }


        /// <summary>
        /// 指定したハンドルの座標を頂点に合わせる
        /// </summary>
        /// <param name="index"></param>
        /// <param name="p"></param>
        public void SyncThumbPosition(int index, Point p)
        {
            if (MyCanvas.Children.Count == 0) { return; }
            if (MyCanvas.Children[index] is FlatHandle thumb)
            {
                thumb.MyLeft = p.X - MyHandleSizeHalfOffset;
                thumb.MyTop = p.Y - MyHandleSizeHalfOffset;
            }
        }

        public void SyncAllThumbPoition()
        {
            if (MyCanvas.Children.Count == 0) { return; }
            foreach (FlatHandle item in MyCanvas.Children.OfType<FlatHandle>())
            {
                if (item.Tag is int ii)
                {
                    item.MyLeft = MyGeoPoints[ii].X - MyHandleSizeHalfOffset;
                    item.MyTop = MyGeoPoints[ii].Y - MyHandleSizeHalfOffset;
                }
            }
        }


    }

}