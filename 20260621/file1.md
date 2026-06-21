BitmapVisualizer - Google 検索


# WPFとC#でVisual Studio用のBitmapVisualizerを作成している。Imageコントロールを50倍に拡大表示したときに、各ピクセルの上にARGB各値を表示するにはどんな方法がある？今はImageの上にWrapPanelを置いてそこに、ARGB各値を表示するTextBlockをStackしたStackPanelをピクセルの個数分作成て流し込んでいるけど処理が重いので軽くしたい。画像を表示しているImageコントロールはScrollViewerの中においているので、表示範囲だけARGBの値を表示できれば良い

---

WPFの WrapPanel や StackPanel をピクセルごとに大量に生成すると、UI要素（UIElement）のオーバーヘッドが大きすぎるため、スクロールや拡大時に致命的な重さになります。
解決の鍵は、UI要素の生成を一切やめ、表示領域（Viewport）のピクセルだけを「描画処理（Drawing）」で直接画面に書き込むことです。
最も軽量かつWPF標準の仕組みで行える3つのアプローチを、推奨順に紹介します。
------------------------------
## 推奨1：DrawingVisual を使ったダイレクト描画（最速・最も柔軟）
最もパフォーマンスが高いのは、DrawingContext を使ってテキストを「絵」として直接描画する方法です。UI要素を一切生成しないため、表示範囲が変わっても一瞬で再描画されます。
## 実装手順

   1. ScrollViewer の ScrollChanged イベントと、拡大率変更のイベントを監視します。
   2. 表示されている「現在の画面サイズ」と「スクロール位置（HorizontalOffset / VerticalOffset）」から、今画面に見えている画像のピクセル範囲（X座標、Y座標の開始と終了）を計算します。
   3. カスタムコントロール（または FrameworkElement を継承したクラス）の OnRender をオーバーライドするか、内部の DrawingVisual を更新します。

## 描画コードのイメージ（C#）

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

------------------------------
## 推奨2：ItemsControl + VirtualizingWrapPanel（XAMLで書きたい場合）
どうしてもXAMLのデータバインディングやテンプレートを使って文字をレイアウトしたい場合は、UIの仮想化（Virtualization）を強制導入します。画面に見えている部分のUI要素だけを作成し、スクロールアウトした要素は自動で使い回す仕組みです。
WPF標準の WrapPanel は仮想化に対応していませんが、オープンソースのライブラリ（WpfToolkit や VirtualizingWrapPanel）を使用するか、ItemsControl のレイアウトを工夫します。
## 実装のヒント

* 1ピクセルを表現するViewModelのリスト（数万件あってもメモリは数MB）を用意します。
* ItemsControl の ItemsPanel に VirtualizingStackPanel を縦方向に使い、横方向は各行（Row）ごとに仮想化、またはグリッド計算を行います。
* ※ただし、50倍拡大だと画面内のピクセル数がそこまで多くならないため、「推奨1」の描画アプローチの方が圧倒的に実装がシンプルで軽くなります。

------------------------------
## 推奨3：ピクセルフォント（文字）が描き込まれた「別Bitmap」を重ねる
少しハック的なアプローチですが、CPUでの文字列処理すら省きたい場合に有効です。

   1. 文字画像を作る：あらかじめ、0〜255までの数字が描かれた小さなビットマップ（または透過PNG）をメモリ上に持っておきます。
   2. 合成用のBitmapを作る：画面の表示サイズ（例: 800x600）と同じサイズの WriteableBitmap を1枚用意し、Imageコントロールの前面に重ねます。
   3. スクロール時に描画：スクロールされたら、表示範囲のARGB値に応じた数字画像を、WriteableBitmap 上に CopyPixels などで高速に転送（コピー）します。

GPUによるテクスチャ描画の速度が活かせるため、WPFのレイアウトシステムを完全にバイパスして超高速に動作します。
------------------------------
## まとめ：どれを選ぶべきか？

* 最も簡単かつ確実に軽くなる方法：【推奨1】の DrawingVisual / OnRender での直接描画です。表示領域外のピクセル文字は一切計算も描画もしないため、画像全体が10000x10000ピクセルあっても、画面内（例: 20x20ピクセル分）のループしか走らず、非常に軽量になります。

現在作成中のVisual Studioデバッガービジュアライザーは、現在どのフレームワーク（.NET 8、.NET Framework 4.8など）をターゲットにしていますか？また、元となる画像データは BitmapSource と WriteableBitmap のどちらで保持していますか？それに応じた最適な座標計算コードをご案内できます。

