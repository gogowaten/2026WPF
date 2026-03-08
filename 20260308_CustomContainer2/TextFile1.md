# WPFのIsItemItsOwnContainerOverrideの効果や使い方を解説して


---


WPFでカスタムコントロール（特に `ItemsControl` の派生クラス）を作成していると、必ずと言っていいほどぶつかるのが **「コンテナ（Container）」** の扱いです。

`IsItemItsOwnContainerOverride` は、一言で言うと**「渡されたデータそのものを、そのまま表示用UI要素として使っていいか？」**をWPFに教えるためのメソッドです。

---

## 1. そもそも「コンテナ」とは？

`ListBox` を例に考えると分かりやすいです。

* **Item（データ）:** "Hello"（単なる文字列）
* **Container（器）:** `ListBoxItem`（枠線、選択状態、ホバー効果などを持つUI要素）

通常、WPFはデータを受け取ると、それを表示するために「器（コンテナ）」で包みます。

## 2. IsItemItsOwnContainerOverride の役割

このメソッドは、`ItemsControl` クラス内で以下のように定義されています。

```csharp
protected virtual bool IsItemItsOwnContainerOverride(object item);

```

* **`true` を返す場合:** 「そのデータはすでにコンテナ（UI要素）なので、新しく包む必要はないよ」と伝えます。
* **`false` を返す場合:** 「それはただのデータだから、適切なコンテナで包んでね」と伝えます。

---

## 3. 具体的な使い方とコード例

独自の `MyCustomSelector` を作る場合、基本的には `GetContainerForItemOverride` とセットで実装します。

### 実装例

```csharp
public class MyCustomSelector : ItemsControl
{
    // 1. アイテム自体がコンテナかどうかを判定
    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        // 渡されたアイテムが「MyCustomItem」型なら、そのまま使うので true
        return item is MyCustomItem;
    }

    // 2. コンテナが必要な場合に、新しいコンテナを生成して返す
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new MyCustomItem();
    }
}

```

### これにより何が起きるか？

ユーザーが以下のようにコントロールを使ったとします。

```xml
<local:MyCustomSelector>
    <local:MyCustomItem Content="Direct Item" />
    
    <sys:String>Simple Data</sys:String>
</local:MyCustomSelector>

```

1. **パターンAの場合:** `IsItemItsOwnContainerOverride` が `true` を返すため、WPFはそのまま `MyCustomItem` を画面に表示します。
2. **パターンBの場合:** `IsItemItsOwnContainerOverride` が `false` を返すため、WPFは `GetContainerForItemOverride` を呼び出して新しい `MyCustomItem` を作成し、その中に文字列を入れます。

---

## 4. なぜこれが必要なのか？

この仕組みがないと、**「UI要素を直接書き込んだとき」に二重構造になってしまう**からです。

もし常に `false` を返してしまうと：

* `<ListBoxItem>Hello</ListBoxItem>` と書いたとき、
* WPFがさらにそれを `ListBoxItem` で包んでしまい、
* `ListBoxItem` の中に `ListBoxItem` が入るという無駄（かつレイアウト崩れの原因）が発生します。

---

## まとめ

* **目的:** データの二重梱包を防ぎ、UI要素が直接渡された場合に正しく扱うため。
* **戻り値:** `item` が期待するコンテナ型なら `true`、そうでなければ `false`。
* **セット:** 必ず `GetContainerForItemOverride` と組み合わせてオーバーライドする。

次は、このコンテナに対してスタイルを自動で適用する `PrepareContainerForItemOverride` の使い方も解説しましょうか？


---


# コンテナに対してスタイルを自動で適用する PrepareContainerForItemOverride の使い方も解説して


---


前述の `IsItemItsOwnContainerOverride` で「器（コンテナ）」が用意された後、次に行うのが **`PrepareContainerForItemOverride`** による **「中身のセッティング」** です。

このメソッドは、生成されたコンテナに対して、データバインディングやスタイルの適用、状態の同期などを行うために使用されます。

---

## 1. PrepareContainerForItemOverride の役割

このメソッドの定義は以下の通りです。

```csharp
protected virtual void PrepareContainerForItemOverride(DependencyObject element, object item);

```

* **`element`**: 用意されたコンテナ（例：`ListBoxItem`）。
* **`item`**: 元のデータ（例：`String` や `ViewModel`）。

「コンテナが完成した直後、画面に表示される前」に呼び出されるため、ここで最終的な調整を行います。

---

## 2. 具体的な実装例

独自の選択コントロールで、アイテムにスタイルを適用したり、特定のプロパティをバインドしたりする例を見てみましょう。

```csharp
protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
{
    // 1. 基底クラスの処理を必ず呼ぶ（スタイルの適用などが行われるため）
    base.PrepareContainerForItemOverride(element, item);

    // 2. element を自分のコンテナ型にキャスト
    var container = element as MyCustomItem;
    if (container != null)
    {
        // 3. データとコンテナの橋渡しを行う
        // 例: コンテナの特定のプロパティをデータに合わせて調整する
        if (item is MyViewModel vm)
        {
            // プロパティを直接セットしたり、バインディングを構築したりする
            container.IsSpecial = vm.IsImportant;
        }

        // 4. ItemContainerStyle があれば適用を確実にする（通常は base で行われる）
    }
}

```

---

## 3. なぜこれを使うのか？（メリット）

### ① スタイルの自動適用

`ItemsControl.ItemContainerStyle` を設定している場合、`base.PrepareContainerForItemOverride` を呼ぶことで、そのスタイルがコンテナに自動的に反映されます。

### ② コンテナとデータの同期

コンテナ（UI要素）とデータ（ViewModel等）の間で、標準の DataContext 以外に特殊なプロパティを同期させたい場合に便利です。

### ③ コンテナの再利用（仮想化）への対応

WPFの `VirtualizingStackPanel` などでコンテナが再利用（リサイクル）される場合、このメソッドは **「再利用されるたびに」** 呼ばれます。そのため、古いデータの設定を上書きし、新しいデータの内容に正しく更新する役割を担います。

---

## 4. 注意点：対になるメソッド

コンテナが不要になったとき（アイテムが削除されたり、画面外に消えてリサイクルされるとき）は、 **`ClearContainerForItemOverride`** を使って後片付けをします。

* **`Prepare...`**: データの紐付け、イベントの購読。
* **`Clear...`**: 紐付けの解除、イベントの購読解除（メモリリーク防止）。

---

## まとめ：コンテナ制御の 3 点セット

| メソッド名 | 役割 |
| --- | --- |
| **`IsItemItsOwnContainerOverride`** | 「これ、もうコンテナになってる？」と確認する。 |
| **`GetContainerForItemOverride`** | 「新しいコンテナ（空箱）」を作成する。 |
| **`PrepareContainerForItemOverride`** | 「コンテナにデータを詰めて、飾り（Style）を付ける」。 |

カスタムコントロールの作成において、これら 3 つをマスターすれば、標準の `ListBox` や `TreeView` に負けない柔軟な UI コンポーネントが作れるようになります。

次は、実際にこれらを使って「選択状態を管理する Selector クラス」の具体的な作り方に進んでみますか？


---


# 実際にこれらを使って「選択状態を管理する Selector クラス」の具体的な作り方を解説して


---


これまでに学んだ 3 つのメソッド（`IsItem...`, `Get...`, `Prepare...`）を組み合わせて、独自の選択機能を持つ `CustomSelector` を作ってみましょう。

WPF の `Selector` クラス（`ListBox` の親）を自作するようなイメージで、**「アイテムをクリックしたらその項目がハイライトされる」** 仕組みを実装します。

---

## 1. 独自のコンテナを作る（MySelectorItem）

まずは、個々のアイテムを表示するための「器」を作ります。これに `IsSelected` プロパティを持たせます。

```csharp
public class MySelectorItem : ContentControl
{
    // 選択状態を保持する依存関係プロパティ
    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register("IsSelected", typeof(bool), typeof(MySelectorItem), new PropertyMetadata(false));

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    // 静的コンストラクタで、デフォルトのスタイル（Generic.xaml）を適用
    static MySelectorItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(MySelectorItem), new FrameworkPropertyMetadata(typeof(MySelectorItem)));
    }
}

```

---

## 2. Selector 本体を実装する

ここで 3 つのメソッドをオーバーライドします。

```csharp
public class MySelector : ItemsControl
{
    // --- 1. アイテムが既にコンテナかチェック ---
    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is MySelectorItem;
    }

    // --- 2. 新しいコンテナ（器）を作成 ---
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new MySelectorItem();
    }

    // --- 3. コンテナにデータを流し込み、セットアップする ---
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is MySelectorItem container)
        {
            // マウスクリックイベントをフックして、選択状態を切り替える例
            container.MouseDown += (s, e) => 
            {
                UpdateSelection(container);
            };
        }
    }

    private void UpdateSelection(MySelectorItem selectedContainer)
    {
        // 全アイテムをループして、クリックされたものだけ true にする簡易実装
        foreach (var item in Items)
        {
            var container = ItemContainerGenerator.ContainerFromItem(item) as MySelectorItem;
            if (container != null)
            {
                container.IsSelected = (container == selectedContainer);
            }
        }
    }
}

```

---

## 3. Generic.xaml で見た目を定義する

コンテナ（`MySelectorItem`）が `IsSelected` に応じてどう見えるかを定義します。

```xml
<Style TargetType="{x:Type local:MySelectorItem}">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type local:MySelectorItem}">
                <Border x:Name="border" Background="Transparent" BorderBrush="Gray" BorderThickness="1" Padding="5">
                    <ContentPresenter />
                </Border>
                <ControlTemplate.Triggers>
                    <Trigger Property="IsSelected" Value="True">
                        <Setter TargetName="border" Property="Background" Value="LightBlue" />
                        <Setter TargetName="border" Property="BorderBrush" Value="Blue" />
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>

```

---

## この実装のポイント

1. **データの混在に対応:** 文字列などの「生データ」を渡しても、`GetContainerForItemOverride` が自動で `MySelectorItem` で包んでくれます。
2. **スタイルのカプセル化:** `PrepareContainerForItemOverride` を通じて、すべてのアイテムが一貫した「選択ロジック」を持つようになります。
3. **拡張性:** `Prepare...` 内で `ItemContainerStyle` が適用されるため、ユーザーが外から見た目をカスタマイズすることも可能です。

---

## 次のステップへのヒント

今回は `MouseDown` で直接処理しましたが、本格的な `Selector` を作るなら、**`SelectedItem` 依存関係プロパティ** を作成し、それとコンテナの `IsSelected` を同期させるコードを `PrepareContainerForItemOverride` に書くのが王道です。

次は、**「コンテナが破棄されたときの後処理（ClearContainerForItemOverride）」** について具体的に見てみますか？それとも、**バインディングを使った同期方法** を深掘りしますか？


---



# SelectedItem 依存関係プロパティ を作成し、それとコンテナの IsSelected を同期させるコードを PrepareContainerForItemOverride に書いて、さらに「コンテナが破棄されたときの後処理（ClearContainerForItemOverride）」 についても解説して


---


本格的な `Selector` の実装へようこそ。ここがカスタムコントロール作成の山場であり、最も面白い部分です。

`SelectedItem` と `IsSelected` を同期させるには、**「データからUIへの反映」**だけでなく、コンテナが再利用（リサイクル）される際の**「お掃除（Clear）」**が不可欠です。

---

## 1. SelectedItem を持つ Selector の実装

まず、親となる `MySelector` に `SelectedItem` プロパティを追加し、コンテナ（器）との同期ロジックを組み込みます。

```csharp
public class MySelector : ItemsControl
{
    // 1. SelectedItem 依存関係プロパティの定義
    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register("SelectedItem", typeof(object), typeof(MySelector), 
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // SelectedItem が外から変わったとき、各コンテナの IsSelected を更新する処理（省略）
    }

    // --- コンテナの準備 ---
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is MySelectorItem container)
        {
            // 2. コンテナの IsSelected を SelectedItem と同期させる
            container.IsSelected = (item == SelectedItem);

            // 3. クリック時に SelectedItem を更新するイベント購読
            container.PreviewMouseDown += OnContainerMouseDown;
        }
    }

    private void OnContainerMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is MySelectorItem container)
        {
            // コンテナの中身（データ本体）を SelectedItem にセット
            SelectedItem = ItemContainerGenerator.ItemFromContainer(container);
        }
    }

    // --- ここが重要：コンテナの後処理 ---
    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        if (element is MySelectorItem container)
        {
            // 4. イベントハンドラの解除（メモリリーク防止）
            container.PreviewMouseDown -= OnContainerMouseDown;

            // 5. 状態のリセット（再利用されたときのために綺麗にする）
            container.IsSelected = false;
        }

        base.ClearContainerForItemOverride(element, item);
    }
}

```

---

## 2. なぜ ClearContainerForItemOverride が必要なのか？

WPFの `ItemsControl`（特に仮想化が有効な場合）は、画面外に消えたコンテナを破棄せず、次に画面内に入ってくる新しいデータのために **使い回します（再利用）**。

この「お掃除」を忘れると、以下のような問題が起きます。

* **メモリリーク:** `Prepare...` で登録したイベントハンドラが残っていると、古いデータがメモリから解放されず、パフォーマンスが低下します。
* **ゴースト現象:** 前のデータで「選択中（True）」だった状態が残り、新しいデータが表示された瞬間に、意図せずハイライトされた状態に見えてしまうことがあります。

---

## 3. 実装のポイント：SelectedItem の同期

上記のコードでは、`SelectedItem` が変わったときに全アイテムをチェックする代わりに、**「コンテナが表示される瞬間（Prepare）」** に自分が選択されているべきかをチェックしています。

もし、プログラム側から `SelectedItem` を書き換えたときに即座に UI を更新したい場合は、以下のようなメソッドを用意して `OnSelectedItemChanged` から呼び出すのが一般的です。

```csharp
private void UpdateVisualSelection(object selectedItem)
{
    foreach (var item in Items)
    {
        if (ItemContainerGenerator.ContainerFromItem(item) is MySelectorItem container)
        {
            container.IsSelected = (item == selectedItem);
        }
    }
}

```

---

## まとめ：カスタム Selector の完成形

1. **`IsItemItsOwnContainerOverride`**: 二重梱包を防止。
2. **`GetContainerForItemOverride`**: 専用の `MySelectorItem` を生成。
3. **`PrepareContainerForItemOverride`**: `IsSelected` の初期化とイベント購読（紐付け）。
4. **`ClearContainerForItemOverride`**: イベント解除と状態リセット（お掃除）。

これで、データの型（String か ViewModel か）を問わず、一貫した選択挙動を持つ堅牢なコントロールの土台が完成しました！

次は、このコントロールをより実用的にするために、**「キーボード（上下キー）での選択移動」**をサポートする実装に興味はありますか？それとも、**MVVMでのバインディング効率**について深掘りしますか？