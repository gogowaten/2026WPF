using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace _20260327
{
    public class GeoShape : Shape
    {
        //private Vector _lastOffset;// 描画位置オフセット用
        private Geometry? _cachedGeometry;

        #region 依存関係プロパティ

        // オフセット無しの描画Bounds
        public Rect OriginRenderBounds
        {
            get { return (Rect)GetValue(OriginRenderBoundsProperty); }
            set { SetValue(OriginRenderBoundsProperty, value); }
        }
        public static readonly DependencyProperty OriginRenderBoundsProperty =
            DependencyProperty.Register(nameof(OriginRenderBounds), typeof(Rect), typeof(GeoShape), new PropertyMetadata(new Rect()));


        // オフセットするかどうかのフラグ
        public bool IsOffset
        {
            get { return (bool)GetValue(IsOffsetProperty); }
            set { SetValue(IsOffsetProperty, value); }
        }
        public static readonly DependencyProperty IsOffsetProperty =
            DependencyProperty.Register(nameof(IsOffset), typeof(bool), typeof(GeoShape), new FrameworkPropertyMetadata(false, OnPointsChanged));

        public Pen StrokePen
        {
            get { return (Pen)GetValue(StrokePenProperty); }
            set { SetValue(StrokePenProperty, value); }
        }
        public static readonly DependencyProperty StrokePenProperty =
            DependencyProperty.Register(nameof(StrokePen), typeof(Pen), typeof(GeoShape), new FrameworkPropertyMetadata(null, OnStrokePenChanged));

        private static void OnStrokePenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GeoShape GeoShape)
            {
                GeoShape.UpdateRenderBounds();
            }
        }

        // 実際の描画のBounds
        public Rect RenderBounds
        {
            get { return (Rect)GetValue(RenderBoundsProperty); }
            set { SetValue(RenderBoundsProperty, value); }
        }
        public static readonly DependencyProperty RenderBoundsProperty =
            DependencyProperty.Register(nameof(RenderBounds), typeof(Rect), typeof(GeoShape), new FrameworkPropertyMetadata(new Rect(0, 0, 0, 0)));

        public PointCollection Points
        {
            get { return (PointCollection)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register(nameof(Points), typeof(PointCollection), typeof(GeoShape),
                new FrameworkPropertyMetadata(null, OnPointsChanged));


        //private static void OnPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    var shape = (GeoShape)d;

        //    // 古いコレクションのイベントを解除
        //    if (e.OldValue is PointCollection oldPC && !oldPC.IsFrozen)
        //    {
        //        oldPC.Changed -= shape.OnPointsCollectionChanged;
        //    }
        //    // 新しいコレクションのイベントを購読
        //    if (e.NewValue is PointCollection newPC && !newPC.IsFrozen)
        //    {
        //        newPC.Changed += shape.OnPointsCollectionChanged;
        //    }

        //    //shape.UpdateRenderBounds();
        //}

        // OnPointsChangedでの処理はこれで十分？
        private static void OnPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GeoShape shape) { shape.UpdateRenderBounds(); }
        }

        //// 既存の OnPointsChanged などを利用してGeometryキャッシュを破棄する
        //private void OnPointsCollectionChanged(object? sender, EventArgs e)
        //{
        //    _cachedGeometry = null; // キャッシュクリア
        //    // コレクションの中身が変わったときに再描画とサイズ更新を強制
        //    //InvalidateVisual();
        //    //UpdateRenderBounds();
        //}
        #endregion 依存関係プロパティ


        public GeoShape()
        {
            MultiBinding mb = new() { Converter = new ConvStrokePen() };
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeThicknessProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeMiterLimitProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeEndLineCapProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeStartLineCapProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeLineJoinProperty) });
            SetBinding(StrokePenProperty, mb);

            Loaded += GeoShape_Loaded; // 起動直後に表示されないときはあったほうが良い？必要ないかも
        }

        private void GeoShape_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateRenderBounds();
            //InvalidateVisual(); // 再描画を強制、いる？なくても動く
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                // キャッシュが在ればそれを返して終わる
                if (_cachedGeometry is not null) { return _cachedGeometry; }

                if (Points is null || Points.Count == 0) { return Geometry.Empty; }

                var geo = MakeBezierPathGeometry();

                // 変形前のBoundsを記録
                OriginRenderBounds = geo.GetRenderBounds(StrokePen);

                // オフセット有効なら、変形後のGeometryをキャッシュ
                // 無効ならそのままのGeometryをキャッシュ
                if (IsOffset)
                {
                    // 描画位置が(0,0)になるようにオフセットしたGeometryを作成して返す
                    var transformedGeo = geo.Clone();
                    transformedGeo.Transform = new TranslateTransform(-OriginRenderBounds.X, -OriginRenderBounds.Y);
                    transformedGeo.Freeze(); // 要る？
                    _cachedGeometry = transformedGeo;
                }
                else
                {
                    // オフセットなしのGeometryを返す
                    geo.Freeze(); // 要る？
                    _cachedGeometry = geo;
                }

                RenderBounds = _cachedGeometry.GetRenderBounds(StrokePen);
                return _cachedGeometry;
            }

        }

        private PathGeometry MakeBezierPathGeometry()
        {
            var figure = new PathFigure() { StartPoint = Points[0] };
            var segment = new PolyBezierSegment();
            for (int i = 1; i < Points.Count; i++)
            {
                segment.Points.Add(Points[i]);
            }
            figure.Segments.Add(segment);
            var geo = new PathGeometry([figure]);
            return geo;
        }




        public void UpdateRenderBounds()
        {
            _cachedGeometry = null; // 強制再計算
            InvalidateMeasure();

            if (Points is null || Points.Count == 0 || StrokePen == null)
            {
                OriginRenderBounds = new Rect();
                RenderBounds = new Rect();
                return;
            }

            // 見た目上のBoundsをpenを使って取得
            Rect bounds = DefiningGeometry.GetRenderBounds(StrokePen);

            // なにも描画するものがない（サイズが0）場合は更新しない
            if (bounds.Width == 0 || bounds.Height == 0) return;

            RenderBounds = bounds;

            if (DataContext is Data data)
            {
                //data.Width = ActualWidth; // これが良いけど一手遅れる
                //data.Height = ActualHeight;

                //data.Width = bounds.Width;
                //data.Height = bounds.Height;
                //data.Width = OriginRenderBounds.Width;
                //data.Height = OriginRenderBounds.Height;
                data.Bounds = RenderBounds;
                data.OriginBounds = OriginRenderBounds;
            }
        }

        //[RelayCommand]
        //public void ChangeOffset()
        //{
        //    IsOffset = !IsOffset;
        //}
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