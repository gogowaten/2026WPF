# WPFのDefiningGeometryに付いて解説して



---


WPF（Windows Presentation Foundation）における **`DefiningGeometry`** は、カスタム形状の図形（Shape）を自作する際に避けて通れない、非常に重要なプロパティです。

簡単に言うと、**「その図形が具体的にどのような形をしているのか」という数学的な定義を保持する場所**です。

---

## 1. DefiningGeometry の役割
WPF で `Rectangle` や `Ellipse` のような標準の図形ではない、独自の図形を作りたいときは `Shape` クラスを継承したクラスを作成します。その際、`DefiningGeometry` プロパティをオーバーライドして、描画したいジオメトリを返す必要があります。

* **Shape クラス:** `Fill`（塗りつぶし）や `Stroke`（線）などの見た目を管理します。
* **DefiningGeometry:** その「枠組み（パス）」を定義します。



---

## 2. 実装の仕組み
`Shape` クラスを継承してカスタム図形を作る場合の基本的な流れは以下の通りです。

```csharp
public class MyCustomShape : Shape
{
    // 1. ジオメトリを返すプロパティをオーバーライド
    protected override Geometry DefiningGeometry
    {
        get
        {
            // ここで形状（Line, Ellipse, PathGeometryなど）を作成して返す
            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(new Point(0, 0), true, true);
                ctx.LineTo(new Point(100, 0), true, false);
                ctx.LineTo(new Point(50, 80), true, false);
            }
            return geometry;
        }
    }
}
```

---

## 3. なぜ DefiningGeometry を使うのか？
直接 `OnRender` で `DrawingContext.DrawGeometry` を使って描画することも可能ですが、`DefiningGeometry` を使うことには大きなメリットがあります。

* **自動レイアウト:** `Shape` クラスが、返されたジオメトリのサイズ（Bounds）を元に、自動で `ActualWidth` や `ActualHeight` を計算してくれます。
* **ストレッチ機能:** `Stretch` プロパティ（Uniform, Fill など）が自動的に適用されるようになります。
* **一貫性:** 他の標準コントロール（`Rectangle` 等）と同じように、XAML から `Fill` や `Stroke` を直感的に扱えるようになります。

---

## 4. 注意点：パフォーマンスと再描画
`DefiningGeometry` の `get` アクセサ内で毎回新しい `Geometry` オブジェクトを生成すると、パフォーマンスが低下することがあります。

> **Tips:**
> 図形のサイズやパラメータが変わったときだけジオメトリを再構築し、それ以外はキャッシュしたジオメトリを返すように設計するのが一般的です。その際、値が変わったことを通知するために `InvalidateVisual()` を呼び出す必要があります。

---

## まとめ
* **`DefiningGeometry`** は `Shape` クラスの抽象メンバで、図形の「カタチ」を定義するもの。
* カスタム図形を作るときは、これをオーバーライドして `PathGeometry` や `StreamGeometry` を返す。
* これを利用することで、WPF の強力なレイアウトシステムやスタイル機能の恩恵をフルに受けられる。

もし具体的なカスタム図形（例えば「星型」や「矢印」など）のコード例が必要であれば、すぐに作成できますがいかがでしょうか？


---
<br><br><br>




# PathGeometryとStreamGeometryの違いについて解説して


---


`DefiningGeometry` を扱う上で、**`PathGeometry`** と **`StreamGeometry`** の使い分けはパフォーマンスに直結する非常に重要なポイントです。

一言でいうと、**「後から形を変えたい（編集したい）なら PathGeometry」、「描画するだけで速さを求めるなら StreamGeometry」**という違いがあります。

---

## 1. StreamGeometry（軽量・高速版）

`StreamGeometry` は、その名の通り「ストリーム（流し込み）」形式で図形を定義します。

* **特徴:** メモリ消費が非常に少なく、描画速度が速いです。
* **読み取り専用:** 一度定義して `Close()` すると、後から特定の点（Point）を動かしたり、中身を書き換えたりすることはできません。
* **データバインディング不可:** 個々のセグメント（線やカーブ）に対してバインディングを設定することはできません。

### 使いどころ
* アイコンや静的なグラフィック。
* 数千個の図形を同時に表示するなど、パフォーマンスが要求される場合。
* `DefiningGeometry` で、その都度ジオメトリを生成して返す場合。

---

## 2. PathGeometry（高機能・編集可能版）

`PathGeometry` は、オブジェクトモデルとして図形を保持します。

* **特徴:** ジオメトリを構成する `Figure` や `Segment` が個別のオブジェクトとして存在します。
* **編集可能:** 実行中に特定の頂点の座標を変更したり、動的に線を追加したりできます。
* **データバインディング:** セグメントの座標に対して Binding を設定し、ViewModel から形を操作することが可能です。
* **アニメーション:** 特定のパスに沿ったアニメーションなど、複雑な操作に適しています。

### 使いどころ
* ユーザーの操作に合わせて頂点をドラッグして変形させる図形。
* データバインディングを使って形を制御したい場合。
* リソースとして定義し、後から一部を書き換えたい場合。

---

## 3. 主な違いの比較表

| 特徴 | StreamGeometry | PathGeometry |
| :--- | :--- | :--- |
| **パフォーマンス** | **非常に高い（高速）** | 普通 |
| **シリアライズ** | バイナリ形式で軽量 | XML/オブジェクト形式 |
| **動的な変更** | 不可（作り直しが必要） | **可能** |
| **バインディング** | 不可 | 可能 |
| **主な用途** | 固定のアイコン、大量の描画 | 編集可能な図形、アニメーション |

---

## 4. コードでの書き方の違い

### StreamGeometry の場合
コンテキストを開いて「書き込む」スタイルです。

```csharp
var geometry = new StreamGeometry();
using (var ctx = geometry.Open())
{
    ctx.BeginFigure(new Point(0, 0), true, true);
    ctx.LineTo(new Point(100, 100), true, false);
}
// これ以降、geometry の中身は変更不可
```

### PathGeometry の場合
「オブジェクトを組み立てる」スタイルです。

```csharp
var line = new LineSegment(new Point(100, 100), true);
var figure = new PathFigure();
figure.StartPoint = new Point(0, 0);
figure.Segments.Add(line);

var geometry = new PathGeometry();
geometry.Figures.Add(figure);

// 後から座標を変えられる
line.Point = new Point(200, 200); 
```

---

## どちらを選ぶべき？

基本的には **「`StreamGeometry` で済むなら `StreamGeometry` を使う」** というのが WPF のセオリーです。特に `DefiningGeometry` で図形を返す際、複雑な形状を大量に扱うのであれば、`StreamGeometry` を選ぶことでアプリの動作が軽くなります。

次は、実際にこれらを使って「矢印」や「多角形」などの具体的なカスタムコントロールを実装する手順を見てみますか？


---



<br><br><br>



# WPFのInvalidateVisualってなに？


---


WPFの**InvalidateVisual**は、「この要素の見た目（描画）が古くなったから、もう一回描き直してね」とWPFに通知するためのメソッドです。呼ぶと、その要素は**再描画が必要**だとマークされ、次の描画タイミング（レンダリングパス）で **OnRender** が呼ばれて描き直されます。

### どういうときに使う？
主に、**OnRenderで自前描画している要素**（`FrameworkElement`/`UIElement`を継承して `OnRender(DrawingContext dc)` をオーバーライドしているようなケース）で、

- 内部状態が変わって「描画内容を変えたい」
- でもDependencyPropertyの変更通知などで自動的に再描画されない

みたいなときに、明示的に再描画を促すために使います。

### 何が起きる？（ポイント）
- **レイアウト（Measure/Arrange）をやり直すわけではない**  
  → 見た目だけの更新要求です（サイズや配置が変わるなら別）。
- 実際にすぐ描かれるとは限らず、WPFがまとめて最適なタイミングで描きます。
- `OnRender` を使っていない普通のコントロールでも呼べますが、効果が分かりやすいのは自前描画系です。

### 似ているメソッドとの違い
- **InvalidateVisual**：再描画（OnRender）したい  
- **InvalidateMeasure**：サイズ計測からやり直したい（Measure）  
- **InvalidateArrange**：配置からやり直したい（Arrange）  
- **UpdateLayout**：レイアウトを“今すぐ”反映（多用注意。重くなりがち）

---

いま知りたいのは、**OnRenderで描画している自作コントロール**の話ですか？それとも、例えばCanvas上の図形みたいな「既存コントロールの見た目が更新されない」ケースで困ってますか？


---


<br><br><br>



WPFにおける **`InvalidateVisual()`** は、簡単に言うと**「このコントロールの見た目が古くなったから、次の描画タイミングで描き直して！」とシステムに予約を入れる命令**です。

カスタムコントロール（特に `Shape` を継承したもの）を作っていると、プロパティが変わっただけでは画面が更新されないことがあります。その時にこのメソッドを使います。

---

## 1. なぜこれが必要なのか？
WPFはパフォーマンスを最適化するため、**「何もしなくていいなら描き直さない」**というスタンスをとっています。

例えば、カスタム図形に `Angle`（角度）という独自のプロパティを作ったとします。
1. プロパティの値が `10` から `20` に変わる。
2. しかし、WPFシステムは「その値が変わったことで見た目にどう影響するか」を自動では判断できません。
3. 結果、内部の数値は変わっているのに、**画面上の図形は止まったまま**になります。

ここで `InvalidateVisual()` を呼ぶことで、「形状が変わったから再描画が必要だよ」とWPFに通知するわけです。

---

## 2. 実行されるタイミング
`InvalidateVisual()` を呼び出すと、即座に描き直しが始まるわけではありません。

* **要求:** `InvalidateVisual()` を呼ぶ。
* **予約:** WPFのレンダリングキューに「描き直し」が追加される。
* **実行:** 他の優先度の高い処理（入力など）が終わった後の適切なタイミングで、**`OnRender`** メソッド（または `DefiningGeometry` の再評価）が呼ばれます。



---

## 3. 具体的な使いどころ（コード例）
カスタムシェイプで、プロパティ変更時に再描画させる一般的な書き方です。

```csharp
public class MyCircle : Shape
{
    // 依存関係プロパティ（Radius）
    public static readonly DependencyProperty RadiusProperty =
        DependencyProperty.Register("Radius", typeof(double), typeof(MyCircle),
            new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsRender));
            // ↑ 実は AffectsRender を指定すると、内部で InvalidateVisual が呼ばれます

    protected override Geometry DefiningGeometry
    {
        get {
            // Radius を使って円を描く
            return new EllipseGeometry(new Point(50, 50), Radius, Radius);
        }
    }
}
```

> **補足:** 上記の例のように `FrameworkPropertyMetadataOptions.AffectsRender` を設定するのが最もスマートな方法ですが、複雑なロジックで手動で更新をかけたい場合に `this.InvalidateVisual()` を直接使います。

---

## 4. 注意：使いすぎに注意！
`InvalidateVisual()` は強力ですが、乱発するとパフォーマンスに影響します。

* **重い処理を避ける:** `OnRender` や `DefiningGeometry` の中で、巨大な計算やファイルの読み込みをしてはいけません。
* **レイアウト更新との違い:**
    * **`InvalidateVisual`**: 見た目（色や形）だけを変える。
    * **`InvalidateMeasure`**: サイズや配置（レイアウト）自体を変える。

---

## まとめ
* **`InvalidateVisual()`** は「描き直し予約」のスイッチ。
* カスタム図形のパラメータが変わった時に、画面を最新にするために呼ぶ。
* 依存関係プロパティの `AffectsRender` フラグを使うと、自動でこれを呼んでくれるので便利。

この `InvalidateVisual` と、先ほどの `DefiningGeometry` を組み合わせれば、自由自在に動くカスタム図形が作れます。実際に動くコードの全体像を書いてみましょうか？


---

