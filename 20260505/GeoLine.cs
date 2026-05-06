using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace _20260505
{
    /// <summary>
    /// 頂点ハンドル付き
    /// </summary>
    public class GeoLineEX : GeoLine
    {
        private VertexAdorner? MyVertexAdorner; // 頂点ハンドル用のAdorner
        private AdornerLayer MyAdornerLayer = null!; // AdornerLayer保持
        public GeoLineEX()
        {
            SetMyBind();
            Loaded += GeoLineEX_Loaded;
        }

        #region 初期化
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


        private void GeoLineEX_Loaded(object sender, RoutedEventArgs e)
        {
            ReplaceAllPointsToBoundsZero();
            if(AdornerLayer.GetAdornerLayer(this) is AdornerLayer layer)
            {
                MyAdornerLayer = layer;
                if (IsVertexHandle)
                {
                    ShowVertexAdorner();
                }
            }
            else
            {
                throw new InvalidOperationException("AdornerLayerが見つからなかった");
            }
        }

        #endregion 初期化

        #region 依存関係プロパティ


        public GeoLineData MyData
        {
            get { return (GeoLineData)GetValue(MyDataProperty); }
            set { SetValue(MyDataProperty, value); }
        }
        public static readonly DependencyProperty MyDataProperty =
            DependencyProperty.Register(nameof(MyData), typeof(GeoLineData), typeof(GeoLineEX), new PropertyMetadata(null));

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
            //if (d is GeoLineEX geo && geo.MyAdornerLayer is not null)
            //{
            //    if ((bool)e.NewValue)
            //    {
            //        geo.ShowVertexAdorner();
            //    }
            //    else
            //    {
            //        geo.HideVertexAdorner();
            //    }
            //}
        }
        /// <summary>
        /// 背景色の有無
        /// </summary>
        public bool MyIsBackgroundDraw
        {
            get { return (bool)GetValue(MyIsBackgroundDrawProperty); }
            set { SetValue(MyIsBackgroundDrawProperty, value); }
        }
        public static readonly DependencyProperty MyIsBackgroundDrawProperty =
            DependencyProperty.Register(nameof(MyIsBackgroundDraw), typeof(bool), typeof(GeoLineEX), new PropertyMetadata(false));

        /// <summary>
        /// 背景色
        /// </summary>
        public Brush MyBackground
        {
            get { return (Brush)GetValue(MyBackgroundProperty); }
            set { SetValue(MyBackgroundProperty, value); }
        }
        public static readonly DependencyProperty MyBackgroundProperty =
            DependencyProperty.Register(nameof(MyBackground), typeof(Brush), typeof(GeoLineEX), new PropertyMetadata(Brushes.Gray));
        public Rect MyRenderBounds
        {
            get { return (Rect)GetValue(MyRenderBoundsProperty); }
            set { SetValue(MyRenderBoundsProperty, value); }
        }
        public static readonly DependencyProperty MyRenderBoundsProperty =
            DependencyProperty.Register(nameof(MyRenderBounds), typeof(Rect), typeof(GeoLineEX), new PropertyMetadata(Rect.Empty));


        public Pen MyStrokePen
        {
            get { return (Pen)GetValue(MyStrokePenProperty); }
            set { SetValue(MyStrokePenProperty, value); }
        }
        public static readonly DependencyProperty MyStrokePenProperty =
            DependencyProperty.Register(nameof(MyStrokePen), typeof(Pen), typeof(GeoLineEX), new PropertyMetadata(null, OnMyStrokePenChanged));

        private static void OnMyStrokePenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GeoLineEX geo)
            {
                geo.ReplaceAllPointsToBoundsZero();
            }
        }

        #endregion 依存関係プロパティ



        #region 頂点ハンドル

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
            // 頂点ハンドルを一旦削除
            HideVertexAdorner();

            // 新規作成追加
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
        #endregion 頂点ハンドル

        #region パブリックメソッド

        /// <summary>
        /// すべてのポイントをゼロ基点に置き換える
        /// </summary>
        /// <remarks>
        /// 使用先：今のところStrokePenの更新時だけ
        /// 描画BoundsのXYが0になるように、Pointsを置き換える
        /// 再描画を1回で済ませるためにPointsを新たに作成して、それと入れ替える
        /// </remarks>
        public void ReplaceAllPointsToBoundsZero()
        {
            if (MyPoints is null) { return; }
            
            var bounds = GetRenderBoundsWithPen();
            if (Math.Abs(bounds.X + bounds.Y) < 0.01) { return; }

            var ps = new ObservableCollection<Point>();
            foreach (Point item in MyPoints)
            {
                ps.Add(new Point(item.X - bounds.X, item.Y - bounds.Y));
            }
            MyPoints = ps;
        }



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

        //// 使わない？OnRender実行になる
        //public void RedBG()
        //{
        //    this.InvalidateVisual();
        //}
        #endregion パブリックメソッド

        // 描画、背景色
        protected override void OnRender(DrawingContext drawingContext)
        {
            MyRenderBounds = GetRenderBoundsWithPen();
            if (MyBackground is not null)
            {
                //var bounds = GetRenderBoundsWithPen();
                //drawingContext.DrawRectangle(MyBackground, null, bounds);
                drawingContext.DrawRectangle(MyBackground, null, MyRenderBounds);
            }
            base.OnRender(drawingContext);
        }



    }


    //public partial class ObPoint : ObservableObject
    //{
    //    [ObservableProperty] private double _x;
    //    [ObservableProperty] private double _y;
    //}






    public class GeoLine : Shape
    {
        private Geometry? _cachedGeometry; // キャッシュ用Geometry

        protected override Geometry DefiningGeometry
        {
            get
            {
                // キャッシュが在ればそれを返して終了
                if (_cachedGeometry is not null) { return _cachedGeometry; }
#if true
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.ReflectedType?.Name}" +
                    $"__{MethodBase.GetCurrentMethod()?.Name}");
#endif

                if (MyPoints is null) { return Geometry.Empty; }

                PathGeometry geo = MakeLineGeometry(MyPoints);
                _cachedGeometry = geo;
                return _cachedGeometry;
            }
        }

        #region 初期化

        public GeoLine()
        {

            Loaded += GeoLine_Loaded;
        }

        private void GeoLine_Loaded(object sender, RoutedEventArgs e)
        {
            MyPoints.CollectionChanged += MyPoints_CollectionChanged;
        }
        #endregion 初期化

        private void MyPoints_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                _cachedGeometry = null; // Invalidateとの順番はどちらでも良いみたい？
                InvalidateVisual(); // 必要、描画更新
                InvalidateMeasure(); // サイズ更新が不必要なら要らない、ActualWidth、ActualHeight
            }

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                _cachedGeometry = null; // Invalidateとの順番はどちらでも良いみたい？
                InvalidateVisual(); // 必要、描画更新
                InvalidateMeasure(); // サイズ更新が不必要なら要らない、ActualWidth、ActualHeight
            }

        }


        #region 依存関係プロパティ


        public ObservableCollection<Point> MyPoints
        {
            get { return (ObservableCollection<Point>)GetValue(MyPointsProperty); }
            set { SetValue(MyPointsProperty, value); }
        }
        public static readonly DependencyProperty MyPointsProperty =
            DependencyProperty.Register(nameof(MyPoints), typeof(ObservableCollection<Point>), typeof(GeoLine), new PropertyMetadata(null, OnMyPointsChanged));

        private static void OnMyPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GeoLineEX geo)
            {
                geo.MyUpdateVisual();
            }
        }

        #endregion 依存関係プロパティ


        public void MyUpdateVisual()
        {
            _cachedGeometry = null; // Invalidateとの順番はどちらでも良いみたい？
            InvalidateVisual(); // 必要、描画更新
            InvalidateMeasure(); // サイズ更新が不必要なら要らない、ActualWidth、ActualHeight
        }


        /// <summary>
        /// Pointsから直線のPathGeometryを作成
        /// </summary>
        /// <param name="pc"></param>
        /// <returns></returns>
        private static PathGeometry MakeLineGeometry(IEnumerable<Point> pc)
        {
            if (!pc.Any()) { return new PathGeometry(); }

            var seg = new PolyLineSegment(pc, true);
            var fig = new PathFigure(pc.First(), [seg], false);
            var geo = new PathGeometry([fig]);
            return geo;
        }
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







    // 頂点ハンドルのアドーナー、GeoLineEX専用
    public class VertexAdorner : Adorner
    {
        protected override int VisualChildrenCount => _visuals.Count;
        protected override Visual GetVisualChild(int index) => _visuals[index];

        private readonly VisualCollection _visuals;
        private readonly GeoLineEX _adornedElement;
        internal readonly Canvas MyCanvas;
        private double MyHandleSizeHalfOffset;
        private readonly ObservableCollection<Point> MyGeoPoints;

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
