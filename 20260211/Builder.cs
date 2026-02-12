using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace _20260211
{

    // ジオメトリ生成ロジック
    public static class ArrowGeometryBuilder
    {
        public static Geometry CreateLineWithArrow(IList<Point> points, double thickness, double arrowSize)
        {
            var geometry = new StreamGeometry();
            if (points.Count < 2) return geometry;

            using (StreamGeometryContext ctx = geometry.Open())
            {
                // 本体の直線の描画
                ctx.BeginFigure(points[0], false, false);
                for (int i = 1; i < points.Count; i++) { ctx.LineTo(points[i], true, false); }

                // 矢印(二等辺三角形)の計算
                Point end = points[^1];
                Point prev = points[^2];
                Vector direction = end - prev;
                direction.Normalize();

                Vector normal = new Vector(-direction.Y, direction.X);

                // arrowSizeを基準に、幅と長さを決める
                double length = arrowSize;
                double width = arrowSize * 0.8; // 二等辺三角形のバランス

                Point basePoint = end - (direction * length);
                Point left = basePoint + (normal * width / 2);
                Point right = basePoint - (normal * width / 2);

                // 矢印部分を一つの図形として追加
                ctx.BeginFigure(left, true, true);
                ctx.LineTo(end, true, false);
                ctx.LineTo(right, true, false);

            }
            geometry.Freeze();
            return geometry;
        }
    }
}
