using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace _20260505
{
    public class GeoLineEX : GeoLine
    {
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
                //return DefiningGeometry.GetRenderBounds(MyStrokePen);
                Pen pp = new() { Thickness = StrokeThickness };
                return DefiningGeometry.GetRenderBounds(pp);
            }
        }

        //// 使わない？OnRender実行になる
        //public void RedBG()
        //{
        //    this.InvalidateVisual();
        //}

        // 描画、背景色
        protected override void OnRender(DrawingContext drawingContext)
        {
            //if (IsBackgroundDraw)
            //{
            //    var bounds = GetRenderBoundsWithPen();
            //    drawingContext.DrawRectangle(Background, null, bounds);
            //}
            drawingContext.DrawRectangle(Brushes.Pink, null, GetRenderBoundsWithPen());
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

        public GeoLine()
        {

            Loaded += GeoLine_Loaded;
        }

        private void GeoLine_Loaded(object sender, RoutedEventArgs e)
        {
            MyPoints.CollectionChanged += MyPoints_CollectionChanged;
        }

        private void MyPoints_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
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
            DependencyProperty.Register(nameof(MyPoints), typeof(ObservableCollection<Point>), typeof(GeoLine), new PropertyMetadata(null));

        #endregion 依存関係プロパティ



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
}
