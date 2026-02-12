using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace _20260211.Extensions
{
   
    // PathのPoints用の添付プロパティ

    public static class PathExtensions
    {
        // 添付プロパティの定義
        public static readonly DependencyProperty ArrowPointsProperty =
            DependencyProperty.RegisterAttached(
                "ArrowPoints",
                typeof(ObservableCollection<Point>),
                typeof(PathExtensions),
                new PropertyMetadata(null, OnArrowPointsChanged));

        public static void SetArrowPoints(DependencyObject element, ObservableCollection<Point> value) =>
            element.SetValue(ArrowPointsProperty, value);

        public static ObservableCollection<Point> GetArrowPoints(DependencyObject element) =>
            (ObservableCollection<Point>)element.GetValue(ArrowPointsProperty);


        // プロパティの値 (ObservableCollection) がセットされた時の処理
        private static void OnArrowPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Path path && e.NewValue is ObservableCollection<Point> points)
            {
                double size = GetArrowSize(path);
                void Update() => path.Data = ArrowGeometryBuilder.CreateLineWithArrow(points, path.StrokeThickness, size);
                points.CollectionChanged += (s, args) => Update();
                Update();
            }
        }

/*        プロパティの値(ObservableCollection) がセットされた時の処理
        private static void OnArrowPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Path path) { return; }

            // 前のコレクションの購読を解除
            if (e.OldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= (s, args) =>
                { UpdatePoints(path, e.NewValue as ObservableCollection<Point>); };
            }

            // 新しいコレクションを購読
            if (e.NewValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += (s, args) =>
                {
                    UpdatePoints(path, e.NewValue as ObservableCollection<Point>);
                };
            }
        }

        private static void UpdatePoints(Path path, IEnumerable<Point>? source)
        {
            if (source == null) { return; }
            {
                // PointCollectionを新しく作って差し替える
                //path.Points = [.. source];
                var geo = CreateArrowGeometry(path.points)
            }
        }*/

        // 矢印のサイズ用プロパティを追加
        public static readonly DependencyProperty ArrowSizeProperty =
            DependencyProperty.RegisterAttached("ArrowSize", typeof(double), typeof(PathExtensions),
                new PropertyMetadata(15.0, OnParamsChanged)); // デフォルト値15

        public static void SetArrowSize(DependencyObject d, double v) => d.SetValue(ArrowSizeProperty, v);
        public static double GetArrowSize(DependencyObject d) => (double)d.GetValue(ArrowSizeProperty);

        // ポイント、またはサイズが変更された時に呼ばれる
        //private static void OnArrowPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => OnParamsChanged(d, e);

        private static void OnParamsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Path path && GetArrowPoints(path) is ObservableCollection<Point> points)
            {
                double size = GetArrowSize(path);
                void Update() => path.Data = ArrowGeometryBuilder.CreateLineWithArrow(points, path.StrokeThickness, size);

                // 初回のみイベント登録
                if (e.Property == ArrowPointsProperty)
                {
                    points.CollectionChanged += (s, args) => Update();
                }
                Update();
            }
        }

    }
}
