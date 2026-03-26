using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace _20260325_01_GeoShapeBoundsOffset
{
    public class GeoShape : Shape
    {
        private Vector _lastOffset;// 描画位置オフセット用

        #region 依存関係プロパティ
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

        private static void OnPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var shape = (GeoShape)d;

            // 古いコレクションのイベントを解除
            if (e.OldValue is PointCollection oldPC && !oldPC.IsFrozen)
            {
                oldPC.Changed -= shape.OnPointsCollectionChanged;
            }
            // 新しいコレクションのイベントを購読
            if (e.NewValue is PointCollection newPC && !newPC.IsFrozen)
            {
                newPC.Changed += shape.OnPointsCollectionChanged;
            }

            shape.UpdateRenderBounds();
        }

        private void OnPointsCollectionChanged(object? sender, EventArgs e)
        {
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

        // オフセット版
        protected override Geometry DefiningGeometry
        {
            get
            {
                if (Points is null || Points.Count == 0) { return Geometry.Empty; }

                StreamGeometry geo = new();
                using (var context = geo.Open())
                {
                    DrawBezier(context, Points[0], false, false, false);
                }

                // --- ここからオフセット版での追加 ---
                // 1. まず「生の」境界を取得
                Rect rawBounds = geo.GetRenderBounds(StrokePen);

                // 2. 左上(X, Y)を 0 にするための変換を作成
                TranslateTransform transform = new(-rawBounds.X, -rawBounds.Y);

                // 3. ジオメトリ自体を変形（左上に詰める）
                Geometry transformedGeo = geo.Clone();
                transformedGeo.Transform = transform;
                // 4. 後続の UpdateRenderBounds で使うために、この X,Y オフセットを保持しておく
                _lastOffset = new Vector(rawBounds.X, rawBounds.Y);

                transformedGeo.Freeze();
                return transformedGeo;

            }
        }

        // オフセット無し版
        //protected override Geometry DefiningGeometry
        //{
        //    get
        //    {
        //        if (Points is null || Points.Count == 0) { return Geometry.Empty; }

        //        StreamGeometry geo = new();
        //        using (var context = geo.Open())
        //        {
        //            DrawBezier(context, Points[0], false, false, false);
        //        }

        //        geo.Freeze();
        //        return geo;
        //    }
        //}



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
            if (Points is null || Points.Count == 0 || StrokePen == null) { return; }

            // 見た目上のBoundsをpenを使って取得
            Rect bounds = DefiningGeometry.GetRenderBounds(StrokePen);

            // なにも描画するものがない（サイズが0）場合は更新しない
            if (bounds.Width == 0 || bounds.Height == 0) return;

            RenderBounds = bounds;

            if (DataContext is GeoShapeData data)
            {
                data.Width = bounds.Width;
                data.Height = bounds.Height;

                // 【重要】前回保存した「左上端」の座標を Canvas 上の Left/Top に反映
                // 既存の Left/Top をベースに、図形が動いた分だけオフセットさせる
                data.Left = _lastOffset.X;
                data.Top = _lastOffset.Y;

            }
        }

        // オフセット無し版
        //public void UpdateRenderBounds()
        //{
        //    if (Points is null || Points.Count == 0 || StrokePen == null)
        //    {
        //        //RenderBounds = new Rect();
        //        return;
        //    }

        //    // 見た目上のBoundsをpenを使って取得
        //    Rect bounds = DefiningGeometry.GetRenderBounds(StrokePen);

        //    // なにも描画するものがない（サイズが0）場合は更新しない
        //    if (bounds.Width == 0 || bounds.Height == 0) return;

        //    RenderBounds = bounds;

        //    if (DataContext is GeoShapeData data)
        //    {
        //        data.Width = bounds.Width;
        //        data.Height = bounds.Height;

        //        //// 自身と親要素でのサイズの再計測、基本的に必要ない
        //        //this.InvalidateMeasure();
        //        //if (Parent is FrameworkElement parent)
        //        //{
        //        //    parent.InvalidateMeasure();
        //        //}


        //        // 必要に応じて Top / Left をオフセットさせる処理もここに書くと
        //        // 座標(10, 10)に描画したものがThumbの左上にピッタリ来ます


        //    }
        //}

    }
}