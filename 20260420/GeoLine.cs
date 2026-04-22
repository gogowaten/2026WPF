using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace _20260420
{
    /// <summary>
    /// GeoLineDataとは疎結合にしたい
    /// </summary>
    public class GeoLine : Shape
    {
        private VertexAdorner2? _vertexAdorner2; // 頂点移動用ハンドル
        //private VertexAdorner? _vertexAdorner; // 頂点移動用ハンドル
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
                    //MyGeometryBounds = new Rect();
                    return Geometry.Empty;
                }

                PathGeometry geo = MakeLineGeometry(MyPoints);
                _cachedGeometry = geo;
                //MyGeometryBounds = _cachedGeometry.GetRenderBounds(MyStrokePen);
                return geo;
            }
        }

        #region 依存関係プロパティ



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
                //geo.MyGeometryBounds = geo.MyData.OnUpdateBounds(geo._cachedGeometry);
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
            InvalidateVisual(); // 再描画？これだけでは不足、サイズが更新されない、図形によっては再描画にならない
            InvalidateMeasure(); // サイズ更新、図形のActualが更新されないけど、使わないので問題ない

            //InvalidateArrange(); // 全く足りない、図形自体すら再描画されない
            //UpdateLayout(); // 全く足りない、図形自体すら再描画されない

            // 頂点移動用ハンドルの配置更新
            //_vertexAdorner?.UpdateHandles(); // これはあかん
            //_vertexAdorner2?.InvalidateArrange();
            //_vertexAdorner?.InvalidateArrange();
        }

        #endregion 依存関係プロパティ


        #region コンストラクタ
        public GeoLine()
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




        #endregion コンストラクタ

        #region オーバーライド

        //protected override void OnRender(DrawingContext drawingContext)
        //{
        //    // オフセット表示の場合はTranslateTransformで変形したものを描画
        //    if (MyIsOffset)
        //    {
        //        //drawingContext.PushTransform(new TranslateTransform(-MyData.Bounds.Left, -MyData.Bounds.Top));
        //        drawingContext.PushTransform(new TranslateTransform(-MyGeometryBounds.Left, -MyGeometryBounds.Top));
        //    }
        //    //if (MyData is GeoLineData data && data.Background is not null)
        //    //{
        //    //    drawingContext.DrawRectangle(data.Background, null, new Rect(MyData.BoundsLeft, MyData.BoundsTop, MyData.BoundsWidth, MyData.BoundsHeight));
        //    //}
        //    base.OnRender(drawingContext);
        //}
        #endregion オーバーライド

        #region publicメソッド
        public void UpdateVertexHandles()
        {
            _vertexAdorner2?.UpdateHandles();
        }

        public void ShowVertexAdorner()
        {
            if (AdornerLayer.GetAdornerLayer(this) is AdornerLayer layer)
            {
                _vertexAdorner2 = new VertexAdorner2(this);
                //_vertexAdorner = new VertexAdorner(this);
                layer.Add(_vertexAdorner2);
                //layer.Add(_vertexAdorner);
            }
        }

        public void HideVertexAdorner()
        {
            if (AdornerLayer.GetAdornerLayer(this) is AdornerLayer layer && _vertexAdorner2 is not null)
            {
                layer.Remove(_vertexAdorner2);
                _vertexAdorner2 = null;
                //layer.Remove(_vertexAdorner);
                //_vertexAdorner = null;
            }
        }

        #endregion publicメソッド


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