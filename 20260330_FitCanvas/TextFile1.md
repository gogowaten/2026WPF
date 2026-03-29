# WPFで子要素全体が収まるサイズにリサイズするメソッドを持つCanvasを作って
---



WPFの標準的な`Canvas`は、子要素を絶対座標で配置することに特化しているため、**「中身に合わせて自分自身のサイズを変える」**という挙動を自動では行いません。

これを実現するには、子要素の配置（`Left`, `Top`）とサイズ（`Width`, `Height`）を計算し、その最大範囲を自身の`DesiredSize`として報告するカスタムクラスを作成するのが最もスマートです。

以下に、中身にフィットさせるための `UpdateSizeToContent` メソッドを持つカスタムCanvasの実装例を紹介します。

---

## カスタムCanvasの実装例

このクラスでは、子要素の右端（$x + width$）と下端（$y + height$）の最大値を計算して、自分自身の `Width` と `Height` を更新します。

```csharp
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

public class FitCanvas : Canvas
{
    /// <summary>
    /// 子要素がすべて収まるようにCanvasのサイズを更新します。
    /// </summary>
    public void UpdateSizeToContent()
    {
        double maxWidth = 0;
        double maxHeight = 0;

        foreach (UIElement child in InternalChildren)
        {
            // 子要素の配置座標を取得（未設定の場合は0）
            double x = GetLeft(child);
            if (double.IsNaN(x)) x = 0;

            double y = GetTop(child);
            if (double.IsNaN(y)) y = 0;

            // 子要素を計測
            child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            
            // 右端と下端の最大値を更新
            maxWidth = Math.Max(maxWidth, x + child.DesiredSize.Width);
            maxHeight = Math.Max(maxHeight, y + child.DesiredSize.Height);
        }

        // 自身のサイズを更新
        this.Width = maxWidth;
        this.Height = maxHeight;
    }
}
```

---

## 使い方（XAML / C#）

### XAMLでの定義
通常のCanvasと同じように配置します。

```xml
<local:FitCanvas x:Name="myCanvas" Background="LightGray">
    <Button Content="Hello" Canvas.Left="50" Canvas.Top="20" Width="100"/>
    <Rectangle Fill="Blue" Canvas.Left="200" Canvas.Top="150" Width="50" Height="50"/>
</local:FitCanvas>
```

### コードビハインドからの実行
要素を動的に追加したり、配置を変えたりした後にメソッドを呼び出します。

```csharp
// 何か要素を追加した後に呼び出す
myCanvas.UpdateSizeToContent();
```

---

### 実装のポイント
* **`DesiredSize` の活用**: 子要素の実際のサイズを取得するために `Measure` を呼び出しています。これにより、明示的に `Width/Height` を指定していない要素にも対応できます。
* **自動化のヒント**: もしメソッドをわざわざ叩くのが面倒な場合は、`MeasureOverride` をオーバーライドして、レイアウトパスの中で自動計算させる手法もあります。ただ、Canvasは本来「無限の広さ」を前提とすることが多いため、今回のように**「必要な時にメソッドでリサイズする」**方が、意図しないレイアウト崩れを防ぎやすく、制御が楽ですよ。

この `FitCanvas` を使って、特定のタイミング（ボタンクリック時やデータ読み込み完了時など）にサイズをフィットさせてみてください。

他にも「要素が削除された時も自動で詰めたい」といった追加の要望はありますか？


<br><br><br>












# 子要素の座標がマイナス値だった場合も考えて下記のようにしてみた、評価して
```cs
    public class FitCanvas : Canvas
    {
        public void UpdateSizeToContent()
        {
            double right = double.MinValue;
            double bottom = double.MinValue;
            double top = double.MaxValue;
            double left = double.MaxValue;

            // 子要素全体が収まるサイズを計算
            foreach (UIElement child in InternalChildren)
            {
                // 子要素の座標を取得（未設定の場合は0で計算）
                double x = GetLeft(child);
                if (double.IsNaN(x)) { x = 0; }
                double y = GetTop(child);
                if (double.IsNaN(y)) { y = 0; }

                // 子要素の左端と上端を計算
                left = Math.Min(left, x);
                top = Math.Min(top, y);

                // 子要素のサイズを測定してから
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                // 右端と下端の取得
                right = Math.Max(right, x + child.DesiredSize.Width);
                bottom = Math.Max(bottom, y + child.DesiredSize.Height);
            }

            // 自身のサイズを更新
            this.Width = right - left;
            this.Height = bottom - top;
        }
    }
}
```

マイナス座標を考慮したロジックへのアップデート、素晴らしい着眼点です！
単に「幅と高さ」を広げるだけでなく、**「左上の余白（オフセット）」を打ち消して中身を原点に引き寄せる**という発想が必要になりますね。

ご提示いただいたコードを評価し、さらに実用性を高めるための修正案を提案します。

---

## コードの評価

### 良い点
* **バウンディングボックスの概念**: `left` と `top` を最小値で追跡することで、全要素を囲む「最小の矩形」を正しく計算しようとしています。
* **NaN（未設定）の考慮**: `GetLeft` 等が返す `NaN` を `0` として扱う処理が漏れておらず、堅実です。

### 改善が必要な点
1.  **「中身が移動していない」問題**:
    ご提示のコードでは `Width` と `Height`（キャンバスの大きさ）は正しくなりますが、子要素の座標（`Canvas.Left/Top`）がマイナスのままだと、**キャンバスの枠外（左上）に突き抜けたまま**になってしまいます。
    * 解決策：計算した `left` と `top` の分だけ、全子要素をシフト（移動）させる必要があります。
2.  **初期値の安全性**:
    子要素が0個の場合、`Width` が `double.MinValue - double.MaxValue` のような計算になり、オーバーフローや異常な値になります。
3.  **右端・下端の初期値**:
    `right` や `bottom` も、要素がない場合は `0` になるよう制御するのが安全です。

---

## 修正後のコード案

マイナス座標を吸収し、コンテンツを (0, 0) の位置へ自動的に整列させる処理を追加しました。

```csharp
public class FitCanvas : Canvas
{
    public void UpdateSizeToContent()
    {
        if (InternalChildren.Count == 0)
        {
            this.Width = 0;
            this.Height = 0;
            return;
        }

        double minX = double.MaxValue;
        double minY = double.MaxValue;
        double maxX = double.MinValue;
        double maxY = double.MinValue;

        // 1. 全要素を走査して範囲を確定させる
        foreach (UIElement child in InternalChildren)
        {
            double x = GetLeft(child);
            if (double.IsNaN(x)) x = 0;
            double y = GetTop(child);
            if (double.IsNaN(y)) y = 0;

            child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            minX = Math.Min(minX, x);
            minY = Math.Min(minY, y);
            maxX = Math.Max(maxX, x + child.DesiredSize.Width);
            maxY = Math.Max(maxY, y + child.DesiredSize.Height);
        }

        // 2. 子要素をシフトさせてマイナス座標を解消する (ここが重要！)
        // 全要素の座標から minX, minY を引くことで、左上端が (0, 0) に揃う
        foreach (UIElement child in InternalChildren)
        {
            double curX = GetLeft(child);
            if (double.IsNaN(curX)) curX = 0;
            double curY = GetTop(child);
            if (double.IsNaN(curY)) curY = 0;

            SetLeft(child, curX - minX);
            SetTop(child, curY - minY);
        }

        // 3. 自身のサイズを更新
        this.Width = maxX - minX;
        this.Height = maxY - minY;
    }
}
```

---

## 修正のポイント解説

### 子要素の再配置（シフト処理）
例えば、ある要素が `Canvas.Left="-50"` にあった場合、キャンバスのサイズをどれだけ広げても、その要素は左にハミ出したままです。
`SetLeft(child, curX - minX)` を行うことで、最左端にある要素の座標が `0` になり、すべての要素がキャンバスの中に正しく収まるようになります。

### バウンディングボックスの計算
計算式 $Width = maxX - minX$ は数学的に正しく、これにより「最も左にある要素から、最も右にある要素までの距離」がキャンバスの幅になります。

### 注意点：副作用
このメソッドを呼ぶと**子要素の `Canvas.Left/Top` プロパティが書き換わります**。
もし元の座標（マイナス値など）をデータとして保持しておきたい場合は、この Canvas は「表示専用」と割り切るか、元の値を別の変数に保持しておく必要がある点に注意してください。

この「座標の補正」まで含めた挙動で、イメージ通りになりそうでしょうか？
