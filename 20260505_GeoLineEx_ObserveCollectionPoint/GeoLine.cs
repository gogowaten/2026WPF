using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace _20260505_GeoLineEx_ObserveCollectionPoint
{
    public class GeoLineEX : GeoLine
    {
        private VertexAdorner? MyVertexAdorner; // 頂点ハンドル用のAdorner
        private AdornerLayer MyAdornerLayer = null!; // AdornerLayer保持
        private Point MyPointOfRightClicked; // 右クリックの位置記録用、頂点追加に使用


        public GeoLineEX()
        {
            SetMyBind();
            Loaded += GeoLineEX_Loaded;

            // 右クリックメニュー作成
            ContextMenu = CreateContextMenu();

            PreviewMouseRightButtonDown += (s, e) =>
            {
                MyPointOfRightClicked = e.GetPosition(this);
            };

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

            // AdornerLayer確保
            if (AdornerLayer.GetAdornerLayer(this) is AdornerLayer layer)
            {
                MyAdornerLayer = layer;
            }
            else
            {
                throw new InvalidOperationException("AdornerLayerが見つからなかった");
            }
        }

        // 右クリックメニュー作成
        private ContextMenu CreateContextMenu()
        {
            var cm = new ContextMenu();
            var item = new MenuItem() { Header = "編集開始" };
            item.SetBinding(IsEnabledProperty, new Binding() { Source = this, Path = new PropertyPath(IsVertexHandleProperty), Converter = new MyConvReverseBool() });
            item.Click += (s, e) => { IsVertexHandle = true; };
            cm.Items.Add(item);

            //item = new MenuItem() { Header = "ここに頂点を追加" };
            //item.SetBinding(IsEnabledProperty, new Binding() { Source = this, Path = new PropertyPath(IsVertexHandleProperty) });
            //item.Click += (s, e) =>
            //{
            //    MyPoints.Add(MyPointOfRightClicked);
            //};
            //cm.Items.Add(item);

            item = new MenuItem() { Header = "ここに頂点を1番目に挿入" };
            item.SetBinding(IsEnabledProperty, new Binding() { Source = this, Path = new PropertyPath(IsVertexHandleProperty) });
            item.Click += (s, e) =>
            {
                MyPoints.Insert(1, MyPointOfRightClicked);
            };
            cm.Items.Add(item);

            item = new MenuItem() { Header = "ここに頂点を追加(先頭)" };
            item.SetBinding(IsEnabledProperty, new Binding() { Source = this, Path = new PropertyPath(IsVertexHandleProperty) });
            item.Click += (s, e) =>
            {
                MyPoints.Insert(0, MyPointOfRightClicked);
            };
            cm.Items.Add(item);

            item = new MenuItem() { Header = "ここに頂点を追加(末尾)" };
            item.SetBinding(IsEnabledProperty, new Binding() { Source = this, Path = new PropertyPath(IsVertexHandleProperty) });
            item.Click += (s, e) =>
            {
                MyPoints.Insert(MyPoints.Count, MyPointOfRightClicked);
            };
            cm.Items.Add(item);


            item = new MenuItem() { Header = "編集終了" };
            item.SetBinding(IsEnabledProperty, new Binding() { Source = this, Path = new PropertyPath(IsVertexHandleProperty) }); cm.Items.Add(item);
            item.Click += (s, e) => { IsVertexHandle = false; };
            return cm;
        }

        #endregion 初期化

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

                    // 再描画、背景色nullの場合に一時的にTransparentで塗るため
                    geo.InvalidateVisual();
                }
                else
                {
                    geo.HideVertexAdorner();
                    geo.InvalidateVisual();
                }
            }
        }


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

        public double MyOffsetLeft
        {
            get { return (double)GetValue(MyOffsetLeftProperty); }
            set { SetValue(MyOffsetLeftProperty, value); }
        }
        public static readonly DependencyProperty MyOffsetLeftProperty =
            DependencyProperty.Register(nameof(MyOffsetLeft), typeof(double), typeof(GeoLineEX), new PropertyMetadata(0.0));

        public double MyOffsetTop
        {
            get { return (double)GetValue(MyOffsetTopProperty); }
            set { SetValue(MyOffsetTopProperty, value); }
        }
        public static readonly DependencyProperty MyOffsetTopProperty =
            DependencyProperty.Register(nameof(MyOffsetTop), typeof(double), typeof(GeoLineEX), new PropertyMetadata(0.0));


        /// <summary>
        /// 頂点追加、削除時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal override void MyPoints_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            base.MyPoints_CollectionChanged(sender, e);

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (IsVertexHandle)
                {
                    if (sender is ObservableCollection<Point> points && e.NewStartingIndex is int ii)
                    {
                        // 頂点ハンドルを追加後に図形の更新
                        MyVertexAdorner?.AddOrInsertHandle(ii, points[ii]);
                        ReplaceAllPointsToBoundsZero();

                        //InvalidateVisual(); // 再描画、ここでは必要ないのは基底クラスで行っているから？
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (IsVertexHandle)
                {
                    if (e.OldStartingIndex is int ii)
                    {
                        // 該当ハンドルを削除後に図形の更新
                        MyVertexAdorner?.RemoveHandle(ii);
                        ReplaceAllPointsToBoundsZero();
                    }
                }
            }
        }
        #endregion 依存関係プロパティ


        #region 頂点ハンドル

        // 頂点ハンドルの更新
        // 頂点の追加や削除時に使う
        public void UpdateVertexHandles()
        {
            if (IsVertexHandle)
            {
                MyVertexAdorner?.ReMakeAllHandles();
            }
        }


        // 頂点ハンドル表示
        public virtual void ShowVertexAdorner()
        {
            // 一旦全ハンドル削除
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

            // 誤差程度なら更新しない
            if (Math.Abs(bounds.X + bounds.Y) < 0.01)
            {
                if ((Math.Abs(bounds.Width - Width) +
               Math.Abs(bounds.Height - Height)) < 0.01)
                {
                    return;
                }
            }

            Width = bounds.Width;
            Height = bounds.Height;
            MyOffsetLeft += bounds.X;
            MyOffsetTop += bounds.Y;

            // Points更新はCollection自体を入れ替えすることで
            // Point1つごとの更新処理を省く
            var ps = new ObservableCollection<Point>();
            foreach (Point item in MyPoints)
            {
                ps.Add(new Point(item.X - bounds.X, item.Y - bounds.Y));
            }
            MyPoints = ps; // Collection入れ替え

            // 頂点編集時ならハンドルの位置調整
            MyVertexAdorner?.SyncAllThumbPoition();
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
            // null & 編集中 = transparent
            // not null & 編集中 = ブラシ
            // not null & 通常 ブラシ
            // null & 通常 = null
            if (MyBackground is not null)
            {
                drawingContext.DrawRectangle(MyBackground, null, GetRenderBoundsWithPen());
            }
            else if (IsVertexHandle && MyBackground is null)
            {
                drawingContext.DrawRectangle(Brushes.Transparent, null, GetRenderBoundsWithPen());
            }

            base.OnRender(drawingContext);
        }






    }







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

        internal virtual void MyPoints_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                _cachedGeometry = null; // Invalidateとの順番はどちらでも良いみたい？
                InvalidateVisual(); // 必要、描画更新
                InvalidateMeasure(); // サイズ更新が不必要なら要らない、ActualWidth、ActualHeight

            }

            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                _cachedGeometry = null; // Invalidateとの順番はどちらでも良いみたい？
                InvalidateVisual(); // 必要、描画更新
                InvalidateMeasure(); // サイズ更新が不必要なら要らない、ActualWidth、ActualHeight
            }

            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                _cachedGeometry = null; // Invalidateとの順番はどちらでも良いみたい？
                InvalidateVisual(); // 必要、描画更新
                InvalidateMeasure(); // サイズ更新が不必要なら要らない、ActualWidth、ActualHeight
            }

        }


        #region 依存関係プロパティ

        [TypeConverter(typeof(MyTypeConverterStringObserveablePoints))]
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
                if (e.OldValue is ObservableCollection<Point> oldPs)
                {
                    oldPs.CollectionChanged -= geo.MyPoints_CollectionChanged;
                }
                ((ObservableCollection<Point>)e.NewValue).CollectionChanged += geo.MyPoints_CollectionChanged;

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
        private readonly GeoLineEX MyTargetGeoShape;
        internal readonly Canvas MyCanvas;
        private double MyHandleSizeHalfOffset;

        #region 初期化

        public VertexAdorner(GeoLineEX adornedElement) : base(adornedElement)
        {
            MyTargetGeoShape = adornedElement;
            _visuals = new(this);
            MyCanvas = new Canvas();
            MyInit();

            // 頂点の数だけハンドルを作成
            ReMakeAllHandles();


        }


        private void MyInit()
        {
            this.UseLayoutRounding = true; // ドットに合わせてくっきり表示
            _visuals.Add(MyCanvas);
            MyHandleSizeHalfOffset = MyHandleSize / 2.0;
            SetBinding(MyHandleSizeProperty, new Binding() { Source = MyTargetGeoShape, Path = new PropertyPath(GeoLineEX.VertexHandleSizeProperty) });
            SetBinding(MyHandleFillBrushProperty, new Binding() { Source = MyTargetGeoShape, Path = new PropertyPath(GeoLineEX.VertexHandleFillBrushProperty) });
        }



        #endregion 初期化

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

                var points = ador.MyTargetGeoShape.MyPoints;
                for (int i = 0; i < points.Count; i++)
                {
                    ador.SyncThumbPosition(i, points[i]);
                }
            }
        }

        #endregion プロパティ

        /// <summary>
        /// すべてのハンドルを再作成、再配置
        /// </summary>
        public virtual void ReMakeAllHandles()
        {
            var points = MyTargetGeoShape.MyPoints;
            if (points == null) { return; }

            MyCanvas.Children.Clear();

            for (int i = 0; i < points.Count; i++)
            {
                AddOrInsertHandle(i, points[i]);
            }
        }

        private FlatHandle CreateHandle(int i, Point p)
        {
            var thumb = new FlatHandle()
            {
                Cursor = Cursors.Hand,
                MyIndex = i, // インデックスを保持
            };

            thumb.SetBinding(WidthProperty, new Binding() { Source = this, Path = new PropertyPath(MyHandleSizeProperty) });
            thumb.SetBinding(HeightProperty, new Binding() { Source = this, Path = new PropertyPath(MyHandleSizeProperty) });
            thumb.SetBinding(FlatHandle.MyFillBrushProperty, new Binding() { Source = this, Path = new PropertyPath(MyHandleFillBrushProperty) });

            thumb.MyLeft = p.X - MyHandleSizeHalfOffset;
            thumb.MyTop = p.Y - MyHandleSizeHalfOffset;

            thumb.DragDelta += Thumb_DragDelta;
            thumb.DragCompleted += Thumb_DragCompleted;

            thumb.ContextMenu = CreateContextMenuForHandle();

            return thumb;

        }

        /// <summary>
        /// ハンドルの右クリックメニュー作成
        /// </summary>
        /// <returns></returns>
        private ContextMenu CreateContextMenuForHandle()
        {
            var menu = new ContextMenu();
            var item = new MenuItem()
            {
                Header = "頂点削除",
            };
            item.Click += (s, e) =>
            {
                // 右クリックされたハンドルを取得
                if (s is MenuItem item
                && item.Parent is ContextMenu cm
                && cm.Parent is Popup pop
                && pop.PlacementTarget is FlatHandle handle)
                {
                    // 指定インデックスの頂点削除、これは図形側から行う
                    MyTargetGeoShape.MyPoints.RemoveAt(handle.MyIndex);
                }
            };

            menu.Items.Add(item);

            return menu;
        }



        /// <summary>
        /// 指定インデックスを持つハンドルを削除
        /// </summary>
        /// <param name="index"></param>
        public void RemoveHandle(int index)
        {
            // 削除
            MyCanvas.Children.RemoveAt(index);

            // 削除箇所以降のIndexを1詰める
            for (int i = index; i < MyCanvas.Children.Count; i++)
            {
                if (MyCanvas.Children[i] is FlatHandle handle)
                {
                    handle.MyIndex--;
                }
            }

        }

        /// <summary>
        /// 指定Point用のハンドルを追加(挿入)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="p"></param>
        public void AddOrInsertHandle(int index, Point p)
        {
            // 指定インデックスが総数より小さい場合は挿入なので、
            // 挿入箇所以降のハンドルのIndexを底上げする
            for (int i = index; i < MyCanvas.Children.Count; i++)
            {
                if (MyCanvas.Children[i] is FlatHandle handle)
                {
                    handle.MyIndex++;
                }
            }

            MyCanvas.Children.Insert(index, CreateHandle(index, p));
        }

        #region イベント
        // ハンドル移動終了通知用
        //public event EventHandler? MyDragCompleted;
        #endregion   イベント

        // ハンドル移動終了時、ターゲット図形のBoundsと描画更新
        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            //MyDragCompleted?.Invoke(this, e);

            MyTargetGeoShape.ReplaceAllPointsToBoundsZero();
            MyTargetGeoShape.MyUpdateVisual();
        }

        // ハンドル移動時
        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is FlatHandle hanlde && hanlde.MyIndex is int index)
            {
                ObservableCollection<Point> points = MyTargetGeoShape.MyPoints;
                if (points != null && index < points.Count)
                {
                    Point p = points[index];

                    // 頂点座標を更新
                    points[index] = new Point(p.X + e.HorizontalChange, p.Y + e.VerticalChange);

                    // ハンドル位置更新
                    SyncThumbPosition(index, points[index]);
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

        /// <summary>
        /// ハンドルの位置調整
        /// </summary>
        public void SyncAllThumbPoition()
        {
            if (MyCanvas.Children.Count == 0) { return; }
            foreach (FlatHandle item in MyCanvas.Children.OfType<FlatHandle>())
            {
                if (item.MyIndex is int ii)
                {
                    item.MyLeft = MyTargetGeoShape.MyPoints[ii].X - MyHandleSizeHalfOffset;
                    item.MyTop = MyTargetGeoShape.MyPoints[ii].Y - MyHandleSizeHalfOffset;
                }
            }
        }


    }





    public class MyConvReverseBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }






    /// <summary>
    /// 文字列をObservableCollection Point に変換
    /// 例：XAMLでの入力が"0,10 20,100"なら、
    /// ObserableCollectionPoint[Point(0,10), Point(20,100)]に変換する
    /// </summary>
    public class MyTypeConverterStringObserveablePoints : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is null) { return null; }

            if (value is string str)
            {
                ObservableCollection<Point> points = [];

                string[] ss = str.Split(" ");// スペースで分割
                foreach (var item in ss)
                {
                    string[] xy = item.Split(","); // カンマで分割
                    if (double.TryParse(xy[0], out double x) &&
                        double.TryParse(xy[1], out double y))
                    {
                        points.Add(new Point(x, y));
                    }
                }
                return points;
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}