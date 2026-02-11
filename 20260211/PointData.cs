using System;
using System.Collections.Generic;
using System.Text;

namespace _20260211
{

    // record
    // **「データを保持することに特化した、書き換え不可（イミュータブル）なクラス」**のようなものです。
    // 2020年のC# 9.0から導入された比較的新しい機能で、今回のような「座標データ」を扱うには最高の選択肢です。

    public record PointData(double X, double Y);

    // ↑は↓とほぼ同じ意味

    //public class PointData
    //{
    //    public double X { get; init; }
    //    public double Y { get; init; }
    //    public PointData(double x, double y) => (X, Y) = (x, y);
    //}


}
