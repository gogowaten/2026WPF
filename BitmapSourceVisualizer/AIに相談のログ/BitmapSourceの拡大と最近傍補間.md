WPFで`BitmapSource`を**最近傍補間（Nearest Neighbor）**を使って2倍に拡大・縮小し、新しい`BitmapSource`として取得するには、大きく分けて**2つのアプローチ**があります。

目的が「描画時に拡大して表示したいだけ」なのか、「データとして拡大後の`BitmapSource`オブジェクトが欲しい」のかによって使い分けます。

---

## 方法1：`TransformedBitmap` を使う（おすすめ：軽量・高速）

最もWPFらしく、パフォーマンスが良い方法です。`TransformedBitmap` を使い、拡大時の補間モードを最近傍補間に指定します。これによって、メモリを無駄に消費せず、オブジェクトとして新しい`BitmapSource`を取得できます。

```csharp
using System.Windows.Media;
using System.Windows.Media.Imaging;

// 元のBitmapSourceがあると仮定 (originalBitmap)
BitmapSource originalBitmap = ...;

// 1. 2倍にするためのスケール変換を作成
var scaleTransform = new ScaleTransform(2.0, 2.0);

// 2. TransformedBitmapを作成して重ねる
var resizedBitmap = new TransformedBitmap(originalBitmap, scaleTransform);

// 3. ここが重要：レンダリング時の補間モードを「最近傍補間」に設定
RenderOptions.SetBitmapScalingMode(resizedBitmap, BitmapScalingMode.NearestNeighbor);

// これで resizedBitmap を Image コントロールの Source などにバインドできます

```

### 💡 ポイント

* `RenderOptions.SetBitmapScalingMode` は、対象のオブジェクトが画面に描画される（または他の描画処理に回される）際の計算方法をコントロールします。これを `NearestNeighbor` にすることで、ぼやけずにドット絵のようなパキッとした2倍拡大になります。

---

## 方法2：`DrawingVisual` を使って新しいビットマップに焼き付ける

「画面に表示するだけでなく、完全に2倍の解像度を持った独立した新しいピクセルデータ（ファイル保存用など）として`BitmapSource`が欲しい」という場合は、`RenderTargetBitmap` を使ってグラフィックとして描き出します。

```csharp
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

BitmapSource originalBitmap = ...;

// 1. 拡大後のサイズを計算
int newWidth = originalBitmap.PixelWidth * 2;
int newHeight = originalBitmap.PixelHeight * 2;

// 2. 描き込み用の Visual を作成
DrawingVisual drawingVisual = new DrawingVisual();
using (DrawingContext drawingContext = drawingVisual.RenderOpen())
{
    // 最近傍補間を設定
    RenderOptions.SetBitmapScalingMode(drawingVisual, BitmapScalingMode.NearestNeighbor);
    
    // 2倍のサイズに引き伸ばして描画
    drawingContext.DrawImage(originalBitmap, new Rect(0, 0, newWidth, newHeight));
}

// 3. RenderTargetBitmap に焼き付ける
RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
    newWidth, 
    newHeight, 
    originalBitmap.DpiX, 
    originalBitmap.DpiY, 
    PixelFormats.Pbgra32); // 必要に応じてフォーマットを調整

renderTargetBitmap.Render(drawingVisual);

// renderTargetBitmap は BitmapSource を継承しているので、そのまま使えます

```

### 💡 ポイント

* この方法で生成された `renderTargetBitmap` は、内部のピクセルデータ自体が実際に2倍のサイズに拡張されています。そのままPNGなどにエンコードして保存したい場合に最適です。


## まとめ：どちらを選ぶべき？

* **UIに表示するだけ、メモリを節約したい**
👉 **方法1 (`TransformedBitmap` + `RenderOptions`)** が圧倒的に軽量でおすすめです。
* **拡大後のピクセルデータを確定させて、ファイル保存や他の画像処理に回したい**
👉 **方法2 (`RenderTargetBitmap`)** を使用してください。

<br><br><br>


---


# 拡大後のBitmapSourceをファイルに保存したいので、方法2を試したけど、輪郭がぼやけているので、補間方式が最近傍補間で行われていない

<br><br><br>



---



方法2（`RenderTargetBitmap`）を試したにもかかわらず輪郭がぼやけてしまう場合、WPFのグラフィックス描画における「座標のアンチエイリアス（実数座標のズレ）」または「UI要素とDrawingContextの評価タイミング」が原因の可能性が高いです。

特に、`RenderOptions.SetBitmapScalingMode` は設定する対象（ターゲット）を間違えると効果を発揮しません。

以下の**2つの修正ポイント**を確認・適用してみてください。これでパキッとした最近傍補間の画像が書き出せるようになります。

---

## 修正ポイント1：`RenderOptions` を `drawingVisual` ではなく「描画処理」自体に効かせる

`DrawingVisual` 自体に `BitmapScalingMode.NearestNeighbor` を設定しても、内部の `DrawImage` の処理にうまく伝播しないことがあります。

確実に行うには、**`DrawingGroup` を使って、描画する画像そのものに補間モードをカプセル化する**方法が最も確実です。

### 改良版のコード（これで解決するケースが多いです）

```csharp
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

BitmapSource originalBitmap = ...; // 元の画像

int newWidth = originalBitmap.PixelWidth * 2;
int newHeight = originalBitmap.PixelHeight * 2;

// 1. DrawingGroup を作成し、ここに最近傍補間を設定する ★ココが確実
DrawingGroup drawingGroup = new DrawingGroup();
RenderOptions.SetBitmapScalingMode(drawingGroup, BitmapScalingMode.NearestNeighbor);

// 2. DrawingGroup 内部に画像を等倍～拡大で描画する
using (DrawingContext drawingContext = drawingGroup.Open())
{
    drawingContext.DrawImage(originalBitmap, new Rect(0, 0, newWidth, newHeight));
}

// 3. DrawingVisual に DrawingGroup を描画する
DrawingVisual drawingVisual = new DrawingVisual();
using (DrawingContext drawingContext = drawingVisual.RenderOpen())
{
    drawingContext.DrawDrawing(drawingGroup);
}

// 4. RenderTargetBitmap に焼き付ける
RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
    newWidth, 
    newHeight, 
    originalBitmap.DpiX, 
    originalBitmap.DpiY, 
    PixelFormats.Pbgra32);

renderTargetBitmap.Render(drawingVisual);

// この renderTargetBitmap を保存すれば、ぼやけずに2倍になっているはずです！

```

---

## 修正ポイント2：ガイドライン（SnapToDevicePixels）の適用

WPFはデフォルトで、ピクセルの境界線を滑らかにしようとする（アンチエイリアス）特性があります。拡大サイズが整数であっても、デバイスのDpi設定（125%や150%などの拡大ディスプレイ環境）によっては、内部で「0.5ピクセル」のズレが生じてぼやける原因になります。

上記の「修正ポイント1」でもまだ微妙にぼやける場合は、`drawingVisual` に対してエッジモードをエイリアス（アンチエイリアス無効）に設定してください。

```csharp
// drawingVisual に対しても、境界線をくっきりさせる設定を追加
RenderOptions.SetEdgeMode(drawingVisual, EdgeMode.Aliased);

```

---

## 保存時の注意（DPIの確認）

`RenderTargetBitmap` を作成する際の DPI（上記のコードの `originalBitmap.DpiX` などの部分）が、元の画像と保存先のエンコーダー（PNGなど）で一致しているか確認してください。ここがシステム既定の `96` 以外になっていると、ファイル保存時のエンコード処理で再度リサイズ（補間）が走り、ぼやけることがあります。

もし上記を試しても解決しない場合は、元画像のDPIや、保存処理（Encoder）のコード周辺を一度教えていただければ、さらに詳しく原因を探れます！


