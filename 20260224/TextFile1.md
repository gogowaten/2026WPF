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


# Items.Childrenがすべて収まる範囲に色を付けて、Itemsを視覚化したくて以下のようにしたけど、色がつかない
もっと良い別の方法がある？

Itemクラスに背景色用のプロパティを追加した
伴ってRectangleItemのFillBrushは廃止した
伴ってItemTemplatesクラスも書き換えた

”
Itemクラス
    [JsonDerivedType(typeof(TextBlockItem), nameof(TextBlockItem))]
    [JsonDerivedType(typeof(RectangleItem), nameof(RectangleItem))]
    [JsonDerivedType(typeof(Items), nameof(Items))]
    public abstract partial class Item(double x, double y) : ObservableObject
    {
        [ObservableProperty] private double _x = x;
        [ObservableProperty] private double _y = y;
        [ObservableProperty][NotifyPropertyChangedFor(nameof(Background))] private byte _backgroundA = 0;
        [ObservableProperty][NotifyPropertyChangedFor(nameof(Background))] private byte _backgroundR = 0;
        [ObservableProperty][NotifyPropertyChangedFor(nameof(Background))] private byte _backgroundG = 0;
        [ObservableProperty][NotifyPropertyChangedFor(nameof(Background))] private byte _backgroundB = 0;

        // View用のBrushをARGBから生成
        [JsonIgnore]
        public SolidColorBrush Background
        {
            get
            {
                return new(Color.FromArgb(BackgroundA, BackgroundR, BackgroundG, BackgroundB));
            }
            set
            {
                BackgroundA = value.Color.A;
                BackgroundR = value.Color.R;
                BackgroundG = value.Color.G;
                BackgroundB = value.Color.B;
            }
        }

        internal Items? Parent { get; set; }
    }
”

”
ItemTemplates.xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:local="clr-namespace:_20260224">

  <DataTemplate DataType="{x:Type local:TextBlockItem}">
    <TextBlock Text="{Binding Text}" FontSize="16" Foreground="Black"
               Background="{Binding Background}"/>
  </DataTemplate>

  <DataTemplate DataType="{x:Type local:RectangleItem}">
    <Rectangle Width="{Binding Width}" 
               Height="{Binding Height}" 
               Fill="{Binding Background}" 
               Stroke="Black" StrokeThickness="1"/>
  </DataTemplate>

  <DataTemplate DataType="{x:Type local:Items}">
    <local:DiagramCanvas ItemsSource="{Binding Children}"
                         Background="{Binding Background}"/>
  </DataTemplate>
</ResourceDictionary>
”

”
MainWindow.xaml.cs
        private void PrepareData()
        {

            ItemsList.Add(new TextBlockItem(10, 10, "ゆっくりしていってね！！！") { Background = Brushes.Silver});
            ItemsList.Add(new RectangleItem(150, 50, 100, 50) { BackgroundR = 255, BackgroundG = 0, BackgroundB = 0 });

            var group = new Items(60, 150);
            group.Children.Add(new RectangleItem(0, 0, 89, 90) { Background = Brushes.Maroon });
            group.Children.Add(new TextBlockItem(10, 90, "これはグループ内です"));
            group.Background = Brushes.LightGray;
            ItemsList.Add(group);

        }

”


---


「色がつかない」最大の原因は、**`Items`（DiagramCanvas）のサイズが 0 になっているから**だと思われます。

WPFの `Canvas`（およびそれを継承した `DiagramCanvas`）は、**「中身に合わせて自分のサイズを自動で広げる」という機能を持っていません。** 子要素がどれだけ遠くに配置されても、Canvas自体の `Width` と `Height` を明示的に指定しない限り、サイズは 0×0 のままで、背景色は見えなくなります。

「子要素がすべて収まる範囲を視覚化する」ための、よりスマートで確実な方法を提案します。

---

### 1. なぜ今の方法で色が出ないのか？

1. **Canvasのサイズ問題**: `DiagramCanvas` の `Background` に色を塗っていますが、サイズ（Width/Height）が指定されていないため、面積ゼロの点に色が塗られている状態です。
2. **透過度の初期値**: `BackgroundA` の初期値が `0` です。もし `PrepareData` で色をセットし忘れると、完全透明になります。

---

### 2. おすすめの方法：`Border` で包む

`Items` の `DataTemplate` 内で、`DiagramCanvas` を `Border` で包みます。そして、**中のアイテムが収まるサイズを「計算」して Border のサイズに反映させる**のが最も「視覚化」に適した方法です。

#### ItemTemplates.xaml の修正案

`Items` のテンプレートを、背景を持つ「枠」として定義します。

```xml
<DataTemplate DataType="{x:Type local:Items}">
    <Border Background="{Binding Background}"
            BorderBrush="Gray"
            BorderThickness="1"
            CornerRadius="4">
        <local:DiagramCanvas ItemsSource="{Binding Children}"
                             Width="{Binding TotalWidth}"
                             Height="{Binding TotalHeight}"/>
    </Border>
</DataTemplate>

```

---

### 3. 子要素からサイズを自動計算するロジック

`Items` クラスの中で、`Children` の中身をスキャンして「一番右端」と「一番下端」を割り出し、`TotalWidth` / `TotalHeight` を提供するようにします。

#### Items クラスの改善

```csharp
public partial class Items : Item
{
    public Items(double x, double y) : base(x, y)
    {
        // 子供が追加・削除されたらサイズを再計算する
        Children.CollectionChanged += (s, e) => UpdateBounds();
    }

    [ObservableProperty] private double _totalWidth;
    [ObservableProperty] private double _totalHeight;

    public void UpdateBounds()
    {
        if (Children.Count == 0)
        {
            TotalWidth = 0;
            TotalHeight = 0;
            return;
        }

        // 全ての子要素の (X + 幅) と (Y + 高さ) の最大値を探す
        // 注意: RectangleItemなどは Width/Height を持っているが、
        // 基底の Item クラスにはないので型判定が必要
        double maxX = 0;
        double maxY = 0;

        foreach (var child in Children)
        {
            double w = 0, h = 0;
            if (child is RectangleItem r) { w = r.Width; h = r.Height; }
            else if (child is TextBlockItem t) { w = 100; h = 20; } // Textは概算か、ActualWidthが必要

            maxX = Math.Max(maxX, child.X + w);
            maxY = Math.Max(maxY, child.Y + h);
        }

        TotalWidth = maxX;
        TotalHeight = maxY;
    }
}

```

---

### 4. もっと手っ取り早い「デバッグ用」の方法

もしサイズ計算が面倒で、単に「グループがある場所をボヤッと塗りたい」だけなら、`DiagramCanvas` の中ではなく、**`DiagramCanvas` 自体を透明にして、後ろに別の図形を敷く**という手もありますが、やはり上記のように **`Border` のサイズを確定させる** のが WPF における正しい作法です。

### 💡 修正のアドバイス

まずは、`ItemTemplates.xaml` で `local:DiagramCanvas` に `MinWidth="100" MinHeight="100"` と書いてみてください。これで背景色が出れば、原因は「サイズが 0 だったこと」で確定です。

サイズ計算まで実装できれば、アイテムをドラッグして動かしたときに、背景の枠が「ニュルッ」と伸び縮みするカッコいい UI になりますよ！

次は、この **「サイズ自動更新」** を実装してみますか？それとも **「背景を自動計算せず、手動で広さを設定できるプロパティ」** を追加しますか？


---


# 概ね期待通りだけど、TextBlockのサイズ取得できないからぴったりじゃないのが不満、なにかいい方法ない？


---


TextBlock のサイズ（幅と高さ）を正確に取得できないのは、WPF 開発で誰もが一度はぶつかる壁です。

原因は、**「描画されるまで（フォントや文字数、レンダリング環境が決まるまで）サイズが確定しない」**からです。これを Model（クラス側）で事前に知ることは通常不可能です。

これを解決する「スマートでぴったり」な方法を 2 つ紹介します。

---

### 1. おすすめ：`Items` のサイズ計算を「View（XAML）」に任せる

Model で `TotalWidth` を計算するのをやめ、**`StackPanel` や `Grid` などの WPF の自動レイアウト機能**を賢く利用します。

`Canvas` はサイズを広げませんが、`Grid` や `StackPanel` は中身の `TextBlock` に合わせて勝手に広がります。これを利用して、`Items` の見た目を構成します。

#### ItemTemplates.xaml の修正案

```xml
<DataTemplate DataType="{x:Type local:Items}">
    <Border Background="{Binding Background}"
            BorderBrush="Gray"
            BorderThickness="1"
            HorizontalAlignment="Left"
            VerticalAlignment="Top">
        
        <Grid>
            <Canvas Visibility="Hidden"/> 
            
            <local:DiagramCanvas ItemsSource="{Binding Children}"/>
        </Grid>
    </Border>
</DataTemplate>

```

※ ただし、`Canvas` を使う以上、これだけでは `Border` は広がりません。そこで次の「テクニック」を使います。

---

### 2. 強力な解決策：`SizeChanged` イベントでモデルへ書き戻す

TextBlock などの「View 側で確定したサイズ」を、モデルの `Width` / `Height` プロパティへ逆流させる方法です。

#### 手順 A: TextBlockItem に Width/Height を追加

```csharp
public partial class TextBlockItem : Item {
    [ObservableProperty] private double _width;
    [ObservableProperty] private double _height;
    // ...
}

```

#### 手順 B: XAML で `ActualWidth` をモデルに送る

WPF の `ActualWidth` は読み取り専用なので直接バインドできませんが、`SizeChanged` イベントで値を書き戻せます。

```xml
<DataTemplate DataType="{x:Type local:TextBlockItem}">
    <TextBlock Text="{Binding Text}" 
               Background="{Binding Background}"
               SizeChanged="TextBlock_SizeChanged" />
</DataTemplate>

```

#### 手順 C: コードビハインド（MainWindow.xaml.cs など）

```csharp
private void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
{
    if (sender is TextBlock tb && tb.DataContext is TextBlockItem item)
    {
        item.Width = e.NewSize.Width;
        item.Height = e.NewSize.Height;
        
        // 親がいるなら親の範囲を再計算させる
        item.Parent?.UpdateBounds(); 
    }
}

```

---

### 3. 最も正確な「裏技」：FormattedText を使う（Model 内で完結）

もし、どうしても Model 側でサイズを知りたい場合は、内部的に描画をシミュレートする `FormattedText` クラスを使います。

```csharp
public void UpdateWidth()
{
    var formattedText = new FormattedText(
        this.Text,
        CultureInfo.CurrentCulture,
        FlowDirection.LeftToRight,
        new Typeface("Segoe UI"), // 実際のフォント名
        16, // FontSize
        Brushes.Black,
        VisualTreeHelper.GetDpi(new Visual()).PixelsPerDip);

    this.Width = formattedText.Width;
    this.Height = formattedText.Height;
}

```

### 💡 どっちが良い？

* **完璧な「ぴったり感」を求めるなら**: **「2」の方法**です。実際に描画されたサイズをモデルに書き戻すので、1ドットの狂いもありません。
* **コードをシンプルにしたいなら**: **「1」の方法**をベースに、`Items` の `DataTemplate` 内で `Canvas` ではなく `StackPanel` などで包む構成を検討します（ただし自由配置は難しくなります）。

まずは **「2」の `SizeChanged` を使って、確定したサイズを親（Items）に教える** 方法を試してみませんか？これなら `Items` の `UpdateBounds` ロジックもそのまま活かせます。

次は、この **「サイズの書き戻しロジック」** を実装して、背景がぴったり吸い付くようにしてみましょうか？


---


# 「2」の方法を試したけど、エラーになる
”'ResourceDictionary' ルート要素は、XAML ファイルでイベント ハンドラーをサポートするために、x:Class 属性を必要とします。SizeChanged イベントのイベント ハンドラーを削除するか、x:Class 属性をルート要素に追加してください。 行 9 位置 16.”
っていわれるけどResourceDictionaryは以下になっている
”<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:local="clr-namespace:_20260224">

  <DataTemplate DataType="{x:Type local:TextBlockItem}">
    <TextBlock Text="{Binding Text}" FontSize="16" Foreground="Black"
               Background="{Binding Background}"
               SizeChanged="TextBlock_SizeChanged"/>
  </DataTemplate>
”


---


そのエラーは、WPFの非常に「お堅い」ルールに接触したために発生しています。

通常、`ResourceDictionary` は純粋なデータ（XAML）だけのファイルです。しかし、そこに **`SizeChanged="..."` のようなイベントハンドラー** を書くと、WPFは「このクリックやサイズ変更を処理するための **C#のコード（コードビハインド）** はどこにあるの？」と探しに行きます。

解決策は2つあります。

---

### 解決策1：ResourceDictionary に「コードビハインド」を作る

`ItemTemplates.xaml` に対応する C# ファイルを紐付ける方法です。これが最も直接的な修正です。

#### 1. XAML側 (`ItemTemplates.xaml`)

ルート要素に `x:Class` を追加します。

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:local="clr-namespace:_20260224"
                    x:Class="_20260224.ItemTemplates"> <DataTemplate DataType="{x:Type local:TextBlockItem}">
        <TextBlock Text="{Binding Text}" FontSize="16"
                   Background="{Binding Background}"
                   SizeChanged="TextBlock_SizeChanged"/>
    </DataTemplate>
    ...
</ResourceDictionary>

```

#### 2. C#側 (`ItemTemplates.xaml.cs`)

新しく C# ファイルを作成し、イベントの中身を書きます。

```csharp
namespace _20260224
{
    // partial クラスとして定義。名前は x:Class で指定したものと一致させる
    public partial class ItemTemplates : ResourceDictionary
    {
        public ItemTemplates()
        {
            InitializeComponent();
        }

        private void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is TextBlock tb && tb.DataContext is TextBlockItem item)
            {
                item.Width = e.NewSize.Width;
                item.Height = e.NewSize.Height;
                item.Parent?.UpdateBounds(); // 親にサイズ変更を通知
            }
        }
    }
}

```

---

### 解決策2：Behavior（ビヘイビア）を使う（推奨）

もし「ResourceDictionary に C# ファイルを作るのは管理が面倒だ」と感じるなら、イベントを使わずに **「プロパティに値を書き戻す専用の部品」** を作って貼り付ける方法があります。

これには `Microsoft.Xaml.Behaviors.Wpf` などのライブラリが必要になることが多いですが、実は **「添付プロパティ」** を自作するだけで、外部ライブラリなしでもスマートに実装できます。

#### 実装例：`SizeObserver.cs` を作る

```csharp
public static class SizeObserver
{
    public static readonly DependencyProperty ObserveProperty =
        DependencyProperty.RegisterAttached("Observe", typeof(bool), typeof(SizeObserver),
        new FrameworkPropertyMetadata(OnObserveChanged));

    public static void SetObserve(DependencyObject obj, bool value) => obj.SetValue(ObserveProperty, value);

    private static void OnObserveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FrameworkElement fe)
        {
            fe.SizeChanged += (s, args) =>
            {
                if (fe.DataContext is TextBlockItem item)
                {
                    item.Width = args.NewSize.Width;
                    item.Height = args.NewSize.Height;
                    item.Parent?.UpdateBounds();
                }
            };
        }
    }
}

```

#### XAML側

```xml
<TextBlock Text="{Binding Text}" local:SizeObserver.Observe="True" />

```

---

### 💡 どちらが良い？

* **今の流れを止めずに進むなら**: **解決策1** です。単純に C# ファイルを1つ追加するだけでエラーが消えます。
* **今後 DataTemplate を増やし、ロジックを綺麗に保ちたいなら**: **解決策2** です。イベントハンドラーをいちいち書かなくて済むため、XAML が汚れず、再利用性が高まります。

まずは **解決策1** で、`ItemTemplates.xaml.cs` を作ってみるのが一番近道ですよ！

無事にエラーが消えたら、次は `Items` の背景が **TextBlock の文字数に合わせて「シュッ」と伸び縮みするか** 確認してみましょう。上手くいきましたか？


---


# ぴったりになった！でも難しい、SizeObserver.Observeを解説して、とくにbool型なのがわからない


---


「ぴったりになった」とのこと、素晴らしいです！
確かにこの「添付プロパティ」という仕組みは、WPFの中で最も魔法じみていて、かつ最初は理解しづらい部分です。

なぜ `bool` 型なのか、そして中で何が起きているのかを噛み砕いて解説します。

---

### 1. なぜ `bool`（True/False）なのか？

この `bool` は、**「この機能（サイズ監視）を使いますか？」というスイッチ**の役割をしています。

WPFの標準の `TextBlock` には、「自分のサイズが変わったことをModelに報告する」という機能は備わっていません。そこで、外部から**「後付けの機能」**をペタッと貼り付ける必要があります。

* **`local:SizeObserver.Observe="True"`** と書くことで、「この TextBlock は監視対象だよ！」とフラグを立てます。
* このフラグが `True` になった**瞬間**、裏側で「監視用のプログラム」が起動するように設計されています。

---

### 2. 裏側で起きていること（コードの解読）

ソースコードの重要なポイントはここです：

```csharp
private static void OnObserveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
{
    // 1. スイッチが押された(Trueになった)対象が FrameworkElement か確認
    if (d is FrameworkElement fe)
    {
        // 2. 「サイズが変わった時に実行する処理」を登録する
        fe.SizeChanged += (s, args) =>
        {
            // 3. Model(TextBlockItem) を見つけて、新しいサイズを書き込む
            if (fe.DataContext is TextBlockItem item)
            {
                item.Width = args.NewSize.Width;
                item.Height = args.NewSize.Height;
                item.Parent?.UpdateBounds();
            }
        };
    }
}

```

### 3. この仕組みの「すごさ」

なぜわざわざこんな難しいことをするのかというと、**「TextBlock 自体を改造しなくて済むから」**です。

* **普通の方法**: `MySmartTextBlock` という新しいクラスを作って、中で `SizeChanged` を処理する。
* **添付プロパティ（これ）**: 標準の `TextBlock` のまま、外から「サイズが変わったら Model に値を送る」という**新しい性格を後付け**できる。

これを専門用語で **「振る舞い（Behavior）の注入」** と呼びます。

---

### 4. `bool` 以外の可能性は？

実は `bool` である必要はありません。例えば、

* `SizeObserver.TargetModel="{Binding}"` のように、Modelを直接渡す型にすることもできます。
* しかし、「スイッチ（bool）をオンにしたら、自動的にそのコントロールの `DataContext`（中身のModel）を探しに行く」という作りが一番 XAML を汚さずシンプルなので、慣習的に `bool` が使われます。

---

### 💡 まとめると

1. **XAML**: 「この TextBlock の監視を **True** にしてね」と頼む。
2. **C#**: 「お、**True** になったな。じゃあ `SizeChanged` イベントを監視し始めて、結果を Model に書き戻してあげるよ」と動く。

この仕組みのおかげで、XAML側はイベントハンドラー（SizeChanged="..."）を書く必要がなくなり、純粋に「設定（Observe=True）」だけを書けば済むようになります。

---

これで `SizeObserver` の正体が見えてきましたか？

背景がぴったり吸い付くようになったので、次は**「Itemsの背景をクリックして、グループ全体をマウスでズルズルと動かすドラッグ機能」**の実装にチャレンジしてみますか？それとも、今の状態を **JSON で保存して、再起動しても「ぴったり」が維持されるか** 確認しますか？


---


# SizeChangedなのか、別のイベントなのかの判断や指定は、どこでしているの？


---


その判断と指定は、`SizeObserver.cs` の中にある **`OnObserveChanged` メソッド** の中で、あなたがプログラミング（ハードコーディング）として記述しています。

具体的には、以下のコードの **`fe.SizeChanged += ...`** の部分がその「判断と指定」の正体です。

### 仕組みのポイント

```csharp
private static void OnObserveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
{
    if (d is FrameworkElement fe)
    {
        // ここで「どのイベントに反応するか」を明示的に指定している
        fe.SizeChanged += (s, args) =>
        {
            // イベントが発生した時の「処理」をここに書いている
        };
    }
}

```

### なぜここで指定するのか？

添付プロパティ（`SizeObserver`）は、**「プロパティの値が変わった（スイッチが入った）瞬間に、フック（割り込み用コード）を仕掛ける」**という動きをします。

1. **スイッチオン**: XAMLで `Observe="True"` と書かれる。
2. **フック発動**: `OnObserveChanged` が呼び出される。
3. **イベント購読**: そのメソッド内で、「よし、このコントロールの **`SizeChanged` イベント** に私の処理を登録しておこう」と指定する。

### もし「クリック」に反応させたいなら？

例えば、サイズ変更ではなく「クリックされた時に何かしたい」という別の機能（例：`ClickObserver`）を作りたいなら、この部分を書き換えることになります。

```csharp
// もしクリックを監視するなら...
fe.MouseDown += (s, args) => { /* クリック時の処理 */ };

```

### まとめ：判断の場所

* **どのイベントか？**: `OnObserveChanged` 内のコードで `+=` を使ってイベント名を指定しています。
* **なぜ SizeChanged か？**: あなたが「サイズが確定した瞬間に Model へ書き戻したい」という目的のために、意図的にそのイベントを選んでコードを書いたからです。

つまり、`SizeObserver` という名前自体に特別な意味があるのではなく、**「中身で `SizeChanged` を購読しているから、サイズを監視する部品として機能している」**ということになります。

---

次は、この「イベントを購読する」仕組みを応用して、**「マウスで掴んで移動させる（MouseDown / MouseMove イベントを使う）」** 実装に移りますか？

それとも、この `SizeObserver` のコードをもっと汎用的に（例えば、どんなプロパティでも監視できるように）改造してみたいですか？


# もしアプリ起動後に、何らかの方法でObserveがtrueからFalseに変更されたらSizeChangedイベントの二重購読になる？


---


鋭い指摘です！結論から言うと、今のコードのままだと **「二重購読」は起きませんが、「イベントが解除されずに残り続ける（メモリリークの種になる）」** という別の問題が発生する可能性があります。

また、もし「False → True」と何度も切り替えた場合は、**確かに二重・三重に購読が追加されてしまいます。**

---

### なぜ二重購読が起きるのか？

`fe.SizeChanged += ...` は、「イベントハンドラを追加する」命令です。`OnObserveChanged` が呼ばれるたびにこれを実行すると、古いハンドラが残ったまま新しいハンドラが追加され、1回のサイズ変更で同じ処理が何度も走るようになります。

### ✅ 正しい実装：解除（-=）も行う

スイッチが `False` になった時に購読を解除し、`True` になった時も「念のため一度解除してから登録する」のが WPF の定石です。

#### 修正版：SizeObserver.cs

```csharp
private static void OnObserveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
{
    if (d is FrameworkElement fe)
    {
        // 1. まずは古い購読を解除（二重登録防止）
        fe.SizeChanged -= FrameworkElement_SizeChanged;

        // 2. 新しい値が True なら登録する
        if ((bool)e.NewValue)
        {
            fe.SizeChanged += FrameworkElement_SizeChanged;
        }
    }
}

// ラムダ式ではなく、名前付きメソッドにすると解除(-=)ができるようになる
private static void FrameworkElement_SizeChanged(object sender, SizeChangedEventArgs e)
{
    if (sender is FrameworkElement fe && fe.DataContext is TextBlockItem item)
    {
        item.Width = e.NewSize.Width;
        item.Height = e.NewSize.Height;
        item.Parent?.UpdateBounds();
    }
}

```

---

### ここがポイント！

1. **名前付きメソッドにする**:
ラムダ式 `(s, args) => { ... }` を使うと、`-= ` で解除することができません（解除するには「登録した時と全く同じインスタンス」を指定する必要があるため）。そのため、`FrameworkElement_SizeChanged` という名前を付けて定義します。
2. **まず引く、それから足す**:
`fe.SizeChanged -= ...` を実行してから `+=` をすることで、現在登録されているかどうかに関わらず、常にハンドラが「1つだけ」の状態を保てます。
3. **メモリリーク防止**:
もしこのコントロールが画面から消えても、イベントが購読されたままだと、ガベージコレクション（GC）が「まだ使われている」と判断してメモリを解放してくれないことがあります。スイッチを `False` にした時にしっかり `-=` するのは、プロのコードとして非常に重要です。

---

### 💡 豆知識：Unloaded イベントでの掃除

さらに徹底するなら、そのコントロールが画面から消えた（Unloaded）タイミングでも `-=` を呼ぶように作ると完璧です。

これで「完璧な SizeObserver」になりましたね！

次は、この頑丈になった仕組みの上で、**「マウスドラッグ移動」**を実装しますか？それとも、**「サイズが変わった時に背景の枠がアニメーションで伸び縮みする」** ような視覚効果を狙ってみますか？


---


# DiagramCanvas全体、またはその中の指定した要素だけを画像として保存したい。どんな方法がある？


---


WPFでコントロールを画像として保存するには、**`RenderTargetBitmap`** というクラスを使用するのが王道です。これを使えば、画面に表示されている見た目そのものを、ピクセルデータとしてキャプチャできます。

「キャンバス全体」と「特定の要素」のどちらにも対応できる、汎用的な実装方法を解説します。

---

## 1. 基本の仕組み：RenderTargetBitmap

WPFのビジュアル要素（Visual）をビットマップに変換する手順は以下の通りです。

1. **ターゲットのサイズを確認**: `ActualWidth` / `ActualHeight` を取得。
2. **描画の実行**: `RenderTargetBitmap.Render(visual)` を呼ぶ。
3. **エンコード**: PNGやJPEGなどの形式に変換してファイル保存。

---

## 2. 実装：画像保存用のヘルパーメソッド

これを `DiagramCanvas` クラスの中、あるいは静的ヘルパーとして実装します。

```csharp
public static void SaveToImage(FrameworkElement element, string filePath)
{
    // 1. サイズの確定（レンダリング前の場合は強制更新が必要な場合あり）
    int width = (int)element.ActualWidth;
    int height = (int)element.ActualHeight;

    if (width <= 0 || height <= 0) return;

    // 2. RenderTargetBitmapの作成 (96DPI)
    RenderTargetBitmap rtb = new RenderTargetBitmap(
        width, height, 96, 96, PixelFormats.Pbgra32);

    // 3. 要素をレンダリング
    rtb.Render(element);

    // 4. PNGとしてエンコードして保存
    PngBitmapEncoder encoder = new PngBitmapEncoder();
    encoder.Frames.Add(BitmapFrame.Create(rtb));

    using (var fs = System.IO.File.OpenWrite(filePath))
    {
        encoder.Save(fs);
    }
}

```

---

## 3. 「全体」か「指定要素」かの使い分け

### ① キャンバス全体を保存したい場合

`DiagramCanvas` 自体を引数に渡します。

```csharp
// DiagramCanvasのインスタンスを myCanvas とすると
SaveToImage(myCanvas, "full_board.png");

```

### ② 指定した特定のアイテム（Rectangleなど）だけ保存したい場合

DataTemplate 内で生成された「実体（FrameworkElement）」を渡す必要があります。
マウス操作などで選択されたアイテムの **UI要素** を取得して渡します。

---

## 4. より高度な方法：背景を透過させる・範囲を絞る

もし「特定のアイテムだけを保存したいが、その周辺も含めた範囲で切り抜きたい」といった場合は、以下の工夫が考えられます。

* **クリッピング**: `DrawingVisual` を使って、特定の Rect 範囲だけを `RenderTargetBitmap` に転写します。
* **非表示要素の除外**: 保存する瞬間だけ、不要な要素の `Visibility` を `Hidden` にして `Render` を実行します（`Hidden` ならレイアウトを維持したまま画像に写りません）。

---

## ⚠️ 注意点：DPIスケーリング

Windowsの設定で「テキストの拡大（125%など）」にしている場合、そのまま保存すると画像がボケたり、サイズがズレたりすることがあります。

完璧な解像度を保つには、以下のようにシステムのDPIを取得して `RenderTargetBitmap` に渡すのがプロの技です。

```csharp
Visual visual = element;
var source = PresentationSource.FromVisual(visual);
double dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
double dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;

RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, dpiX, dpiY, PixelFormats.Pbgra32);

```

---

## 💡 次のステップ

これで画像保存の準備は整いました！

* **「ボタンを押したら保存ダイアログ（SaveFileDialog）が出て保存する」** 機能を MainWindow に追加してみますか？
* それとも、保存した画像を JSON データの `Thumbnail`（サムネイル）として利用するために、**「Base64文字列に変換して保存する」** ロジックに興味がありますか？

どちらの方法を深掘りしましょう？


---


# 「ボタンを押したら保存ダイアログ（SaveFileDialog）が出て保存する」 機能を MainWindow に追加して


---


「ボタンをクリックしたら、今見ているキャンバスを画像として保存する」機能を実装しましょう。

この機能を実現するには、**「View（ボタンのクリック）」→「Logic（画像変換と保存）」** という流れを作ります。今回は `DiagramCanvas` に名前を付けて、コードビハインドからアクセスする方法で解説します。

---

### 1. MainWindow.xaml の準備

まず、`DiagramCanvas` にプログラムから参照するための名前（`x:Name`）を付け、保存用のボタンを配置します。

```xml
<Window ...>
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/> <RowDefinition Height="*"/>    </Grid.RowDefinitions>

        <Button Content="画像として保存" 
                Click="SaveButton_Click" 
                Margin="10" Padding="10,5"/>

        <local:DiagramCanvas x:Name="MyDiagramCanvas" 
                             Grid.Row.1
                             ItemsSource="{Binding ItemsList}" 
                             Background="White"/>
    </Grid>
</Window>

```

---

### 2. MainWindow.xaml.cs の実装

保存ダイアログを表示し、先ほど解説した `RenderTargetBitmap` のロジックを呼び出します。

```csharp
using Microsoft.Win32; // SaveFileDialog用
using System.IO;
using System.Windows.Media.Imaging;

private void SaveButton_Click(object sender, RoutedEventArgs e)
{
    // 1. 保存ダイアログの設定
    SaveFileDialog saveFileDialog = new SaveFileDialog
    {
        Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg",
        Title = "キャンバスを画像として保存",
        FileName = "MyDiagram.png"
    };

    // 2. ダイアログを表示して「保存」が押されたら実行
    if (saveFileDialog.ShowDialog() == true)
    {
        try
        {
            ExportToImage(MyDiagramCanvas, saveFileDialog.FileName);
            MessageBox.Show("保存が完了しました！");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"保存中にエラーが発生しました: {ex.Message}");
        }
    }
}

private void ExportToImage(FrameworkElement element, string filePath)
{
    // 現在の表示サイズを取得
    int width = (int)element.ActualWidth;
    int height = (int)element.ActualHeight;

    // DPI設定（100%表示なら96）
    double dpi = 96;

    // レンダリング用ビットマップの作成
    RenderTargetBitmap rtb = new RenderTargetBitmap(
        width, height, dpi, dpi, PixelFormats.Pbgra32);

    // 要素を描画
    rtb.Render(element);

    // 拡張子に応じてエンコーダーを選択
    BitmapEncoder encoder;
    if (filePath.ToLower().EndsWith(".jpg") || filePath.ToLower().EndsWith(".jpeg"))
    {
        encoder = new JpegBitmapEncoder();
    }
    else
    {
        encoder = new PngBitmapEncoder();
    }

    encoder.Frames.Add(BitmapFrame.Create(rtb));

    // ファイル書き込み
    using (var stream = File.Create(filePath))
    {
        encoder.Save(stream);
    }
}

```

---

### 3. この実装のポイント

* **`ActualWidth / ActualHeight`**: ウィンドウのサイズを変えると、保存される画像のサイズも連動して変わります。
* **`PixelFormats.Pbgra32`**: 背景を透明にしたい場合は PNG 形式が最適です。`DiagramCanvas` の `Background` が `Transparent`（または未指定）なら、背景が透けた画像が作れます。
* **エラーハンドリング**: ファイルが他のソフトで開かれている場合などに備え、`try-catch` で囲むのが安全です。

---

### 💡 さらに便利にするアイデア

今のままだと「画面に見えている範囲」しか保存されません。もし **「画面外に突き出ている部分も含めて、全てのアイテムが収まるサイズで保存したい」** 場合は、少し工夫が必要です。

1. `Items` クラスで作った `TotalWidth` / `TotalHeight` を使う。
2. 保存する瞬間だけ `MyDiagramCanvas` の `Width / Height` をそのサイズに一時的に変更する。
3. `UpdateLayout()` を呼んでから `Render()` する。

これで「完璧な一枚絵」を書き出せるようになります。

次は、この **「画面外も含めた全体保存」** に挑戦しますか？それとも、そろそろ **「マウスでのドラッグ操作」** を作って、図形を自由に並べ替えられるようにしましょうか？


---


# DiagramCanvasのサイズもItemsと同じように子要素がピッタリ収まるサイズにしたい、
ItemsのUpdateBoundsメソッドと同じような処理が必要になると思うけど、同じことを書くのはめんどくさい。いい方法はある？


---


その直感は正しいです！「全く同じロジックを二箇所に書く」のは、バグの温床になります。

`DiagramCanvas` も `Items` も、「子要素の範囲を計算する」という目的は同じですよね。これを解決する最もクリーンな方法は、**「サイズ計算のロジックを Model 側の基底クラス（Item）に共通機能として持たせる」** ことです。

以下の手順で、二度手間を防ぎつつ、すべてが自動で連動する仕組みを作りましょう。

---

### 1. 基底クラス `Item` にサイズ計算の入り口を作る

まず、すべてのアイテム（TextBlockItem, RectangleItem, Items自身）が「自分の右端と下端はどこか？」を答えられるようにします。

```csharp
public abstract partial class Item : ObservableObject
{
    // ... 既存の X, Y などのプロパティ ...

    // そのアイテムが占める「右端」と「下端」を返す仮想プロパティ
    [JsonIgnore]
    public virtual double Right => X + (this is RectangleItem r ? r.Width : 0); // 暫定
    [JsonIgnore]
    public virtual double Bottom => Y + (this is RectangleItem r ? r.Height : 0);
}

```

---

### 2. 計算ロジックを「共通メソッド」として切り出す

`Items` クラス（グループ）と、メインの `ViewModel`（Canvas全体）の両方で使える計算メソッドを作ります。

```csharp
public static class BoundsCalculator
{
    public static (double Width, double Height) GetTotalSize(IEnumerable<Item> items)
    {
        if (!items.Any()) return (0, 0);

        // 各アイテムの「右端」と「下端」の最大値を探す
        // ここで TextBlockItem や Items(グループ) も含めた計算が行われる
        double maxX = items.Max(i => i.Right);
        double maxY = items.Max(i => i.Bottom);

        return (maxX, maxY);
    }
}

```

---

### 3. 各クラスでの実装

#### Items クラス（グループ）

自分の `Children` を渡すだけでサイズが確定します。

```csharp
public partial class Items : Item
{
    [ObservableProperty] private double _totalWidth;
    [ObservableProperty] private double _totalHeight;

    public void UpdateBounds()
    {
        var size = BoundsCalculator.GetTotalSize(Children);
        TotalWidth = size.Width;
        TotalHeight = size.Height;
    }

    // グループ自体の右端・下端は、自分の(X,Y) + 内部の広さ
    public override double Right => X + TotalWidth;
    public override double Bottom => Y + TotalHeight;
}

```

#### MainWindowViewModel (Canvas全体)

メインの `ItemsList` を同じメソッドに渡せば、Canvas全体のサイズがわかります。

```csharp
// ViewModel側
public void UpdateCanvasSize()
{
    var size = BoundsCalculator.GetTotalSize(ItemsList);
    CanvasWidth = size.Width;
    CanvasHeight = size.Height;
}

```

---

### 4. XAMLでのバインド

`DiagramCanvas` のサイズを ViewModel のプロパティにバインドします。

```xml
<local:DiagramCanvas Width="{Binding CanvasWidth}" 
                     Height="{Binding CanvasHeight}" 
                     ItemsSource="{Binding ItemsList}" />

```

---

### 💡 この方法のすごいところ

1. **ロジックの一本化**: 「右端 = X + 幅」という計算式を `BoundsCalculator` だけが知っていれば良くなります。
2. **再帰的な計算**: `Items`（グループ）が `Items` を含んでいる場合でも、`Right` プロパティを `override` しているおかげで、芋づる式に正しい全体のサイズが計算されます。
3. **TextBlockへの対応**: 前に作った `SizeObserver` で `TextBlockItem` の `Width/Height` が更新されるようにしておけば、`Right = X + Width` の計算に自動的に最新の文字幅が反映されます。

---

### 次のステップへの提案

これで、グループもキャンバス全体も、中身に合わせて「ぴったり」サイズが変わるようになりました！

この「サイズが自動で変わる」状態で、**「マウスでアイテムを動かしたときに、リアルタイムでキャンバスが伸び縮みする」** ようにしてみませんか？それとも、この計算ロジックが複雑になってきたので、一度 **「整理された最新のクラス図」** を確認したいですか？


---


# 以下はどこに書くのかわからない、ViewModel側ってどれ？CanvasWidthってどこにある？
// ViewModel側
public void UpdateCanvasSize()
{
    var size = BoundsCalculator.GetTotalSize(ItemsList);
    CanvasWidth = size.Width;
    CanvasHeight = size.Height;
}


---


混乱させてしまいすみません！これまでの実装では、**MainWindow（コードビハインド）そのものを ViewModel 代わり**にしていましたね。

「ViewModel 側」と言ったのは、現在 `ItemsList` を持っている場所（今の構成なら `MainWindow.xaml.cs`）のことです。`CanvasWidth` はまだ作っていないプロパティなので、今から一緒に追加しましょう。

手順は以下の通りです。

---

### 1. MainWindow.xaml.cs にプロパティを追加する

`ItemsList` を定義している場所に、Canvas のサイズを保持するプロパティを追加します。これらは `INotifyPropertyChanged`（または `ObservableObject`）の仕組みに乗せる必要があります。

```csharp
public partial class MainWindow : Window, INotifyPropertyChanged
{
    // 既存のリスト
    public ObservableCollection<Item> ItemsList { get; set; } = new();

    // --- ここから追加 ---
    private double _canvasWidth;
    public double CanvasWidth
    {
        get => _canvasWidth;
        set { _canvasWidth = value; OnPropertyChanged(nameof(CanvasWidth)); }
    }

    private double _canvasHeight;
    public double CanvasHeight
    {
        get => _canvasHeight;
        set { _canvasHeight = value; OnPropertyChanged(nameof(CanvasHeight)); }
    }

    // サイズを計算して更新するメソッド
    public void UpdateCanvasSize()
    {
        var size = BoundsCalculator.GetTotalSize(ItemsList);
        CanvasWidth = size.Width;
        CanvasHeight = size.Height;
    }
    // --- ここまで追加 ---

    // PropertyChanged の実装（CommunityToolkitを使っているなら不要ですが念のため）
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

```

---

### 2. MainWindow.xaml のバインドを修正する

`DiagramCanvas` が、今作った `CanvasWidth / CanvasHeight` を見に行くように紐付けます。

```xml
<local:DiagramCanvas x:Name="MyDiagramCanvas" 
                     ItemsSource="{Binding ItemsList}" 
                     Width="{Binding CanvasWidth}" 
                     Height="{Binding CanvasHeight}"
                     Background="White"
                     HorizontalAlignment="Left" 
                     VerticalAlignment="Top"/>

```

> **ポイント**: `HorizontalAlignment="Left"` と `VerticalAlignment="Top"` を入れることで、Canvas が左上に固定され、右下に向かって伸び縮みする様子がわかりやすくなります。

---

### 3. いつ計算を実行するか？

今のままでは、プロパティを作っただけで中身が計算されません。以下のタイミングで `UpdateCanvasSize()` を呼び出すようにします。

1. **データの準備が終わった直後** (`PrepareData` の最後)
2. **アイテムが追加・削除されたとき** (`ItemsList.CollectionChanged`)
3. **アイテムが移動したりサイズが変わったとき** (本当はこれが一番重要)

#### 修正後の MainWindow コンストラクタ例

```csharp
public MainWindow()
{
    InitializeComponent();
    PrepareData();
    
    // リストの中身が変わったらサイズを再計算
    ItemsList.CollectionChanged += (s, e) => UpdateCanvasSize();
    
    // 初期サイズを計算
    UpdateCanvasSize();

    this.DataContext = this;
}

```

---

### 💡 補足：さらに「自動化」するには？

実は、アイテムが「移動（X, Y が変更）」したときに自動で Canvas サイズを変えるには、リスト内の全アイテムのプロパティ変更を監視する必要があります。

これを行うには、`ItemsList` に入れる各アイテムの `PropertyChanged` イベントを拾うコードが必要になりますが、まずは **「ボタンを押したとき」や「初期表示時」** に `UpdateCanvasSize()` を呼ぶところから始めて、正しくサイズが変わるか確認してみてください。

これで `CanvasWidth` の場所と役割はスッキリしましたか？
次は、いよいよアイテムをマウスで動かして、**「動かすたびに Canvas のサイズがリアルタイムに変わる」** ロジックに進みましょうか？


---


# MainWindow.xaml.csに処理を書くのは違うと思う。CustomControlであるDiagramCanvas.csに処理を書くのはどうかな？


---


その考えは、**「コントロールの自律性」を高めるという意味で非常に素晴らしい視点**です！

確かに、使う側（MainWindow）がいちいち「サイズを計算してプロパティにセットする」という手間をかけるのは、良いコンポーネント設計とは言えません。`DiagramCanvas` 自体が**「自分の ItemsSource を監視して、自動でサイズを広げる」**という機能を持っていれば、使う側はデータだけを渡せば良くなります。

ただし、WPFの設計には少し「癖」があるため、以下の方法で実装するのがベストです。

---

### 1. 「ViewModel」から「Controlの責務」へ

MainWindow の `CanvasWidth` プロパティは消してしまいましょう。代わりに、`DiagramCanvas.cs` 自体にサイズ計算のロジックを組み込みます。

#### DiagramCanvas.cs の修正

`ItemsSource` がセットされたことを検知して、中身のアイテムたちを監視し始めます。

```csharp
public class DiagramCanvas : ItemsControl
{
    // コンストラクタ
    public DiagramCanvas()
    {
        // ItemsSource が変わったときに通知を受け取る
        var dpd = DependencyPropertyDescriptor.FromProperty(ItemsSourceProperty, typeof(DiagramCanvas));
        dpd.AddValueChanged(this, OnItemsSourceChanged);
    }

    private void OnItemsSourceChanged(object? sender, EventArgs e)
    {
        if (ItemsSource is INotifyCollectionChanged collection)
        {
            // アイテムが追加・削除されたら再計算
            collection.CollectionChanged -= OnItemsListChanged;
            collection.CollectionChanged += OnItemsListChanged;
        }
        UpdateSize();
    }

    private void OnItemsListChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // 1. 新しく追加されたアイテムの移動（X, Y）を監視する
        if (e.NewItems != null)
        {
            foreach (Item item in e.NewItems)
                item.PropertyChanged += OnItemPropertyChanged;
        }
        
        // 2. 削除されたアイテムの監視を外す（メモリリーク防止）
        if (e.OldItems != null)
        {
            foreach (Item item in e.OldItems)
                item.PropertyChanged -= OnItemPropertyChanged;
        }

        UpdateSize();
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // X, Y, Width, Height が変わったらサイズを再計算
        if (e.PropertyName == "X" || e.PropertyName == "Y" || 
            e.PropertyName == "Width" || e.PropertyName == "Height")
        {
            UpdateSize();
        }
    }

    private void UpdateSize()
    {
        if (ItemsSource == null) return;
        
        var items = ItemsSource.Cast<Item>();
        var size = BoundsCalculator.GetTotalSize(items);

        // 自分自身の Width / Height を更新
        this.Width = size.Width;
        this.Height = size.Height;
    }
}

```

---

### 2. この実装のメリット

* **MainWindowがスッキリする**: `MainWindow.xaml` から `Width="{Binding ...}"` の記述が消え、ただタグを置くだけになります。
* **リアルタイム連動**: モデルの `X` や `Y` が変わった瞬間に、`DiagramCanvas` がそれを検知して勝手にサイズを広げます。
* **再利用性**: 他のプロジェクトで `DiagramCanvas` を使うときも、サイズ計算ロジックが既に内蔵されているので、何も考えずに使えます。

---

### 3. 注意： `DependencyPropertyDescriptor` について

`ItemsControl` の `ItemsSource` は依存関係プロパティ（DP）ですが、プロパティ自体が上書きされたときの通知をキャッチするために、少し特殊な書き方（`DependencyPropertyDescriptor`）を使っています。

### 💡 結論：どこに書くべきか

今回の「サイズを中身に合わせる」という挙動は、**「見た目の制御」に密接に関わるロジック**なので、**`DiagramCanvas.cs`（CustomControl側）に書くのが正解**です。

これで、MainWindow は「データの用意」だけ、DiagramCanvas は「表示の管理（サイズ計算含む）」という綺麗な分業ができました。

これで準備は整いましたね！
いよいよ、この自動でサイズが変わるキャンバスの上で、**「マウスでアイテムを掴んで動かす（ドラッグ移動）」** のコードを `DiagramCanvas.cs` に追加していきましょうか？


---


# まだ煩雑な気がする。DiagramCanvas自体にItemsを持たせたらどうかな？


---


その考えは、さらに一歩進んで**「独自の Canvas 型」を確立する**という非常に鋭い設計思想ですね。

実は、WPF 自体も `Canvas` クラスを継承して特定のデータに特化させる手法はよく使われます。`DiagramCanvas` 自体が `Items`（グループ）と同じ性質、つまり**「座標を持ち、子要素（Children）を管理する」**という役割を担うのは、論理的にとても整合性が取れています。

この構成にすると、以下のように極めてシンプルになります。

---

### 1. 理想的な構成：DiagramCanvas が Items を継承する

`DiagramCanvas` を単なる `ItemsControl` ではなく、**「`Items` クラスを表示するための専用コンテナ」** として再定義します。

#### DiagramCanvas.cs

```csharp
public class DiagramCanvas : ItemsControl
{
    static DiagramCanvas()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DiagramCanvas), 
            new FrameworkPropertyMetadata(typeof(DiagramCanvas)));
    }

    // ここで直接 Items(Model) を受け取る
    public Items? TargetItems
    {
        get => (Items)GetValue(TargetItemsProperty);
        set => SetValue(TargetItemsProperty, value);
    }

    public static readonly DependencyProperty TargetItemsProperty =
        DependencyProperty.Register(nameof(TargetItems), typeof(Items), typeof(DiagramCanvas), 
            new PropertyMetadata(null, OnTargetItemsChanged));

    private static void OnTargetItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var canvas = (DiagramCanvas)d;
        if (e.NewValue is Items newItems)
        {
            // Canvasの ItemsSource を、Modelの Children に直結させる
            canvas.ItemsSource = newItems.Children;
            
            // Width/Height も Model とバインドしてしまう
            canvas.SetBinding(WidthProperty, new Binding("TotalWidth") { Source = newItems });
            canvas.SetBinding(HeightProperty, new Binding("TotalHeight") { Source = newItems });
        }
    }
}

```

---

### 2. XAML が究極にスッキリする

MainWindow での呼び出しは、これだけになります。

```xml
<local:DiagramCanvas TargetItems="{Binding MyMainItems}" />

```

さらに、`ItemTemplates.xaml` の中の `Items`（グループ）の定義もこうなります：

```xml
<DataTemplate DataType="{x:Type local:Items}">
    <Border Background="{Binding Background}">
        <local:DiagramCanvas TargetItems="{Binding}" />
    </Border>
</DataTemplate>

```

---

### 3. なぜこの方法が「煩雑さ」を解消するのか？

* **バインドの一本化**: 「ItemsSource」「Width」「Height」を個別にバインドする手間が消え、`TargetItems` 一つを渡すだけで内部の配線が完了します。
* **再帰の美しさ**: トップレベルのキャンバスも、グループの中のキャンバスも、全く同じ `DiagramCanvas` クラスとして扱えるようになります。
* **ロジックの集約**: サイズ更新のトリガーなどを `Items` クラス側に寄せておけば、Canvas 側は「言われたサイズに伸び縮みするだけ」というシンプルな描画マシンに徹することができます。

---

### 💡 結論：この方向で進めるべき？

**はい、強くおすすめします。**
「データ（Items）」と「表示（DiagramCanvas）」が 1対1 で対応するこの形は、まさに MVVM とコンポーネント指向を融合させたプロの設計です。

これで基盤の「構造」についての悩みはすべて解消されたのではないでしょうか。

いよいよ満を持して、この洗練された `DiagramCanvas` に **「マウスで要素を掴んで X, Y を書き換える（ドラッグ）」** 機能を追加しませんか？ これができれば、ついに「動くアプリ」としての実感が湧いてくるはずです！


---


# DiagramCanvasクラスはItemsControlを継承しているクラスなのに、名前がCanvasになっているのが気になる。名前をDiagramItemsControlに変更するとか、もっと言うとItemsControlじゃなくてCanvasの継承に変更するとか、もっといい案はないかな？


---


設計センスが非常に鋭いです。WPFの標準クラス名との乖離、そして「Canvas（自由配置）」なのか「ItemsControl（データ駆動）」なのかというジレンマは、ライブラリ設計において非常に重要なポイントです。

結論から言うと、**「`ItemsControl` を継承したまま、名前を `DiagramEditor` または `DiagramBoard` にする」**、あるいは **「`ItemsControl` を継承したまま `Canvas` の性質を内部に取り込む」** のが良い落としどころです。

より洗練された3つの案を提案します。

---

### 案1：`DiagramBoard`（ダイアグラム・ボード）

`Canvas` という名前を捨て、**「ダイアグラムを描くための板（Board）」** という抽象的な名前にします。
`ItemsControl` は「データの集まりを表示する」という役割を指す名前なので、見た目（配置の自由度）を強調するなら `Board` や `Editor` がしっくりきます。

* **メリット**: `Canvas`（配置ルール）と `ItemsControl`（データ管理）の両方のニュアンスを包含できる。
* **呼び方**: `local:DiagramBoard`

---

### 案2：`ItemsCanvas`（アイテムス・キャンバス）

名前は `Canvas` ですが、中身は `ItemsControl` というハイブリッドな命名です。
「データ（Items）を表示するためのキャンバス」という意図が明確になります。

* **メリット**: 使う側が「あ、これは Canvas みたいに自由に置ける ItemsControl なんだな」と直感できる。

---

### 案3：`Canvas` を継承し、中身を自前で制御する

もし `ItemsControl` の継承を完全にやめて `Canvas` 継承に変えるなら、`ItemsSource` や `ItemTemplate` などの仕組みを**自前で再実装（DependencyPropertyを定義）**することになります。

* **デメリット**: WPFの強力な `DataTemplate` の仕組みを捨てる、あるいは再発明することになり、コードが激増します。
* **判断**: 今回のプロジェクトでは「データ（Model）に合わせて見た目を変える」という `DataTemplate` の恩恵が大きいため、**`ItemsControl` 継承は維持するのが賢明**です。

---

### 💡 結論：おすすめの構成

構造はそのままで、名前を **`DiagramBoard`** に変更し、内部に「Canvasとしての振る舞い」を隠蔽するのが最もプロフェッショナルな設計です。

```csharp
// DiagramCanvas 改め DiagramBoard
public class DiagramBoard : ItemsControl
{
    static DiagramBoard()
    {
        // デフォルトスタイルを適用（Themes/Generic.xaml で定義）
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DiagramBoard), 
            new FrameworkPropertyMetadata(typeof(DiagramBoard)));
    }

    // 前述の TargetItems プロパティ
    public Items? TargetItems
    {
        get => (Items)GetValue(TargetItemsProperty);
        set => SetValue(TargetItemsProperty, value);
    }
    
    // ... (TargetItemsProperty の定義)
}

```

そして、`Generic.xaml` での定義を以下のようにします。

```xml
<Style TargetType="{x:Type local:DiagramBoard}">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type local:DiagramBoard}">
                <ItemsPresenter />
            </ControlTemplate>
        </Setter.Value>
    </Setter>
    <Setter Property="ItemsPanel">
        <Setter.Value>
            <ItemsPanelTemplate>
                <Canvas IsItemsHost="True" Background="{TemplateBinding Background}" />
            </ItemsPanelTemplate>
        </Setter.Value>
    </Setter>
</Style>

```

### この構成が最強な理由

* **名前の違和感解消**: `DiagramBoard` なら、`ItemsControl` 継承でも `Canvas` 風の見た目でも違和感がありません。
* **機能の分離**: 「データの管理（ItemsControl）」と「配置のルール（Canvas）」が XAML のプロパティレベルで綺麗に分離されます。

名前を `DiagramBoard` に変更して、この **「中身は Canvas だけど実は ItemsControl」** という最終形態に整えてみますか？それとも別の名前のアイデアを検討しますか？


---


# 名前を DiagramBoard に変更して、この 「中身は Canvas だけど実は ItemsControl」 という最終形態に整えて


---


「中身は `Canvas` だけど、実体はデータ駆動の `ItemsControl`」という構成は、WPFにおけるカスタムエディタ作成の「最終回答」とも言える非常に強力なパターンです。

名前を **`DiagramBoard`** に統一し、構造を完全に整理しましょう。

---

### 1. Model: `Items` クラスの調整

まず、`DiagramBoard` が直接受け取るデータソースとしての `Items` クラスに、サイズ計算を集約させます。

```csharp
public partial class Items : Item
{
    public ObservableCollection<Item> Children { get; } = new();

    [ObservableProperty] private double _totalWidth;
    [ObservableProperty] private double _totalHeight;

    public void UpdateBounds()
    {
        // 前述の BoundsCalculator を使って計算
        var (w, h) = BoundsCalculator.GetTotalSize(Children);
        TotalWidth = w;
        TotalHeight = h;
    }

    // グループ自体の大きさ
    public override double Right => X + TotalWidth;
    public override double Bottom => Y + TotalHeight;
}

```

---

### 2. Control: `DiagramBoard.cs` の実装

`ItemsControl` を継承し、`TargetItems` プロパティを通じて `ItemsSource` と `Width/Height` を自動配線します。

```csharp
public class DiagramBoard : ItemsControl
{
    static DiagramBoard()
    {
        // 独自のスタイル（Themes/Generic.xaml）を適用
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DiagramBoard), 
            new FrameworkPropertyMetadata(typeof(DiagramBoard)));
    }

    public Items? TargetItems
    {
        get => (Items)GetValue(TargetItemsProperty);
        set => SetValue(TargetItemsProperty, value);
    }

    public static readonly DependencyProperty TargetItemsProperty =
        DependencyProperty.Register(nameof(TargetItems), typeof(Items), typeof(DiagramBoard), 
            new PropertyMetadata(null, OnTargetItemsChanged));

    private static void OnTargetItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var board = (DiagramBoard)d;
        if (e.NewValue is Items newItems)
        {
            // 1. データの紐付け
            board.ItemsSource = newItems.Children;
            
            // 2. サイズの紐付け（Modelの計算結果がそのままBoardのサイズになる）
            board.SetBinding(WidthProperty, new Binding("TotalWidth") { Source = newItems, Mode = BindingMode.OneWay });
            board.SetBinding(HeightProperty, new Binding("TotalHeight") { Source = newItems, Mode = BindingMode.OneWay });
        }
    }
}

```

---

### 3. Style: `Themes/Generic.xaml` の定義

ここで「中身は `Canvas` である」という魔法をかけます。`ItemsPanel` を `Canvas` に差し替えるのがポイントです。

```xml
<Style TargetType="{x:Type local:DiagramBoard}">
    <Setter Property="Background" Value="Transparent"/>
    
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type local:DiagramBoard}">
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
                <Canvas IsItemsHost="True" />
            </ItemsPanelTemplate>
        </Setter.Value>
    </Setter>

    <Setter Property="ItemContainerStyle">
        <Setter.Value>
            <Style TargetType="ContentPresenter">
                <Setter Property="Canvas.Left" Value="{Binding X}"/>
                <Setter Property="Canvas.Top" Value="{Binding Y}"/>
            </Style>
        </Setter.Value>
    </Setter>
</Style>

```

---

### 4. ItemTemplates.xaml の更新

再帰構造（グループの中にグループ）も、新しい `DiagramBoard` を使うように書き換えます。

```xml
<DataTemplate DataType="{x:Type local:Items}">
    <Border Background="{Binding Background}" BorderBrush="Silver" BorderThickness="1">
        <local:DiagramBoard TargetItems="{Binding}" />
    </Border>
</DataTemplate>

```

---

### これで何が変わったか？

1. **名前の納得感**: `DiagramBoard` という「場」を定義し、その中身を `Canvas` が支えるという、WPFの王道の構成になりました。
2. **管理の自動化**: `MainWindow` で `Items` オブジェクトを `TargetItems` にセットするだけで、**「ItemsSourceの設定」「幅の同期」「高さの同期」** がすべて一撃で完了します。
3. **座標バインドの集約**: `ItemContainerStyle` を `DiagramBoard` 内に定義したことで、個々の `DataTemplate` で `Canvas.Left` などを書く必要がなくなり、見た目の定義に集中できるようになりました。

これで、構造に関する設計は**「完全版」**です！

この美しい土台の上で、いよいよ **`DiagramBoard` に「マウスでドラッグして `X`, `Y` を変更する」** ロジックを追加しましょうか？（`DiagramBoard.cs` の `OnPreviewMouseDown` などを使って実装していきます）


---



