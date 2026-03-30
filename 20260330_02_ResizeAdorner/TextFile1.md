# WPFで要素のサイズをマウスで変更できるようにするには？


WPFで要素のサイズをマウスでドラッグして変更（リサイズ）したい場合、最も一般的で「WPFらしい」方法は**`Thumb`コントロール**を使用することです。

`Thumb`はドラッグ操作に特化したコントロールで、これを利用することで「つまみ」をドラッグした分だけ要素のサイズを動的に変更できます。

---

## 1. 基本的な仕組み：Thumbコントロール
`Thumb`コントロールの **`DragDelta`イベント** を利用します。このイベントは、マウスでドラッグされている間、直前の位置からの移動量（`HorizontalChange` / `VerticalChange`）をリアルタイムで返してくれます。

### 実装手順
1.  サイズを変更したい要素（例：`Rectangle`や`Grid`）の近くに`Thumb`を配置する。
2.  `Thumb`の`DragDelta`イベントを購読する。
3.  イベントハンドラ内で、移動量に合わせて対象要素の`Width`や`Height`を加算する。

---

## 2. 具体的なコード例
以下は、青い長方形の右下をドラッグしてサイズを変えるシンプルな例です。

### XAML
`Thumb`にスタイルを当てて、見た目を「リサイズハンドル」っぽくするのがコツです。

```xml
<Canvas>
    <Rectangle Name="MyElement" 
               Width="100" Height="100" 
               Fill="DodgerBlue" 
               Canvas.Left="50" Canvas.Top="50"/>

    <Thumb Name="ResizeThumb" 
           Width="10" Height="10" 
           Cursor="SizeNWSE"
           Canvas.Left="145" Canvas.Top="145"
           DragDelta="ResizeThumb_DragDelta"/>
</Canvas>
```

### C# (コードビハインド)
ドラッグされた距離を、現在の幅・高さに足し合わせる処理を書きます。

```csharp
private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
{
    // 移動量を取得してサイズを更新
    double newWidth = MyElement.Width + e.HorizontalChange;
    double newHeight = MyElement.Height + e.VerticalChange;

    // 最小サイズを制限（マイナスにならないように）
    if (newWidth > 10) MyElement.Width = newWidth;
    if (newHeight > 10) MyElement.Height = newHeight;

    // Thumb自体の位置も要素に合わせて動かす
    Canvas.SetLeft(ResizeThumb, Canvas.GetLeft(MyElement) + MyElement.Width - ResizeThumb.Width / 2);
    Canvas.SetTop(ResizeThumb, Canvas.GetTop(MyElement) + MyElement.Height - ResizeThumb.Height / 2);
}
```

---

## 3. より高度なアプローチ
「もっと本格的に、8方向からリサイズしたい」「多くの要素に使い回したい」という場合は、以下の方法を検討してください。

* **Adorner（アドーナ）を使用する**: 
    UI要素の上の「装飾層」にリサイズハンドルを描画する方法です。Visual Studioのデザイナーのような、要素を選択したときだけ枠線とハンドルが出る仕組みを作るときに最適です。
* **GridSplitterを利用する**:
    ウィンドウを分割している境界線を動かしたいだけなら、標準の`GridSplitter`を使うのが最も簡単です。

---

### 次のステップ
今回は`Canvas`上での単純な例を紹介しましたが、**「Adorner（装飾層）を使って、選択したときだけリサイズ枠を表示させる方法」**について詳しく解説しましょうか？


<br><br><br>






---


# Adornerを使った方法を解説して



---

Adorner（アドーナ）は、UI要素の上に重なる「装飾層」に描画される特殊なクラスです。これを使うと、元のコントロールのレイアウトを汚さずに、リサイズハンドルや境界線を重ねることができます。

Visual Studioのフォームデザイナーのように、**「クリックした要素にだけリサイズ枠が出る」**といった挙動を作るのに最適です。

---

## Adornerを使った実装の流れ

実装には大きく分けて3つのステップが必要です。

1.  **Adornerクラスを継承したカスタムクラスを作る**: ここでハンドルの見た目（`Thumb`）を定義します。
2.  **VisualChildrenを管理する**: Adornerの中に`Thumb`などの子要素を持たせる処理を書きます。
3.  **対象の要素にAdornerを付与する**: コードから`AdornerLayer`に登録します。



---

## 1. カスタムAdornerの実装例
右下のリサイズハンドルを1つ持つシンプルなAdornerの例です。

```csharp
public class ResizeAdorner : Adorner
{
    // ハンドルとしてのThumb
    private readonly Thumb _bottomRight;
    private readonly VisualCollection _visualChildren;

    public ResizeAdorner(UIElement adornedElement) : base(adornedElement)
    {
        _visualChildren = new VisualCollection(this);
        _bottomRight = new Thumb { Width = 10, Height = 10, Background = Brushes.Red };
        
        // ドラッグイベントの登録
        _bottomRight.DragDelta += OnResize;
        _visualChildren.Add(_bottomRight);
    }

    private void OnResize(object sender, DragDeltaEventArgs e)
    {
        var element = AdornedElement as FrameworkElement;
        if (element == null) return;

        // 要素のサイズを更新
        if (element.Width + e.HorizontalChange > 10)
            element.Width += e.HorizontalChange;
        if (element.Height + e.VerticalChange > 10)
            element.Height += e.VerticalChange;
    }

    // 配置の決定（右下に配置）
    protected override Size ArrangeOverride(Size finalSize)
    {
        _bottomRight.Arrange(new Rect(finalSize.Width - 5, finalSize.Height - 5, 10, 10));
        return finalSize;
    }

    // Visualの子要素をフレームワークに教えるための定型文
    protected override int VisualChildrenCount => _visualChildren.Count;
    protected override Visual GetVisualChild(int index) => _visualChildren[index];
}
```

---

## 2. Adornerを表示させる（コードビハインド）
作成したAdornerを、実際にターゲットとなる要素に貼り付けます。

```csharp
private void Window_Loaded(object sender, RoutedEventArgs e)
{
    // 対象のコントロール（例：TargetRect）を取得
    var layer = AdornerLayer.GetAdornerLayer(TargetRect);
    if (layer != null)
    {
        // レイヤーにAdornerを追加
        layer.Add(new ResizeAdorner(TargetRect));
    }
}
```

---

## Adornerを使うメリット

* **カプセル化**: リサイズロジックをコントロール側ではなくAdorner側に閉じ込められるため、どんなコントロールにも後付けでリサイズ機能を持たせられます。
* **レイアウトに影響しない**: Adornerは「浮いている」状態なので、リサイズハンドルが表示されたからといって周囲のコントロールがズレることがありません。
* **柔軟な描画**: `OnRender`をオーバーライドすれば、枠線を点線にしたり、独自の図形を描画したりと自由自在です。

---

### 次のステップ
実際のアプリでは、**「8方向にハンドルを出す」**ために複数の`Thumb`を配置するのが一般的です。その場合の「各ハンドルの配置計算（`ArrangeOverride`）」についても具体的に解説しましょうか？




<br><br><br>














# ”protected override Size ArrangeOverride(Size finalSize)”にブレークポイントを設置して動きを見ると、1度の動作に2回ArrangeOverrideが処理されているのはなんで？



---

デバッグ中に `ArrangeOverride` が2回呼ばれるのを見ると「無駄な計算が走っているのでは？」と気になりますよね。ですが、これはWPFのレイアウトシステム（Layout Engine）の仕様によるもので、異常な動作ではありません。

主に以下の2つの理由が考えられます。

---

## 1. 2パス・レイアウト・プロセスの影響
WPFのレイアウト決定は、**Measure（計測）** と **Arrange（配置）** の2フェーズで行われます。

1.  **Measure**: 要素が「自分はこれくらいのサイズになりたい」と親に伝える。
2.  **Arrange**: 親が「じゃあ、この範囲に収まってね」と最終的な位置とサイズを決定する。



リサイズ（`DragDelta`）が発生すると、対象要素の `Width` や `Height` が変わります。すると、その要素だけでなく、**それを載せている `AdornerLayer` 自体も再レイアウトが必要**と判断され、以下のように連続して呼び出されることがあります。
* 1回目：子要素（Thumbなど）のサイズ変更に伴う再配置。
* 2回目：親要素（AdornedElement）のサイズ確定後の最終調整。

## 2. 「再入」による更新
`DragDelta` 内でプロパティを書き換えると、WPFは「あ、見た目が変わったから描き直さなきゃ」というフラグ（Dirty Flag）を立てます。

* `ArrangeOverride` の実行中に、内部の `Thumb` の配置が確定する。
* その結果、親である `Adorner` の境界（Bounding Box）が微妙に変わり、システムが「念のためもう一度正しく配置されているか確認しよう」と2回目のパスを走らせる。

特に `Adorner` は、**「装飾対象の要素（AdornedElement）に追従する」**という性質があるため、対象が動いたりサイズが変わったりすると、非常に敏感に再計算が行われます。

---

## 注意点：ここで重い処理は禁物
この「複数回呼ばれる」性質があるため、`ArrangeOverride` や `OnRender` の中に以下のような処理を書くのは避けるべきです。

* **インスタンスの生成 (`new ...`)**: 1回のドラッグで何十回もインスタンスが作られ、メモリを圧迫します。
* **副作用のある計算**: 呼ばれるたびに変数の値をインクリメントするような処理を書くと、意図しない挙動になります。

> [!TIP]
> **デバッグのコツ**
> 2回呼ばれるのが「全く同じ引数（Size）」なのか「少し違う値」なのかをウォッチウィンドウで確認してみてください。値が同じであれば、単なるWPFの安全策としての再描画です。

---

### 次のステップ
もしパフォーマンスが気になるようであれば、**「8方向リサイズを実装する際に、計算負荷を抑えるためのThumbの共通化」**などのテクニックを紹介しましょうか？デバッグ中に `ArrangeOverride` が2回呼ばれるのを見ると「無駄な計算が走っているのでは？」と気になりますよね。ですが、これはWPFのレイアウトシステム（Layout Engine）の仕様によるもので、異常な動作ではありません。

主に以下の2つの理由が考えられます。

---

## 1. 2パス・レイアウト・プロセスの影響
WPFのレイアウト決定は、**Measure（計測）** と **Arrange（配置）** の2フェーズで行われます。

1.  **Measure**: 要素が「自分はこれくらいのサイズになりたい」と親に伝える。
2.  **Arrange**: 親が「じゃあ、この範囲に収まってね」と最終的な位置とサイズを決定する。



リサイズ（`DragDelta`）が発生すると、対象要素の `Width` や `Height` が変わります。すると、その要素だけでなく、**それを載せている `AdornerLayer` 自体も再レイアウトが必要**と判断され、以下のように連続して呼び出されることがあります。
* 1回目：子要素（Thumbなど）のサイズ変更に伴う再配置。
* 2回目：親要素（AdornedElement）のサイズ確定後の最終調整。

## 2. 「再入」による更新
`DragDelta` 内でプロパティを書き換えると、WPFは「あ、見た目が変わったから描き直さなきゃ」というフラグ（Dirty Flag）を立てます。

* `ArrangeOverride` の実行中に、内部の `Thumb` の配置が確定する。
* その結果、親である `Adorner` の境界（Bounding Box）が微妙に変わり、システムが「念のためもう一度正しく配置されているか確認しよう」と2回目のパスを走らせる。

特に `Adorner` は、**「装飾対象の要素（AdornedElement）に追従する」**という性質があるため、対象が動いたりサイズが変わったりすると、非常に敏感に再計算が行われます。

---

## 注意点：ここで重い処理は禁物
この「複数回呼ばれる」性質があるため、`ArrangeOverride` や `OnRender` の中に以下のような処理を書くのは避けるべきです。

* **インスタンスの生成 (`new ...`)**: 1回のドラッグで何十回もインスタンスが作られ、メモリを圧迫します。
* **副作用のある計算**: 呼ばれるたびに変数の値をインクリメントするような処理を書くと、意図しない挙動になります。

> [!TIP]
> **デバッグのコツ**
> 2回呼ばれるのが「全く同じ引数（Size）」なのか「少し違う値」なのかをウォッチウィンドウで確認してみてください。値が同じであれば、単なるWPFの安全策としての再描画です。

---

### 次のステップ
もしパフォーマンスが気になるようであれば、**「8方向リサイズを実装する際に、計算負荷を抑えるためのThumbの共通化」**などのテクニックを紹介しましょうか？



<br><br><br><br><br><br>












---

# Adornerを外す方法は？それとも非表示するだけでいい？

---




Adornerを「使わない」状態にするには、**「レイヤーから削除する」**方法と**「非表示（VisibilityをCollapsed）にする」**方法の2通りがあります。

結論から言うと、**「一時的に消したいだけなら非表示」「その要素のリサイズ機能自体を完全にやめるなら削除」**という使い分けがベストです。

---

## 1. Adornerを「非表示」にする（おすすめ）
最も手軽で、再表示もしやすい方法です。Adornerは`UIElement`を継承しているため、通常のコントロールと同じように`Visibility`プロパティを持っています。

* **メリット**: インスタンスを保持したままなので、次に表示するときに計算負荷がない。
* **デメリット**: メモリ上には残り続ける（微々たるものですが）。

```csharp
// インスタンスを保持しておき、切り替える
myAdorner.Visibility = Visibility.Collapsed; // 非表示
myAdorner.Visibility = Visibility.Visible;   // 表示
```

---

## 2. Adornerを「レイヤーから削除」する
要素から完全にリサイズ機能を切り離したい場合に適しています。

* **メリット**: 完全にクリーンアップされる。
* **デメリット**: 再表示したいときに、再度 `new` して `Add` する手間と処理コストがかかる。

### 特定のAdornerを削除する
```csharp
var layer = AdornerLayer.GetAdornerLayer(TargetRect);
if (layer != null)
{
    // 追加した時のインスタンスを指定して削除
    layer.Remove(myAdorner); 
}
```

### 全てのAdornerを一括削除する
対象の要素にくっついているすべてのAdornerを消したい場合は、配列を取得してループさせます。
```csharp
var layer = AdornerLayer.GetAdornerLayer(TargetRect);
var adorners = layer.GetAdorners(TargetRect);

if (adorners != null)
{
    foreach (var adorner in adorners)
    {
        layer.Remove(adorner);
    }
}
```

---

## どちらを選ぶべきか？

| パターン | 推奨される方法 | 理由 |
| :--- | :--- | :--- |
| **選択中だけ表示したい** | **非表示 (Visibility)** | 選択・非選択が頻繁に起こるため、毎回生成/破棄を繰り返すより効率的です。 |
| **編集モードを終了する** | **削除 (Remove)** | ツールを切り替えるなど、しばらくその機能を使わない場合はメモリを解放すべきです。 |

> [!NOTE]
> **よくある落とし穴**
> `AdornerLayer.GetAdornerLayer(element)` は、その要素の親を遡ってレイヤーを探します。もし `element` がまだビジュアルツリー（画面）に接続されていない状態だと `null` を返すので、必ず `Loaded` イベント以降に実行するか、nullチェックを忘れずに行ってください。

---

### 次のステップ
例えば、**「要素をクリックした時にだけAdornerを表示させ、外側をクリックしたら非表示にする」**といった、実践的な切り替えロジックの実装コードを見てみますか？


