using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace _20260326
{
    public class GeoShape : Shape
    {
        //private Vector _lastOffset;// 描画位置オフセット用
        private Geometry? _cachedGeometry;
        private Rect _lastRawBounds;
        //public bool IsOffset = false;

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
            DependencyProperty.Register(nameof(IsOffset), typeof(bool), typeof(GeoShape), new PropertyMetadata(false));

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
            DependencyProperty.Register(nameof(RenderBounds), typeof(Rect), typeof(GeoShape), new FrameworkPropertyMetadata(new Rect(0, 0, 0, 0), FrameworkPropertyMetadataOptions.AffectsRender));

        public PointCollection Points
        {
            get { return (PointCollection)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register(nameof(Points), typeof(PointCollection), typeof(GeoShape),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnPointsChanged));

        
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
            var shape = (GeoShape)d;
            shape.UpdateRenderBounds();
        }

        // 既存の OnPointsChanged などを利用してGeometryキャッシュを破棄する
        private void OnPointsCollectionChanged(object? sender, EventArgs e)
        {
            _cachedGeometry = null; // キャッシュクリア
            // コレクションの中身が変わったときに再描画とサイズ更新を強制
            InvalidateVisual();
            UpdateRenderBounds();
        }
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
            InvalidateVisual(); // 再描画を強制

        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                // キャッシュが在ればそれを返して終わる
                if (_cachedGeometry is not null) { return _cachedGeometry; }

                if (Points is null || Points.Count == 0) { return Geometry.Empty; }

                StreamGeometry geo = new();
                using (var context = geo.Open())
                {
                    DrawBezier(context, Points[0], false, false, false);
                }

                // 変形前のBoundsを記録



                OriginRenderBounds = geo.GetRenderBounds(StrokePen);
                // オフセット有効なら、変形後のGeometryをキャッシュ
                // 無効ならそのままのGeometryをキャッシュ
                if (IsOffset)
                {
                    var transformedGeo = geo.Clone();
                    transformedGeo.Transform = new TranslateTransform(-OriginRenderBounds.X, -OriginRenderBounds.Y);
                    transformedGeo.Freeze();
                    _cachedGeometry = transformedGeo;
                    RenderBounds = transformedGeo.GetRenderBounds(StrokePen);
                    return transformedGeo;
                }
                else
                {
                    geo.Freeze();
                    _cachedGeometry = geo;
                    //OriginRenderBounds = geo.GetRenderBounds(StrokePen);
                    RenderBounds = geo.GetRenderBounds(StrokePen);
                    return geo;
                }
            }

        }




        private void DrawBezier(StreamGeometryContext context, Point begin, bool isFill, bool isClose, bool isSmoothJoin)
        {
            context.BeginFigure(begin, isFill, isClose);
            List<Point> bezier = Points.ToList();
            //var bezier = Points.Clone();
            bezier.RemoveAt(0);

            context.PolyBezierTo(bezier, true, isSmoothJoin);
        }

        // オフセット版
        public void UpdateRenderBounds()
        {
            var inu = IsOffset;

            // DefiningGeometry を一度呼んで _lastRawBounds を確定させる
            //var geometry = DefiningGeometry;

            _cachedGeometry = null; // 強制再計算

            if (Points is null || Points.Count == 0 || StrokePen == null) { return; }

            // 見た目上のBoundsをpenを使って取得
            Rect bounds = DefiningGeometry.GetRenderBounds(StrokePen);

            // なにも描画するものがない（サイズが0）場合は更新しない
            if (bounds.Width == 0 || bounds.Height == 0) return;

            RenderBounds = bounds;

            //if (DataContext is GeoShapeData data)
            //{
            //    //data.Width = _lastRawBounds.Width;
            //    //data.Height = _lastRawBounds.Height;
            //    //data.Left = _lastRawBounds.X;
            //    //data.Top = _lastRawBounds.Y;
            //    var neko = RenderBounds;
            //    var last = _lastRawBounds;
            //    data.Width = RenderBounds.Width;
            //    data.Height = RenderBounds.Height;
            //    data.Left = RenderBounds.Left;
            //    data.Top = RenderBounds.Top;
            //}
        }


    }
}