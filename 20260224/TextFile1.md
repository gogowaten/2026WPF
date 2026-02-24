# チェックなしに勝手に追加されないようにするためには、Itemsは外に公開しないほうが良いの？


---


結論から言うと、`Items`（コレクション）自体を非公開にする必要はありませんが、**「勝手に追加（Add）できないようにカプセル化する」**のは非常に良い設計です。

MVVMやドメイン設計において、整合性を保つための「よくある手法」を2つ紹介します。

---

## 1. 読み取り専用で公開し、操作メソッドを提供する（推奨）

`ObservableCollection` をそのまま `public get; set;` で公開すると、誰でもどこからでも `Add` できてしまい、先ほどのチェックロジックが走りません。

代わりに、外部には **`ReadOnlyObservableCollection`**（読み取り専用ビュー）を見せ、追加は専用のメソッド（`AddItem` など）経由で行うようにします。

```csharp
public partial class Items(double x, double y) : Item(x, y)
{
    public override ItemType ItemType => ItemType.Items;

    // 内部管理用の実体（private）
    private readonly ObservableCollection<Item> _children = [];

    // 外部公開用の読み取り専用プロパティ（public）
    // シリアライズ対象にするために属性を付与
    [JsonInclude]
    public ReadOnlyObservableCollection<Item> Children { get; }

    public Items(double x, double y) : this(x, y)
    {
        Children = new ReadOnlyObservableCollection<Item>(_children);
    }

    // 安全に追加するための窓口
    public void AddChild(Item newItem)
    {
        // ここでチェックロジックを実行できる！
        if (ReferenceEquals(this, newItem)) return; 
        
        _children.Add(newItem);
    }
}

```

---

## 2. コレクションの変更を監視して「差し戻す」

もし `ObservableCollection` を公開したままにしたい場合は、コレクションの `CollectionChanged` イベントを購読し、不正なアイテムが追加された瞬間に `Remove` するという荒業もありますが、コードが複雑になるため **「1」の方法が一般的** です。

---

## 🛠️ シリアライズとの兼ね合い

ここで一つ注意点があります。`System.Text.Json` はデフォルトで **「引数なしのコンストラクタ」** または **「プロパティ名と一致する引数を持つコンストラクタ」** を探します。

上記のように `Children` を読み取り専用にすると、デシリアライズ時に中身が復元されなくなる場合があります。その対策として、以下のように **`JsonConstructor`** を使うか、シリアライザーが `private` な実体にアクセスできるように設定します。

```csharp
public partial class Items : Item
{
    [JsonConstructor]
    public Items(double x, double y, IEnumerable<Item> children) : base(x, y)
    {
        foreach (var child in children) _children.Add(child);
        Children = new ReadOnlyObservableCollection<Item>(_children);
    }
}

```

---

## ⚖️ 結論：どっちがいい？

* **小規模・個人開発なら:** そのまま公開して、「追加するときは必ずこのメソッドを使ってね」という自分ルール（運用）で十分です。
* **大規模・ライブラリ化するなら:** メソッド経由に制限し、勝手な `Add` をコンパイルエラーで防ぐべきです。

今のフェーズでは、まず **`AddChild` メソッドを作ってそこでチェックを行う** 仕組みにするのが、デバッグもしやすくておすすめです。

次は、この `Items` クラスを使って、実際に **Canvas上にデータを並べるView（XAML）の実装** に挑戦してみますか？それとも、ドラッグ移動のロジックを深掘りしますか？


---


# 次はBrushをシリアライズしたい、どのような方法がある？
        [ObservableProperty] private SolidColorBrush _fill = Brushes.Maroon;


---


WPFの **`SolidColorBrush`** は、色情報の他にも `Opacity` や `Transform` など多くの状態を持っており、そのままでは `System.Text.Json` でシリアライズできません。

MVVMパターンでこれを扱うには、**「モデルでは文字列（カラーコード）で保持し、プロパティでBrushに変換する」**という手法が最も一般的で、かつ堅牢です。

いくつかの方法を紹介します。

---

## 1. 推奨：Color文字列プロパティとBrushプロパティを分ける

モデル内ではシリアライズ可能な `string` で色を保持し、ViewModel（またはModel）側で `SolidColorBrush` を生成します。

```csharp
public partial class RectangleItem : Item
{
    // 1. シリアライズ対象（文字列で保存）
    [ObservableProperty]
    [property: JsonPropertyName("Fill")] // JSON上では "Fill" という名前にする
    private string _fillColor = "#FF800000"; // 初期値 (Maroon)

    // 2. シリアライズ対象外（Viewの表示用）
    [JsonIgnore]
    public SolidColorBrush FillBrush => new SolidColorBrush((Color)ColorConverter.ConvertFromString(FillColor));

    // FillColorが変更されたら、FillBrushの変更も通知する必要がある
    partial void OnFillColorChanged(string value) => OnPropertyChanged(nameof(FillBrush));
}

```

---

## 2. カスタム JsonConverter を作成する（上級者向け）

`SolidColorBrush` 自体をそのままプロパティとして持ちたい場合は、独自の変換ルール（JsonConverter）を自作します。これを作っておけば、属性を付けるだけで自動的に `"#FF800000"` ↔ `SolidColorBrush` の変換が行われます。

### Converter クラスの実装

```csharp
public class SolidColorBrushConverter : JsonConverter<SolidColorBrush>
{
    public override SolidColorBrush Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var colorString = reader.GetString();
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorString));
    }

    public override void Write(Utf8JsonWriter writer, SolidColorBrush value, JsonSerializerOptions options)
    {
        // Brushを "#AARRGGBB" 形式の文字列として書き出す
        writer.WriteStringValue(value.ToString());
    }
}

```

### モデルでの使い方

```csharp
public partial class RectangleItem : Item
{
    [ObservableProperty]
    [JsonConverter(typeof(SolidColorBrushConverter))] // 自作コンバーターを指定
    private SolidColorBrush _fill = Brushes.Maroon;
}

```

---

## 3. どちらの方法が良い？

| 方法 | メリット | デメリット |
| --- | --- | --- |
| **1. 文字列で保持** | シンプル。JSONの中身が直感的。特別な設定が不要。 | `FillBrush` を呼び出すたびにインスタンスが生成される（工夫で回避可）。 |
| **2. Converter自作** | クラス内を `SolidColorBrush` 型で統一できる。 | コンバーターのコードを書く手間がかかる。 |

**おすすめは「2」の JsonConverter 方式です。** 一度作ってしまえば、他のクラスで `Brush` を使いたくなった時も属性1行で済むため、非常に使い勝手が良くなります。

---

## 🛠️ 次のステップ：Viewでのバインディング

シリアライズが通るようになったら、次は XAML 側でこれらを表示しましょう。

```xml
<Rectangle Width="{Binding Width}" 
           Height="{Binding Height}" 
           Fill="{Binding Fill}" />

```

もし「既存の `Brushes.Maroon` のような定義済みポインタをそのまま使いたい」のか、「自由な色をカラーピッカーなどで選びたい」のかによっても最適な構成が変わります。

次は **View (XAML) での描画部分の実装** に進みますか？それとも **画像をパス文字列でシリアライズする `ImageItem**` を復活させますか？


---


`Fill` がシリアライズされない原因は、プロパティの **アクセシビリティ（公開範囲）** にあります。

### ❌ 原因：プロパティが `private` になっている

提示されたコードでは、`Fill` プロパティが以下のように定義されています。

```csharp
[JsonConverter(typeof(SolidColorBrushConverter))]
private SolidColorBrush Fill { get; set; } = Brushes.Maroon;

```

`System.Text.Json` はデフォルトで **`public` プロパティのみ** をシリアライズの対象にします。`private` なプロパティは、たとえ属性が付いていても無視されてしまいます。

---

### ✅ 解決策：`public` に変更する

プロパティを `public` に変更するだけで、シリアライザーがこれを見つけられるようになり、自作の `SolidColorBrushConverter` が実行されるようになります。

```csharp
public partial class RectangleItem : Item
{
    // ... 他のプロパティ ...

    // public に変更！
    [JsonConverter(typeof(SolidColorBrushConverter))]
    public SolidColorBrush Fill { get; set; } = Brushes.Maroon;

    public RectangleItem(double x, double y, double width, double height) : base(x, y)
    {
        Width = width;
        Height = height;
        // Brushes.Maroonなどは既にFreezeされていますが、自作Brush用に呼ぶのはOK
        if (Fill.CanFreeze) Fill.Freeze(); 
    }
}

```

---

### 💡 MVVM的に `ObservableProperty` を使いたい場合

もし `Fill` の変更も画面に通知したい（色を動的に変えたい）場合は、CommunityToolkit.Mvvm の `[ObservableProperty]` を使いつつ、シリアライズ対象に指定します。

```csharp
public partial class RectangleItem : Item
{
    [ObservableProperty]
    [property: JsonConverter(typeof(SolidColorBrushConverter))] // 生成されるpublicプロパティに属性を付与
    private SolidColorBrush _fill = Brushes.Maroon;

    // ... コンストラクタなど ...
}

```

> **注意:** `[property: ...]` という書き方は、「生成されるプロパティに対してこの属性を付けてね」という意味になります。

---

### 🛠️ 動作確認

プロパティを `public` にして実行すると、出力結果は以下のようになるはずです。

```json
{
  "Type": "RectangleItem",
  "Width": 40,
  "Height": 50,
  "Fill": "#FF800000",
  "CanDrag": true,
  "X": 10,
  "Y": 20
}

```

### 💡 ワンポイント：`ColorConverter` の挙動

`SolidColorBrushConverter` 内で `value.ToString()` を使うと、結果は `#FF800000` のような文字列になります。これはデシリアライズ時の `ColorConverter.ConvertFromString` で正しく解釈できる形式なので、これで完璧です。

これで `Fill` も無事に保存・読み込みができるようになりましたか？
次は、いよいよ **`ImageItem`（BitmapSourceを文字列パスで扱う方法）** に進みましょうか？


