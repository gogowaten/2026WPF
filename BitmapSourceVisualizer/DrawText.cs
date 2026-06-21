using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static System.Windows.Forms.AxHost;

namespace BitmapSourceVisualizer
{
    public class DrawText : FrameworkElement
    {
        public DrawText()
        {

        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            // 1. Imageコントロールの現在の拡大サイズと位置を取得
            // 2. スクロール領域から「今見えているピクセル範囲」を計算
            int startX = (int)(scrollViewer.HorizontalOffset / 50);
            int endX = (int)((scrollViewer.HorizontalOffset + scrollViewer.ViewportWidth) / 50) + 1;
            // Y方向も同様に計算...

            // 3. 見えている範囲だけループしてテキストを描画
            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    Color pixelColor = GetPixelColor(x, y); // 画像からARGB取得
                    string argbText = $"{pixelColor.A}\n{pixelColor.R}\n{pixelColor.G}\n{pixelColor.B}";

                    // FormattedText を使って文字列を作成（※.NET Core / .NET 5+ の場合は PixelsPerDip が必要）
                    FormattedText formattedText = new FormattedText(
                        argbText,
                        CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Segoe UI"),
                        10, // フォントサイズ
                        Brushes.Black, // 文字色
                        VisualTreeHelper.GetPixelsPerDip(this));

                    // 50倍拡大されたピクセルの中心に文字を描画
                    Point textPosition = new Point(x * 50 + 2, y * 50 + 2);
                    drawingContext.DrawText(formattedText, textPosition);
                }
            }
        }
    }
}
