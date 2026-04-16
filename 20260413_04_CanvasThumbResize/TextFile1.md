# ”現在の OnResize 内にある if (dir == ...) のロジックは非常に長いので、**「どの方向に動いたら、どのプロパティ（Left/Top/Width/Height）がどう変化するか」**を定義した構造体や辞書を作ると、メンテナンス性が劇的に向上します。”

これを具体的に示して



---


<br><br><br><br><br><br>




現在の `if` 文の羅列は、新しいハンドル（例えば斜め 45 度固定リサイズなど）を追加しようとするとさらに肥大化し、バグの温床になります。

これを**「各方向ごとの変化の係数（重み）」**という概念でデータ化すると、ロジックが 1 つに集約され、非常にスッキリします。

---

## 改善の考え方：リサイズ係数マトリックス

リサイズ時の挙動は、以下の 4 つの係数（$-1, 0, 1$）で表現できます。

* **WidthFactor**: $\Delta X$ が Width を増やす方向か（右ハンドルなら $1$、左なら $-1$）
* **LeftFactor**: $\Delta X$ が要素の Left 座標を動かすか（左ハンドルなら $1$、右なら $0$）
* **HeightFactor**: $\Delta Y$ が Height を増やす方向か（下ハンドルなら $1$、上なら $-1$）
* **TopFactor**: $\Delta Y$ が要素の Top 座標を動かすか（上ハンドルなら $1$、下なら $0$）

---

## 具体的なコード実装

### 1. 定義用構造体と辞書の作成

まず、各方向の挙動を定義する構造体を用意します。

```csharp
private record ResizeMatrix(double WidthF, double LeftF, double HeightF, double TopF);

private static readonly Dictionary<ResizeDirection, ResizeMatrix> ResizePolicies = new()
{
    //                           WidthF, LeftF, HeightF, TopF
    { ResizeDirection.Left,        new(-1,    1,     0,    0) },
    { ResizeDirection.Right,       new( 1,    0,     0,    0) },
    { ResizeDirection.Top,         new( 0,    0,    -1,    1) },
    { ResizeDirection.Bottom,      new( 0,    0,     1,    0) },
    { ResizeDirection.TopLeft,     new(-1,    1,    -1,    1) },
    { ResizeDirection.TopRight,    new( 1,    0,    -1,    1) },
    { ResizeDirection.BottomLeft,  new(-1,    1,     1,    0) },
    { ResizeDirection.BottomRight, new( 1,    0,     1,    0) }
};
```

### 2. リファクタリング後の OnResize

このデータを使うと、あれだけ長かった `OnResize` がここまで短縮されます。

```csharp
private void OnResize(object sender, DragDeltaEventArgs e)
{
    if (sender is not Thumb thumb || AdornedElement is not FrameworkElement element) return;

    var dir = (ResizeDirection)thumb.Tag;
    if (!ResizePolicies.TryGetValue(dir, out var policy)) return;

    // 1. 横方向の計算
    if (policy.WidthF != 0)
    {
        double deltaX = e.HorizontalChange;
        double newWidth = element.Width + (deltaX * policy.WidthF);
        
        if (newWidth < 10)
        {
            // 最小サイズ制限時の補正計算
            deltaX = (10 - element.Width) * policy.WidthF;
            newWidth = 10;
        }

        element.Width = newWidth;
        if (policy.LeftF != 0)
        {
            double moveX = deltaX * policy.LeftF;
            Canvas.SetLeft(element, Canvas.GetLeft(element) + moveX);
            LeftLocateChanged?.Invoke(this, moveX);
        }
    }

    // 2. 縦方向の計算
    if (policy.HeightF != 0)
    {
        double deltaY = e.VerticalChange;
        double newHeight = element.Height + (deltaY * policy.HeightF);

        if (newHeight < 10)
        {
            deltaY = (10 - element.Height) * policy.HeightF;
            newHeight = 10;
        }

        element.Height = newHeight;
        if (policy.TopF != 0)
        {
            double moveY = deltaY * policy.TopF;
            Canvas.SetTop(element, Canvas.GetTop(element) + moveY);
            TopLocateChanged?.Invoke(this, moveY);
        }
    }
}
```

---

## この実装のメリット

### ① 圧倒的な視認性
「`TopLeft` のときは Width も Height も減らして、座標は両方動かす」というルールが、辞書内の `new(-1, 1, -1, 1)` という 4 つの数値を見るだけで理解できます。

### ② ロジックの局所化
「最小サイズ（10）以下にさせない」という複雑な計算ロジックが、横方向で 1 箇所、縦方向で 1 箇所に集約されました。修正が必要になったとき（例えば最小サイズを 0 にしたい、あるいはプロパティによって変えたい場合など）、一箇所の書き換えで済みます。

### ③ 拡張性
将来的に「中央左右ハンドル（高さは変えない）」などを追加したくなっても、辞書に 1 行追加し、`ArrangeOverride` で Thumb を置くだけで完了します。

---

## 頂点編集（PathGeometry）への応用

今後実装する「頂点移動」も、実はこの考え方の延長でいけます。

* **頂点ハンドル（Thumb）ごとにインデックス（何番目の頂点か）を Tag に持たせる**
* `OnResize` に相当するイベントで、`PathGeometry.Figures[0].Segments[Tag]` の `Point` を直接更新する

このように、**「どのハンドルが、どのデータに対して、どういう影響を与えるか」をテーブル（辞書）化する**設計に慣れておくと、WPF のような自由度の高い GUI 開発がとても楽になりますよ！


