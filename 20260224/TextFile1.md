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


---


# できた！次は、Brushをそのままじゃなくて、ARGBの４つのbyte型プロパティで持って、それをRectangleのFillプロパティに適用するにはどんな方法があるか知りたい



---


「できた！」との報告、嬉しいです。一歩ずつ着実に進んでいますね！

Brushをそのまま持つのではなく、**A, R, G, B の 4つの `byte` 型プロパティ**で保持するのは、データの透明性が高く、WPF以外のプラットフォーム（例えばモバイルアプリやWeb）へのデータ転送もしやすくなる非常にスマートな方法です。

これをWPFの `Rectangle.Fill` に適用するには、主に **2つのアプローチ** があります。

---

## 1. モデル内で「合成Brush」プロパティを作る（おすすめ）

データとしては `byte` で持ちつつ、Viewがバインディングするための `SolidColorBrush` プロパティを読み取り専用で提供する方法です。

```csharp
public partial class RectangleItem : Item
{
    // 1. データとしての4つの byte (シリアライズ対象)
    [ObservableProperty] private byte _a = 255;
    [ObservableProperty] private byte _r = 128;
    [ObservableProperty] private byte _g = 0;
    [ObservableProperty] private byte _b = 0;

    // 2. View 表示用の計算プロパティ (シリアライズ除外)
    [JsonIgnore]
    public SolidColorBrush FillBrush => new SolidColorBrush(Color.FromArgb(A, R, G, B));

    // 3. いずれかの値が変わったら、FillBrush の変更も View に通知する
    partial void OnAChanged(byte value) => OnPropertyChanged(nameof(FillBrush));
    partial void OnRChanged(byte value) => OnPropertyChanged(nameof(FillBrush));
    partial void OnGChanged(byte value) => OnPropertyChanged(nameof(FillBrush));
    partial void OnBChanged(byte value) => OnPropertyChanged(nameof(FillBrush));
}

```

**XAMLでの適用:**

```xml
<Rectangle Fill="{Binding FillBrush}" />

```

---

## 2. MultiBinding と Converter を使う

モデル側に Brush 生成ロジックを持たせたくない（純粋なデータだけにしたい）場合は、XAML 側で 4 つの値を 1 つの Brush に合成します。

### Converter の作成

```csharp
public class ArgbToBrushConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 4 && values.All(v => v is byte))
        {
            byte a = (byte)values[0];
            byte r = (byte)values[1];
            byte g = (byte)values[2];
            byte b = (byte)values[3];
            return new SolidColorBrush(Color.FromArgb(a, r, g, b));
        }
        return Brushes.Transparent;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

```

### XAML での適用

```xml
<Rectangle>
    <Rectangle.Fill>
        <MultiBinding Converter="{StaticResource ArgbToBrushConverter}">
            <Binding Path="A" />
            <Binding Path="R" />
            <Binding Path="G" />
            <Binding Path="B" />
        </MultiBinding>
    </Rectangle.Fill>
</Rectangle>

```

---

## ⚖️ どちらの方法が良い？

| 手法 | メリット | デメリット |
| --- | --- | --- |
| **1. モデルで合成** | XAMLがシンプルになる。実行速度がわずかに速い（コンバーターのオーバーヘッドがない）。 | モデルがWPFの `SolidColorBrush` に依存する。 |
| **2. MultiBinding** | モデルが完全に純粋なデータ（byte）だけで完結する。 | XAMLの記述が冗長になり、デバッグが少し面倒。 |

**実務的には「1」の方法がよく使われます。** MVVMにおいて、Viewが必要とする形にデータを加工して提供するのは ViewModel/Model の正当な役割だからです。

---

## 🛠️ JSON シリアライズの結果

この構成にすると、JSON は非常に「データらしい」見た目になります。

```json
{
  "Type": "RectangleItem",
  "A": 255,
  "R": 128,
  "G": 0,
  "B": 0,
  "Width": 40,
  "Height": 50,
  "X": 10,
  "Y": 20
}

```

この「4つのbyte」方式、気に入りましたか？
もし OK であれば、次はこれら（Items, TextBlock, Rectangle）を **Canvas上に表示してマウスで動かせるようにする「Viewの実装」** にいよいよ踏み込んでみましょうか？


---


# モデル内で「合成Brush」プロパティを作る方法の中の以下の部分は必要？
”// 3. いずれかの値が変わったら、FillBrush の変更も View に通知する
    partial void OnAChanged(byte value) => OnPropertyChanged(nameof(FillBrush));
    partial void OnRChanged(byte value) => OnPropertyChanged(nameof(FillBrush));
    partial void OnGChanged(byte value) => OnPropertyChanged(nameof(FillBrush));
    partial void OnBChanged(byte value) => OnPropertyChanged(nameof(FillBrush));”

試しに上記部分をコメントアウトした状態で下記を実行してみたけど、FillBrushは更新されていた
"
            RectangleItem rectangleItem = new(10, 20, 40, 50);
            rectangleItem.A = 0;
            rectangleItem.R = 0;
            rectangleItem.G = 0;
            rectangleItem.B = 0;
"


---


結論から言うと、**「C#のコード上で値を確認するだけ」なら不要ですが、「WPFの画面（UI）と連動させる」なら絶対に必要**です。

ここがMVVMにおける「データの更新」と「通知」の面白い（そして重要な）違いです。

---

### 1. なぜコメントアウトしても更新されたように見えたのか？

あなたが実行したコードは、**「プロパティを読み取った瞬間に新しいBrushを作る」**という動きをしているからです。

```csharp
public SolidColorBrush FillBrush => new SolidColorBrush(Color.FromArgb(A, R, G, B));

```

この定義は「getter（ゲッター）」です。`rectangleItem.FillBrush` を呼び出したその瞬間に、その時の `A, R, G, B` の値を参照して新しいインスタンスを生成します。そのため、C#のコードで値を書き換えた後に `FillBrush` を取得すれば、当然新しい色のBrushが手に入ります。

---

### 2. では、なぜ `OnPropertyChanged` が必要なのか？

WPFの画面（XAML）に `Rectangle` を表示しているシーンを想像してください。

```xml
<Rectangle Fill="{Binding FillBrush}" />

```

WPFのバインディングシステムは、一度 `FillBrush` を読み取って画面を描画すると、**「次からはモデル側から『変わったよ！』と言われない限り、自分からは読み直しに行かない」**という性質を持っています。

* **通知がない場合（コメントアウト状態）**:
あなたがコードで `rectangleItem.R = 255;` と書き換えても、WPFは `R` が変わったことは知っていますが、それが **`FillBrush` という別のプロパティに影響することを知りません。** そのため、画面上のRectangleの色は古いまま（初期値のまま）止まってしまいます。
* **通知がある場合**:
`OnRChanged` の中で `OnPropertyChanged(nameof(FillBrush))` を呼ぶと、WPFに対して「おい、`FillBrush` も中身が変わったからもう一度読み直しに来い！」と合図を送ります。これによって初めて、画面の色がリアルタイムでパッと変わります。

---

### 3. CommunityToolkit.Mvvm を使っているならもっと楽に書ける！

ソースジェネレーターの機能を使えば、わざわざ `partial void On...` をたくさん書かなくても、属性一つで解決できます。

```csharp
public partial class RectangleItem : Item
{
    // Aが書き換わったら、自分自身の通知だけでなく FillBrush の通知も飛ばす設定
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FillBrush))] 
    private byte _a = 255;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FillBrush))]
    private byte _r = 128;

    // ... G, B も同様 ...

    [JsonIgnore]
    public SolidColorBrush FillBrush => new SolidColorBrush(Color.FromArgb(A, R, G, B));
}

```

`[NotifyPropertyChangedFor]` を使うのが、今の時代の最も「スマート」な書き方です。

---

### まとめ：いつ必要になる？

* **不要なケース**: JSONを書き出すだけ、またはコンソールに出力するだけの場合。
* **必要なケース**: **スライダーを動かして色をリアルタイムに変えたい**、あるいはプログラムから色を変えた瞬間に画面にも反映させたい場合。

いよいよ次は、この `RectangleItem` などを実際に Canvas に並べて、**「色が変わる様子」を画面で確認する実装** に進みますか？


---


# この Items クラスを使って、実際に Canvas上にデータを並べるView（XAML）の実装を解説して


---


いよいよ一番楽しい「画面への表示」ですね！

WPFで `ObservableCollection` の中身を `Canvas` 上に自由に配置するには、**`ItemsControl`** をカスタマイズするのが王道です。

以下の3ステップで実装を解説します。

---

## 1. DataTemplate の準備

まず、各クラス（TextBlockItem, RectangleItem など）を「どう描画するか」というレシピを定義します。これを `Resources` に置いておくと、WPFが型を自動判別して適切な見た目を割り当ててくれます。

```xml
<Window.Resources>
    <DataTemplate DataType="{x:Type local:TextBlockItem}">
        <TextBlock Text="{Binding Text}" Foreground="Black"/>
    </DataTemplate>

    <DataTemplate DataType="{x:Type local:RectangleItem}">
        <Rectangle Width="{Binding Width}" 
                   Height="{Binding Height}" 
                   Fill="{Binding FillBrush}" />
    </DataTemplate>

    <DataTemplate DataType="{x:Type local:Items}">
        <ItemsControl ItemsSource="{Binding Children}">
            <ItemsControl.ItemsPanel>
                <ItemsPanelTemplate>
                    <Canvas />
                </ItemsPanelTemplate>
            </ItemsControl.ItemsPanel>
        </ItemsControl>
    </DataTemplate>
</Window.Resources>

```

---

## 2. ItemsControl の配置（メインの Canvas）

`ItemsControl` を使って、リストの中身を `Canvas` 上に展開します。ポイントは **「座標（X, Y）をどうやって Canvas に伝えるか」** です。

```xml
<ItemsControl ItemsSource="{Binding ItemsList}">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <Canvas Background="White" IsItemsHost="True" />
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>

    <ItemsControl.ItemContainerStyle>
        <Style TargetType="ContentPresenter">
            <Setter Property="Canvas.Left" Value="{Binding X, Mode=TwoWay}" />
            <Setter Property="Canvas.Top" Value="{Binding Y, Mode=TwoWay}" />
        </Style>
    </ItemsControl.ItemContainerStyle>
</ItemsControl>

```

---

## 3. 解説：なぜこれで動くのか？

* **`DataType` による自動切り替え**: `DataTemplate` に `DataType` を指定すると、`ItemsControl` は「今から表示するデータは `RectangleItem` か？ならこの Rectangle を描こう」と自動判断します。
* **`ItemContainerStyle`**: これが魔法のスパイスです。`ItemsControl` 内の各アイテムは `ContentPresenter` という枠に入れられます。その枠に対して、「お前の Canvas 上の位置（Left/Top）は、モデルの X と Y を見ろ！」と命令しています。
* **再帰的な Items**: `Items` クラスの `DataTemplate` の中にもう一度 `ItemsControl` を書くことで、**「グループの中にさらにアイテムがある」という入れ子構造（ツリー構造）**がそのまま画面に反映されます。

---

## 🛠️ 注意：座標の更新

さきほど `byte` プロパティで話した通り、`X` と `Y` も `[ObservableProperty]` になっている必要があります。

もし、**「画面上でマウスでドラッグして動かしたい」** という場合は、この `Canvas` にマウス操作のロジック（BehaviorやEvent）を追加する必要があります。

次は、この XAML を使って実際に表示してみますか？それとも **「マウスでドラッグして移動させる処理」** のコードを追加しましょうか？


---


# 次は、この XAML を使って実際に表示して



---


さっそく、これまで作成したクラス群と XAML を組み合わせて、実際に画面に表示する最小構成のサンプルを作成しましょう。

ポイントは、**MainWindow の `DataContext` にデータをセットする** ことです。

### 1. ViewModel (または MainWindow.xaml.cs) の準備

まずは、画面に表示するためのデータを準備します。

```csharp
public partial class MainWindow : Window
{
    // 画面にバインドするリスト
    public ObservableCollection<Item> ItemsList { get; set; } = new();

    public MainWindow()
    {
        InitializeComponent();
        
        // テストデータの作成
        PrepareData();

        // このウィンドウのデータソースとして自分自身を指定
        this.DataContext = this;
    }

    private void PrepareData()
    {
        // 1. 単体アイテム
        ItemsList.Add(new TextBlockItem(10, 10, "こんにちはWPF！"));
        ItemsList.Add(new RectangleItem(150, 50, 100, 50) { R = 255, G = 0, B = 0 }); // 赤色

        // 2. 入れ子（グループ）アイテム
        var group = new Items(50, 150);
        group.Children.Add(new RectangleItem(0, 0, 80, 80) { R = 0, G = 0, B = 255 }); // 青色
        group.Children.Add(new TextBlockItem(10, 90, "これはグループ内です"));
        
        ItemsList.Add(group);
    }
}

```

---

### 2. MainWindow.xaml の実装

次に、先ほど解説した `ItemsControl` と `DataTemplate` を配置します。

```xml
<Window x:Class="_20260224_DataTemplateSelector.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:_20260224_DataTemplateSelector"
        Title="JSON Canvas Board" Height="450" Width="800">
    
    <Window.Resources>
        <DataTemplate DataType="{x:Type local:TextBlockItem}">
            <TextBlock Text="{Binding Text}" FontSize="16" Foreground="Black"/>
        </DataTemplate>

        <DataTemplate DataType="{x:Type local:RectangleItem}">
            <Rectangle Width="{Binding Width}" 
                       Height="{Binding Height}" 
                       Fill="{Binding FillBrush}" 
                       Stroke="Black" StrokeThickness="1"/>
        </DataTemplate>

        <DataTemplate DataType="{x:Type local:Items}">
            <ItemsControl ItemsSource="{Binding Children}">
                <ItemsControl.ItemsPanel>
                    <ItemsPanelTemplate>
                        <Canvas />
                    </ItemsPanelTemplate>
                </ItemsControl.ItemsPanel>
                <ItemsControl.ItemContainerStyle>
                    <Style TargetType="ContentPresenter">
                        <Setter Property="Canvas.Left" Value="{Binding X}"/>
                        <Setter Property="Canvas.Top" Value="{Binding Y}"/>
                    </Style>
                </ItemsControl.ItemContainerStyle>
            </ItemsControl>
        </DataTemplate>
    </Window.Resources>

    <Grid Background="#F0F0F0">
        <ItemsControl ItemsSource="{Binding ItemsList}">
            <ItemsControl.ItemsPanel>
                <ItemsPanelTemplate>
                    <Canvas Background="White" Margin="10">
                        <Canvas.Effect>
                            <DropShadowEffect BlurRadius="10" ShadowDepth="2" Opacity="0.2"/>
                        </Canvas.Effect>
                    </Canvas>
                </ItemsPanelTemplate>
            </ItemsControl.ItemsPanel>
            
            <ItemsControl.ItemContainerStyle>
                <Style TargetType="ContentPresenter">
                    <Setter Property="Canvas.Left" Value="{Binding X, Mode=TwoWay}"/>
                    <Setter Property="Canvas.Top" Value="{Binding Y, Mode=TwoWay}"/>
                </Style>
            </ItemsControl.ItemContainerStyle>
        </ItemsControl>
    </Grid>
</Window>

```

---

### 3. 表示の確認ポイント

この状態で実行すると、以下のようになるはずです。

1. **自動判別**: `ItemsList` に入れたオブジェクトが `TextBlockItem` なら文字が、`RectangleItem` なら四角形が表示されます。
2. **座標**: `X` と `Y` で指定した位置にピタッと配置されます。
3. **入れ子**: `Items` クラスの中にある `Children` も、そのグループの `(X, Y)` を起点として正しく表示されます。

### 次のステップへのヒント

これで「表示」までは完璧です！しかし、今のままでは画面上の四角形をマウスで掴んで動かすことはできません。

次は **「マウス操作（ドラッグ）で X, Y 座標を更新して、アイテムを自由に移動させるロジック」** を追加しますか？それとも、**「ボタンを押して今の状態を JSON 保存する機能」** を先に作りますか？


---


# 今の状態をCustomControlにするメリットにはどんなものがある？それとも今のままMainWindowに書いたほうが良い？


---


結論から言うと、**「今のまま MainWindow に書くのは卒業して、少なくとも UserControl、理想的には CustomControl（または独自のコンポーネント）に切り出す」**フェーズに来ています。

特に今回の「Canvas 上にアイテムを並べる」という仕組みは、今後「ドラッグ操作」「ズーム」「グリッド表示」など機能がどんどん増えるため、MainWindow に置いたままだとコードが肥大化して身動きが取れなくなるからです。

---

### 1. `CustomControl` (または `UserControl`) にするメリット

#### ① 圧倒的な「再利用性」

例えば、メインの編集画面だけでなく「保存データのプレビュー画面」や「別のウィンドウ」でも同じように図形を表示したい場合、コントロール化していればタグ一行（`<local:DiagramCanvas ItemsSource="{Binding ...}" />`）で使い回せます。

#### ② 責務の分離（メンテナンス性）

MainWindow は「アプリ全体の管理（メニュー、ファイル保存、ステータスバーなど）」に専念し、キャンバス操作の細かいロジック（マウスの座標計算など）をコントロール側に閉じ込めることができます。

#### ③ 独自プロパティ（DependencyProperty）の追加

CustomControl にすると、以下のような独自のプロパティを外から流し込めるようになります。

* `ZoomLevel`（拡大率）
* `IsReadOnly`（編集禁止モード）
* `GridSpacing`（グリッドの吸着サイズ）

---

### 2. `UserControl` vs `CustomControl` どっちがいい？

今回のケースでは、どちらを選ぶべきか明確な指針があります。

| 種類 | 特徴 | 今回のケースへの適性 |
| --- | --- | --- |
| **UserControl** | XAML と C# (コードビハインド) をセットで作る。簡単。 | **[おすすめ]** まずはここから。ロジックを切り出すには十分です。 |
| **CustomControl** | `Generic.xaml` でスタイルを定義する。汎用性が極めて高い。 | **[本格派]** ライブラリとして公開したり、見た目を完全に入れ替え可能にしたい場合に。 |

---

### 3. 現実的な「次の一歩」：まずは `UserControl` への切り出し

いきなり複雑な CustomControl にせず、まずは今の `ItemsControl` の記述を **`DiagramBoard.xaml`** という UserControl に引っ越すのがベストです。

#### 移行のイメージ：

1. **`DiagramBoard.xaml`** を作成し、MainWindow にあった `ItemsControl` と `DataTemplate` をここに移動。
2. **`MainWindow.xaml`** はスッキリ。
```xml
<Grid>
    <local:DiagramBoard ItemsSource="{Binding ItemsList}" />
</Grid>

```



---

### 💡 結論：どうすべき？

**「今のまま」はおすすめしません。**
今の構成で「マウスでドラッグ移動」のコードを書き始めると、MainWindow.xaml.cs がマウスイベントのコード（MouseDown, MouseMove...）で埋め尽くされてしまいます。

**「ドラッグ移動の実装」に入る前に、UserControl に切り出すことを強く推奨します。**

次は **「UserControl への切り出し方」** を具体的に解説しますか？それとも、UserControl 内で **「マウスドラッグの実装」** を進めますか？


---


# CustomControlに移行したいので、その方法を解説して


---


本格的なライブラリ開発や、高度な機能拡張を見据えた **CustomControl (カスタムコントロール)** への移行は素晴らしい選択です。

CustomControl は、**「ロジック (C#)」** と **「見た目 (XAML)」** を完全に分離するのが特徴です。UserControl と違い、見た目は `Themes/Generic.xaml` という場所に記述します。

以下の手順で、`DiagramCanvas` という名前のカスタムコントロールを作ってみましょう。

---

## 1. CustomControl の作成

Visual Studio のプロジェクトで **[追加] > [新しい項目] > [カスタム コントロール (WPF)]** を選択し、名前を `DiagramCanvas.cs` にします。

これにより、自動的に以下の2つのファイルが準備されます：

1. `DiagramCanvas.cs` (ロジック)
2. `Themes/Generic.xaml` (デフォルトの見た目。既にある場合は追記されます)

---

## 2. ロジックの実装 (`DiagramCanvas.cs`)

`ItemsControl` を継承して、データの入り口を作ります。

```csharp
public class DiagramCanvas : ItemsControl
{
    static DiagramCanvas()
    {
        // このコントロールのスタイルが Generic.xaml にあることを WPF に伝えます
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DiagramCanvas), 
            new FrameworkPropertyMetadata(typeof(DiagramCanvas)));
    }

    // 将来的に「背景グリッドの表示/非表示」などの 
    // DependencyProperty (依存関係プロパティ) をここに追加していきます
}

```

---

## 3. スタイルの実装 (`Themes/Generic.xaml`)

ここが肝心です。MainWindow に書いていた `ItemsPanel` や `ItemContainerStyle` をここに引っ越します。

```xml
<ResourceDictionary
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:local="clr-namespace:_20260224_DataTemplateSelector">

    <Style TargetType="{x:Type local:DiagramCanvas}">
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="{x:Type local:DiagramCanvas}">
                    <Border Background="{TemplateBinding Background}"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}">
                        <ItemsPresenter />
                    </Border>
                </ControlTemplate>
            </Setter.Value>
        </Setter>

        <Setter Property="ItemsPanel">
            <Setter.Value>
                <ItemsPanelTemplate>
                    <Canvas Background="Transparent" IsItemsHost="True" />
                </ItemsPanelTemplate>
            </Setter.Value>
        </Setter>

        <Setter Property="ItemContainerStyle">
            <Setter.Value>
                <Style TargetType="ContentPresenter">
                    <Setter Property="Canvas.Left" Value="{Binding X, Mode=TwoWay}" />
                    <Setter Property="Canvas.Top" Value="{Binding Y, Mode=TwoWay}" />
                </Style>
            </Setter.Value>
        </Setter>
    </Style>
</ResourceDictionary>

```

---

## 4. MainWindow での使用

コントロール化できたので、MainWindow は劇的にスッキリします。

```xml
<Window ... xmlns:local="clr-namespace:_20260224_DataTemplateSelector">
    <Window.Resources>
        </Window.Resources>

    <Grid>
        <local:DiagramCanvas ItemsSource="{Binding ItemsList}" Background="White" />
    </Grid>
</Window>

```

---

## CustomControl にする最大のメリット

CustomControl に移行したことで、**「ドラッグ移動のロジック」を実装する場所**が明確になりました。

これからは `DiagramCanvas.cs` の中で `OnPreviewMouseDown` や `OnPreviewMouseMove` などのメソッドを `override` して記述していきます。MainWindow のコードビハインドを汚さずに、「キャンバスとしての機能」をどんどん作り込めるようになります。

### 次のステップ：ドラッグ移動の実装

CustomControl への引っ越しは完了しましたか？
次は `DiagramCanvas.cs` に **「クリックしたアイテムをマウスで動かす（X, Y を書き換える）コード」** を追加しましょうか？


---


# 下記のようにMainWindowに書いていたDateTemplateはどこに書けばいいの？
”
  <Window.Resources>
    <DataTemplate DataType="{x:Type local:TextBlockItem}">
”


---


CustomControl化した後は、`DataTemplate` の置き場所として**「3つの選択肢」**があります。プロジェクトの規模や、「その図形をどこまで使い回したいか」によって選びます。

---

## 1. 最も手軽：App.xaml に置く（おすすめ）

アプリ全体でその図形（TextBlockItemやRectangleItem）の見た目を統一したい場合に最も一般的な方法です。

`App.xaml` の `Application.Resources` 内に記述すれば、`MainWindow` でも `DiagramCanvas` の中でも、どこでも自動的に適用されます。

```xml
<Application ...>
    <Application.Resources>
        <DataTemplate DataType="{x:Type local:TextBlockItem}">
            <TextBlock Text="{Binding Text}" FontSize="16"/>
        </DataTemplate>
        
        <DataTemplate DataType="{x:Type local:RectangleItem}">
            <Rectangle Width="{Binding Width}" Height="{Binding Height}" Fill="{Binding FillBrush}"/>
        </DataTemplate>
    </Application.Resources>
</Application>

```

---

## 2. 整理整頓：別のファイル（ResourceDictionary）に分ける

DataTemplateの数が増えてくると `App.xaml` が巨大化して見づらくなります。その場合は「図形定義専用」のファイルを作ります。

1. **`ShapeTemplates.xaml`** という名前の「リソースディクショナリ」ファイルを新規作成。
2. その中に `DataTemplate` を書く。
3. `App.xaml` でそれを合流（マージ）させる。

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="ShapeTemplates.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>

```

---

## 3. 究極の独立：Generic.xaml の中で定義する

「この `DiagramCanvas` を使うときは、必ずこの見た目で表示させたい（他の場所の影響を受けたくない）」という、**コントロールとしての独立性**を重視する場合、`Generic.xaml` の中に書くこともできます。

ただし、`Generic.xaml` に書く場合は、`Style` の中にある `Resources` に配置するのがルールです。

```xml
<Style TargetType="{x:Type local:DiagramCanvas}">
    <Style.Resources>
        <DataTemplate DataType="{x:Type local:TextBlockItem}"> ... </DataTemplate>
    </Style.Resources>
    
    <Setter Property="Template"> ... </Setter>
</Style>

```

---

### 💡 どっちが良い？

* **まずは「1（App.xaml）」**で進めるのが一番スムーズです。
* もし今後、この `DiagramCanvas` を**他のプロジェクトでも使い回せる「ライブラリ（DLL）」**として独立させたいなら、**「3」**の方法をとることで、使う側がわざわざ DataTemplate を用意しなくて済むようになります。

まずは `App.xaml` に引っ越して、MainWindow がスッキリしたか確認してみてください！

次は、`DiagramCanvas.cs` のコードビハインドで **「マウスドラッグの実装」** を始めますか？


---


# 今後DataTemplateを増やすことを考えて、「2. 整理整頓：別のファイル（ResourceDictionary）に分ける」にしたい、その方法を解説して



---


今後アイテムの種類（円形、画像、線など）が増えていくことを考えると、専用のファイルに分けるのは非常に賢明な判断です。

以下の手順で、`DataTemplate` 専用のリソース辞書を作成し、アプリ全体で使えるように設定しましょう。

---

### 1. リソース辞書ファイルの作成

プロジェクトの右クリックメニューから **[追加] > [新しい項目] > [リソース ディクショナリ (WPF)]** を選択し、名前を `ItemTemplates.xaml` として作成します。

### 2. `ItemTemplates.xaml` の編集

作成したファイルに、今まで `MainWindow` に書いていた `DataTemplate` を移動します。

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:local="clr-namespace:_20260224_DataTemplateSelector">

    <DataTemplate DataType="{x:Type local:TextBlockItem}">
        <TextBlock Text="{Binding Text}" FontSize="16" Foreground="Black"/>
    </DataTemplate>

    <DataTemplate DataType="{x:Type local:RectangleItem}">
        <Rectangle Width="{Binding Width}" 
                   Height="{Binding Height}" 
                   Fill="{Binding FillBrush}" 
                   Stroke="Black" StrokeThickness="1"/>
    </DataTemplate>

    <DataTemplate DataType="{x:Type local:Items}">
        <local:DiagramCanvas ItemsSource="{Binding Children}"/>
    </DataTemplate>

</ResourceDictionary>

```

※ `Items` のテンプレート内で、作成した `local:DiagramCanvas` を使うように書き換えると、入れ子構造が非常にスッキリします。

---

### 3. `App.xaml` でリソースを合流（マージ）させる

作成した `ItemTemplates.xaml` をアプリ全体で認識させるために、`App.xaml` で読み込み設定を行います。

```xml
<Application x:Class="_20260224_DataTemplateSelector.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="ItemTemplates.xaml"/>
            </ResourceDictionary.MergedDictionaries>
            
            </ResourceDictionary>
    </Application.Resources>
</Application>

```

---

### 4. MainWindow の後片付け

これで `App.xaml` 経由で全てのテンプレートが読み込まれるようになったので、`MainWindow.xaml` の `<Window.Resources>` 内にあった `DataTemplate` は**すべて削除**して大丈夫です。

---

### この構成のメリット

* **追加が楽**: 新しいアイテム（例：`CircleItem`）を作ったら、`ItemTemplates.xaml` に `DataTemplate` を追記するだけで、すぐに画面に反映されます。
* **見通しが良い**: `Generic.xaml`（コントロールの基本構造）と `ItemTemplates.xaml`（各アイテムの具体的な見た目）が分かれているため、どこを直せばいいか迷いません。
* **デザイナでの作業**: `ItemTemplates.xaml` を開くだけで、図形の見た目だけに集中してデザイン作業ができます。

これで表示周りの基盤が完璧に整いましたね！

準備ができたら、次はいよいよ `DiagramCanvas.cs` に **「マウスでアイテムを掴んで動かす（ドラッグ移動）」** のロジックを実装していきましょうか？


---


