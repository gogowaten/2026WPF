using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace _20260321
{
    public class ArrowShape : Shape
    {
        // 矢印の太さ（依存関係プロパティ）
        public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.Register("Thickness", typeof(double), typeof(ArrowShape),
                new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double Thickness
        {
            get => (double)GetValue(ThicknessProperty);
            set => SetValue(ThicknessProperty, value);
        }

        // ジオメトリを定義するコア部分
        protected override Geometry DefiningGeometry
        {
            get
            {
                // 描画領域のサイズを取得
                double w = RenderSize.Width;
                double h = RenderSize.Height;
                double t = Thickness;

                // 高速な StreamGeometry を使用
                StreamGeometry geometry = new StreamGeometry();
                using (StreamGeometryContext ctx = geometry.Open())
                {
                    // 1. 棒の部分の開始点
                    ctx.BeginFigure(new Point(0, h / 2 - t / 2), true, true);
                    // 2. 棒の先端まで
                    ctx.LineTo(new Point(w - t * 2, h / 2 - t / 2), true, false);
                    // 3. 矢印の頭（上）
                    ctx.LineTo(new Point(w - t * 2, 0), true, false);
                    // 4. 矢印の先端
                    ctx.LineTo(new Point(w, h / 2), true, false);
                    // 5. 矢印の頭（下）
                    ctx.LineTo(new Point(w - t * 2, h), true, false);
                    // 6. 棒の下側
                    ctx.LineTo(new Point(w - t * 2, h / 2 + t / 2), true, false);
                    // 7. 棒の根元（下）
                    ctx.LineTo(new Point(0, h / 2 + t / 2), true, false);
                }

                // 読み取り専用にしてフリーズ（パフォーマンス向上）
                geometry.Freeze();
                return geometry;
            }
        }
    }
}