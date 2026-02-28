# Visual StudioとWPFとMVVMパターンを使って、文字列や図形を表示移動できるようなアプリを作りたい
Modelクラスを流し込んで要素を表示管理するクラスをCustomControlで作る場合に
継承するクラスは以下の3つから考えている、どのクラスがふさわしい？それとももっといい方法があれば解説して
* Control
* ItemsControl
* Canvas



バインドするData作成、この「rootData」を流し込んで表示したい
”        private void CreateData()
        {
            Datas rootData = new(0, 0);
            rootData.AddNodeData(new TextBlockData(0, 0, "AAA"));

            Datas group = new(100, 100);
            group.AddNodeData(new RectangleData(0, 0, 100, 50));
            group.AddNodeData(new RectangleData(50, 150, 100, 50));
            rootData.AddNodeData(group);

        }
”


Modelクラス群
”    public abstract partial class Data : ObservableObject
    {
        [ObservableProperty] private double _x;
        [ObservableProperty] private double _y;
        [ObservableProperty] private double _z;
        [ObservableProperty] private double _width;
        [ObservableProperty] private double _height;
        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private Rect _externalRect;

    }

    public partial class Datas : Data
    {
        public ObservableCollection<Data> Nodes { get; set; } = [];
        public Datas(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
        public void AddNodeData(Data node)
        {
            Nodes.Add(node);
        }
    }

    public partial class TextBlockData : Data
    {
        [ObservableProperty] private string _text = string.Empty;
        public TextBlockData(double x, double y, string text)
        {
            this.X = x;
            this.Y = y;
            this.Text = text;
        }
    }

    public partial class RectangleData : Data
    {
        public RectangleData(double x, double y, double width, double height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }
    }
”

DataTemplate
”      <DataTemplate DataType="{x:Type local:RectangleData}">
        <Rectangle Width="{Binding Width}" Height="{Binding Height}"
               Canvas.Left="{Binding X}" Canvas.Top="{Binding Y}"
               Fill="DodgerBlue"/>
      </DataTemplate>

      <DataTemplate DataType="{x:Type local:Datas}">
        <local:NodeContainer TargetNodes="{Binding Nodes}"/>
      </DataTemplate>
”


---


結論から申し上げますと、今回の目的（複数のModelを流し込んで図形を表示・管理する）に最もふさわしいのは **`ItemsControl`** です。

なぜ他の2つではなく `ItemsControl` なのか、そしてMVVMパターンで「移動」や「階層構造」を実現するための最適な実装方法を解説します。

---

## 1. なぜ ItemsControl が最適なのか？

検討されている3つのクラスを比較すると、役割の違いが明確になります。

| クラス | 採用の可否 | 理由 |
| --- | --- | --- |
| **Control** | × | 単一のコンテンツを表示するための最小単位です。複数のデータ（Nodes）を一覧表示する機能（ItemsSource）を自作しなければならず、車輪の再発明になります。 |
| **Canvas** | △ | **「パネル（レイアウト枠）」**であり、コントロールではありません。`Canvas` 自体はデータの束（Collection）を自動でUI要素に変換する機能を持っていません。 |
| **ItemsControl** | **◎** | **「データの集合をUIに変換する」**ための基底クラスです。`ItemsSource` に `Nodes` を渡すだけで、各データを `DataTemplate` に基づいて描画してくれます。 |

---

## 2. 実装のポイント：Canvasを「中」に使う

`ItemsControl` を継承しつつ、その「中身（Panel）」として `Canvas` を使うのがWPFの王道パターンです。

### CustomControl の定義

`ItemsControl` を継承した `NodeContainer` を作成します。

```csharp
public class NodeContainer : ItemsControl
{
    static NodeContainer()
    {
        // StyleをGeneric.xamlから読み込むための設定
        DefaultStyleKeyProperty.OverrideMetadata(typeof(NodeContainer), 
            new FrameworkPropertyMetadata(typeof(NodeContainer)));
    }
}

```

### XAML (Style) の定義

ここが重要です。`ItemsControl` の中身が `Canvas` になるように定義し、さらに **「各アイテムを Canvas のどこに配置するか」** を指定します。

```xml
<Style TargetType="{x:Type local:NodeContainer}">
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
                <Setter Property="Canvas.Left" Value="{Binding X}" />
                <Setter Property="Canvas.Top" Value="{Binding Y}" />
                <Setter Property="Panel.ZIndex" Value="{Binding Z}" />
            </Style>
        </Setter.Value>
    </Setter>
</Style>

```

---

## 3. 階層構造（Datas inside Datas）の扱い

ユーザー様が提示された `DataTemplate` の設計は非常に優れています。`Datas` クラスが `Nodes` を持ち、その `DataTemplate` の中で再び `NodeContainer` を呼ぶことで、再帰的に階層構造が表示されます。

ただし、1点だけ修正が必要です。`Rectangle` などのテンプレート側で `Canvas.Left` を書くのではなく、上述の `ItemContainerStyle` で座標を指定してください。

### 修正した DataTemplate

```xml
<DataTemplate DataType="{x:Type local:RectangleData}">
    <Rectangle Width="{Binding Width}" Height="{Binding Height}" Fill="DodgerBlue"/>
</DataTemplate>

<DataTemplate DataType="{x:Type local:TextBlockData}">
    <TextBlock Text="{Binding Text}" Background="Transparent"/>
</DataTemplate>

<DataTemplate DataType="{x:Type local:Datas}">
    <local:NodeContainer ItemsSource="{Binding Nodes}" />
</DataTemplate>

```

---

## 4. アドバイス：さらに「移動」をさせたい場合

もし今後、「マウスで図形をドラッグして移動させたい」という機能を追加する場合、`ItemsControl` よりも **`ListBox`** を継承することをお勧めします。

* **理由:** `ListBox` は `ItemsControl` の子孫であり、「選択（SelectedItem）」の概念があります。「クリックした図形を選択状態にして、マウス移動に合わせて `X`, `Y` プロパティを更新する」という処理が書きやすくなります。

### まとめ

* **継承元:** `ItemsControl` （選択機能が欲しければ `ListBox`）
* **レイアウト:** `ItemsPanel` を `Canvas` に差し替える
* **座標指定:** `ItemContainerStyle` で `Canvas.Left/Top` を `X/Y` にバインドする

次は、マウスで図形をドラッグ移動させるためのロジック（BehaviorやEvent処理）の実装方法について詳しく説明しましょうか？


---


# 選択機能が欲しいのでListBoxが良さそう。ListBoxとListVewの比較だとどちらが良い？


---


「選択機能」を重視されるのであれば、**`ListBox` 一択**です。

`ListView` も選択機能を備えていますが、この2つの違いを理解すると、なぜ図形描画アプリにおいて `ListBox` が選ばれるのかが明確になります。

---

## 1. ListBox vs ListView 比較

| 特徴 | **ListBox** | **ListView** |
| --- | --- | --- |
| **主な用途** | 汎用的な項目のリスト表示。項目のスタイルを自由に変えたい時。 | 表形式（グリッド）や複数の「表示モード」を切り替えたい時。 |
| **階層** | `ItemsControl` の直系の子。 | `ListBox` の派生クラス。 |
| **追加機能** | 基本的な選択機能のみ。 | `View` プロパティを持ち、`GridView`（列ヘッダーなど）をサポート。 |
| **今回の用途** | **最適。** キャンバス上の図形管理に「列」や「ヘッダー」は不要。 | **過剰（Overkill）。** 表形式にするための内部処理が不要な負荷になる。 |

### なぜ ListBox なのか？

`ListView` は「エクスプローラーの詳細表示」のように、データを**表（テーブル）**として見せることに特化しています。一方、図形描画アプリは「キャンバス上の自由配置」ですので、`ListView` が持つグリッド表示の仕組みは一切使いません。
`ListBox` をベースにすることで、余計な機能を削ぎ落とした、軽量でカスタマイズしやすい基盤が手に入ります。

---

## 2. ListBox を使う際の実装のコツ（MVVM）

`ListBox` を継承して CustomControl を作る場合、**「選択状態」と「座標バインド」**をどこに書くかがポイントになります。

### ItemContainerStyle の設定

`ListBox` は、各アイテムを `ListBoxItem` という「入れ物（コンテナー）」で包みます。このコンテナーに対して、座標と選択状態をバインドします。

```xml
<Style TargetType="{x:Type local:NodeContainer}"> <Setter Property="ItemsPanel">
        <Setter.Value>
            <ItemsPanelTemplate>
                <Canvas IsItemsHost="True" />
            </ItemsPanelTemplate>
        </Setter.Value>
    </Setter>
    <Setter Property="ItemContainerStyle">
        <Setter.Value>
            <Style TargetType="ListBoxItem">
                <Setter Property="Canvas.Left" Value="{Binding X}" />
                <Setter Property="Canvas.Top" Value="{Binding Y}" />
                
                <Setter Property="IsSelected" Value="{Binding IsSelected}" />

                <Setter Property="Template">
                    <Setter.Value>
                        <ControlTemplate TargetType="ListBoxItem">
                            <ContentPresenter />
                            </ControlTemplate>
                    </Setter.Value>
                </Setter>
            </Style>
        </Setter.Value>
    </Setter>
</Style>

```

---

## 3. 今後の拡張アドバイス：Model に必要なもの

`ListBox` を使うなら、抽象クラスである `Data` クラスに **`IsSelected`** プロパティ（`bool`）を追加しておくと非常に便利です。

* **メリット1:** コードビハインド（ViewModel）から「この図形を選択状態にする」という操作ができるようになります。
* **メリット2:** `DataTemplate` 内で、選択されている時だけ「リサイズ用のハンドル（点）」を表示する、といったトリガーが書けるようになります。

### まとめ

* **継承元:** `ListBox` がベスト。
* **理由:** シンプルで選択機能を持っており、Canvasとの相性が良いため。
* **次の一手:** `ListBoxItem` のデフォルトの「青い背景色」を無効化して、図形が自然に見えるようにテンプレートを調整する。

次は、マウスで図形をドラッグした時に、Modelの `X`, `Y` を更新して図形を動かす「ドラッグ移動」の実装方法について解説しましょうか？


---


# NodeContainerクラス全体をCustomControlとして書いて


---


`ListBox` を継承した `NodeContainer` クラスの実装例をまとめました。

カスタムコントロールとして作成する場合、**「C#コード（ロジック）」**と**「Generic.xaml（見た目・スタイル）」**の2つのファイルが必要になります。

---

## 1. C# クラスの実装 (`NodeContainer.cs`)

`ListBox` を継承し、WPFのカスタムコントロールの作法に従って `DefaultStyleKey` を上書きします。

```csharp
using System.Windows;
using System.Windows.Controls;

namespace YourProjectName.Controls
{
    public class NodeContainer : ListBox
    {
        static NodeContainer()
        {
            // このコントロール専用のスタイル(Generic.xaml)を適用するための宣言
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NodeContainer), 
                new FrameworkPropertyMetadata(typeof(NodeContainer)));
        }

        // 今後、ドラッグ＆ドロップ移動のロジックなどをここに追記していくことができます
    }
}

```

---

## 2. スタイルの実装 (`Themes/Generic.xaml`)

ここが最も重要な「キャンバス化」の設定です。`ListBox` の標準的な「縦並びリスト」の挙動を、自由配置できる「キャンバス」に作り変えます。

```xml
<ResourceDictionary
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:local="clr-namespace:YourProjectName.Controls">

    <Style TargetType="{x:Type local:NodeContainer}">
        <Setter Property="Background" Value="Transparent"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Padding" Value="0"/>
        
        <Setter Property="ItemsPanel">
            <Setter.Value>
                <ItemsPanelTemplate>
                    <Canvas IsItemsHost="True" Background="Transparent"/>
                </ItemsPanelTemplate>
            </Setter.Value>
        </Setter>

        <Setter Property="ItemContainerStyle">
            <Setter.Value>
                <Style TargetType="ListBoxItem">
                    <Setter Property="Canvas.Left" Value="{Binding X}" />
                    <Setter Property="Canvas.Top" Value="{Binding Y}" />
                    <Setter Property="Panel.ZIndex" Value="{Binding Z}" />
                    
                    <Setter Property="IsSelected" Value="{Binding IsSelected}" />

                    <Setter Property="Template">
                        <Setter.Value>
                            <ControlTemplate TargetType="ListBoxItem">
                                <ContentPresenter />
                                </ControlTemplate>
                        </Setter.Value>
                    </Setter>
                </Style>
            </Setter.Value>
        </Setter>

        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="{x:Type local:NodeContainer}">
                    <Border Background="{TemplateBinding Background}"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}">
                        <ItemsPresenter />
                    </Border>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
</ResourceDictionary>

```

---

## 3. DataTemplate での再帰呼び出し

Modelの `Datas`（グループ）の中にさらに `NodeContainer` を配置することで、親子関係を表現します。

```xml
<DataTemplate DataType="{x:Type local:Datas}">
    <local:NodeContainer ItemsSource="{Binding Nodes}" />
</DataTemplate>

```

---

## 💡 この実装のポイント

1. **座標バインドの集約:** `Rectangle` や `TextBlock` 側で `Canvas.Left` を書く必要がなくなります。すべて `ItemContainerStyle` が一括で座標を管理します。
2. **選択の透明化:** デフォルトの `ListBox` は選択すると背景が青くなりますが、`ControlTemplate` を上書きしたことで、図形そのものだけが見えるようになります。
3. **再帰構造:** `Datas` クラスが `Nodes` を持ち、それを `NodeContainer` が表示する...という流れが連鎖するため、どれだけ深い階層のデータでも自動で描画されます。

### 注意点

Modelクラス（`Data` クラス）に **`bool IsSelected`** プロパティを追加し、`ObservableProperty` 属性をつけておいてください。これがないと、画面上でクリックしても Model 側に「選択された」という情報が伝わりません。

次は、マウスで図形を掴んで動かせるようにするための、**「ドラッグ移動のイベント処理」**を `NodeContainer` に追加する方法を解説しましょうか？


---


# ModelのDatasをMainWindow.xaml.csで作成して、それをNodeContainerにバインドする処理はどのように書けば良い？


---


`MainWindow.xaml.cs` でデータを生成し、それを XAML 上の `NodeContainer` に結びつける（バインドする）方法を解説します。

MVVMパターンの基本である **「DataContext（データコンテキスト）」** を設定するのがポイントです。

---

### 1. MainWindow.xaml.cs の実装

まず、作成した `rootData` を外部から参照できるようにプロパティとして保持し、自分自身（MainWindow）を `DataContext` に設定します。

```csharp
using System.Windows;

namespace YourProjectName
{
    public partial class MainWindow : Window
    {
        // 画面にバインドするためのプロパティ
        // （変更通知が必要な場合はObservablePropertyにしますが、
        // 　初期化時のみであればこのままでもOKです）
        public Datas RootData { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            // データの作成
            CreateData();

            // このWindowのデータソースとして自分自身を指定する
            // これにより XAML から "RootData" という名前でアクセスできるようになります
            this.DataContext = this;
        }

        private void CreateData()
        {
            // インスタンス化
            RootData = new Datas(0, 0);
            
            // 要素の追加
            RootData.AddNodeData(new TextBlockData(10, 10, "AAA"));

            Datas group = new Datas(100, 100);
            group.AddNodeData(new RectangleData(0, 0, 100, 50));
            group.AddNodeData(new RectangleData(50, 150, 100, 50));
            
            RootData.AddNodeData(group);
        }
    }
}

```

---

### 2. MainWindow.xaml の実装

XAML 側で `NodeContainer` を配置し、`ItemsSource` プロパティを `RootData` の中にある `Nodes` コレクションにバインドします。

```xml
<Window x:Class="YourProjectName.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:YourProjectName"
        xmlns:controls="clr-namespace:YourProjectName.Controls"
        Title="MainWindow" Height="450" Width="800">
    
    <Window.Resources>
        <DataTemplate DataType="{x:Type local:RectangleData}">
            <Rectangle Width="{Binding Width}" Height="{Binding Height}" Fill="DodgerBlue"/>
        </DataTemplate>

        <DataTemplate DataType="{x:Type local:TextBlockData}">
            <TextBlock Text="{Binding Text}" Background="Transparent"/>
        </DataTemplate>

        <DataTemplate DataType="{x:Type local:Datas}">
            <controls:NodeContainer ItemsSource="{Binding Nodes}" />
        </DataTemplate>
    </Window.Resources>

    <Grid>
        <controls:NodeContainer ItemsSource="{Binding RootData.Nodes}" />
    </Grid>
</Window>

```

---

### 3. ここまでの動作の流れ

1. **起動時**: `MainWindow` のコンストラクタが走り、`RootData` という変数に図形やテキストのデータ（Model）が詰まります。
2. **バインド**: `this.DataContext = this` により、XAMLは `RootData` プロパティを見つけられるようになります。
3. **表示**: `NodeContainer` の `ItemsSource` に `Nodes`（リスト）が渡されます。
4. **描画**:
* `Nodes` の中に `RectangleData` があれば、`Rectangle` 用の `DataTemplate` が呼ばれます。
* `Nodes` の中に `Datas` (グループ) があれば、`Datas` 用の `DataTemplate` が呼ばれ、その中で**また新しい `NodeContainer` が生成**されます。



### 注意点：名前空間

`NodeContainer` クラスを `Controls` というフォルダ（名前空間）に入れている場合は、XAMLの上部で `xmlns:controls="clr-namespace:YourProjectName.Controls"` のように定義して、`controls:NodeContainer` として呼び出す必要があります。

これで、`CreateData` メソッドで作成したツリー構造がそのまま画面に反映されるはずです！


---


# 表示された！次はDataTemplateを書く場所、今はMainWindow.xamlに書いているけど、Themesフォルダにリソースディクショナリを「NodeTemplate.xaml」として追加して、そこに書いてみた

NodeTemplate.xaml
```XAML
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:local="clr-namespace:_20260227">

  <DataTemplate DataType="{x:Type local:RectangleData}">
    <Rectangle Width="{Binding Width}"
                 Height="{Binding Height}"
                 Fill="DodgerBlue"/>
  </DataTemplate>

  <DataTemplate DataType="{x:Type local:Datas}">
    <local:NodeContainer ItemsSource="{Binding Nodes}"/>
  </DataTemplate>

  <DataTemplate DataType="{x:Type local:TextBlockData}">
    <TextBlock Text="{Binding Text}"
                 Background="Transparent"/>
  </DataTemplate>


</ResourceDictionary>
```

---

App.xaml
```XAML
<Application x:Class="_20260227.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:_20260227"
             StartupUri="MainWindow.xaml">
  <Application.Resources>
    <ResourceDictionary>
      <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="/Themes/NodeTemplate.xaml"/>
      </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
  </Application.Resources>
</Application>
```
こうしてみたけど、なにか問題や、もっといい方法があれば指摘して


---


表示に成功したとのこと、素晴らしいですね！実装の方向性としては**完璧に「正解」**です。

`App.xaml` で `MergedDictionaries` を使って一括管理する方法は、アプリ全体でデータと見た目の紐付け（Implicit DataTemplate）を一貫させるためのWPFの王道パターンです。

現状でも全く問題ありませんが、さらにプロっぽく、保守性を高めるためのアドバイスをいくつかお伝えしますね。

---

## 1. パックURI形式での記述（推奨）

現在の `Source="/Themes/NodeTemplate.xaml"` という書き方は、同じプロジェクト内であれば動きますが、将来的にプロジェクト名が変わったり、クラスライブラリに切り出したりした際にエラーになることがあります。より堅牢な **パックURI（Pack URI）** 形式で書く癖をつけておくと安心です。

```xml
<ResourceDictionary Source="pack://application:,,,/YourProjectName;component/Themes/NodeTemplate.xaml"/>

```

※ `YourProjectName` は実際のプロジェクト名（アセンブリ名）に置き換えてください。

---

## 2. Generic.xaml との関係性

現在、`NodeContainer` は CustomControl として作られていますよね？CustomControl には「デフォルトの見た目」を定義する `Themes/Generic.xaml` という特別なファイルが存在します。

* **NodeTemplate.xaml**: 「データ（Model）」をどう見せるか
* **Generic.xaml**: 「コントロール（NodeContainer）」自体の枠組みをどうするか

この2つを分けて管理している今の構成は非常に綺麗です。もし `NodeContainer` 以外でこれらのデータを使わないのであれば、`Generic.xaml` の中で `NodeTemplate.xaml` をマージして、リソースを局所化する方法もあります。

---

## 3. グループ要素の「再帰」に関するワンポイント

現在の `Datas` 用テンプレートの中で `NodeContainer` を呼んでいますが、一つ注意点があります。

```xml
<DataTemplate DataType="{x:Type local:Datas}">
    <local:NodeContainer ItemsSource="{Binding Nodes}"/>
</DataTemplate>

```

グループ化された `Datas` が選択されたとき、その「中身」だけが表示されるのか、それとも「グループ全体の枠（バウンディングボックス）」を表示したいのかによって、少し工夫が必要になります。

もし、**グループ全体を一つの塊としてドラッグ移動させたい**場合、このテンプレート内の `NodeContainer` を `Canvas` 等で包み、その `Canvas` 自体を親の `NodeContainer`（ListBoxItem）が管理するようにすると、階層構造がより扱いやすくなります。

---

## 4. 実行時の「リソース探し」を効率化する

`App.xaml` に書いたテンプレートは「アプリケーション全体」から参照可能になります。もし、図形描画以外の画面（設定画面など）が増えた場合、そこでもこれらのテンプレートが常に検索対象になります。

もし、この描画ルールが `MainWindow` でしか使われないのであれば、`MainWindow.Resources` 内でマージしても良いでしょう。

```xml
<Window.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="/Themes/NodeTemplate.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Window.Resources>

```

---

### 次のステップへのヒント

次は、いよいよ**「図形をマウスで動かす（ドラッグ＆ドロップ）」**に挑戦したいところですね！

現状、Model（`Data`）に `X` と `Y` があるので、あとは `NodeContainer` (ListBox) 側でマウスの移動量を計算して、Model の `X/Y` を書き換えるだけです。

「ドラッグ移動のロジック」を `NodeContainer.cs` に書き込む準備はできていますか？必要であれば、その実装コードを提示できます！


---


# 「3. グループ要素の「再帰」に関するワンポイント」
ここでの
「グループ全体を一つの塊としてドラッグ移動させたい場合、このテンプレート内の NodeContainer を Canvas 等で包み、その Canvas 自体を親の NodeContainer（ListBoxItem）が管理するようにすると、階層構造がより扱いやすくなります。」
これを詳しく解説して


---


この「再帰的な階層構造」と「ドラッグ移動」を組み合わせる際、最も混乱しやすいのが**「座標の基準点（座標系）」**と**「ヒットテスト（どこを触ったら動くか）」**の関係です。

詳しく解説します。

---

## 1. 座標の考え方：相対座標の連鎖

WPFの `Canvas` 内に配置された要素は、その親の左上を `(0, 0)` とした相対座標で動きます。

* **ルートの NodeContainer**: 画面（Window）に対して `Datas.X, Y` に配置。
* **グループ（Datas）内部**: そのグループの左上を `(0, 0)` とした座標系が新しく始まります。

もし `Datas` クラスの `X, Y` を変更すると、その中にある `Nodes`（子要素）は、**自分たちの座標値を変えることなく、グループごと一斉に移動します。** これが `ItemsControl`（ListBox）を入れ子にする最大のメリットです。

---

## 2. なぜ DataTemplate 内に Canvas（または親要素）が必要か

`ListBox`（NodeContainer）は、標準では「中身の要素のサイズ」に合わせて自分の大きさを決めようとします。しかし、`Canvas` は子要素のサイズを親に伝えない性質があるため、そのままでは **「グループのクリックできる範囲が 0×0 になってしまう」** 危険があります。

これを防ぐために、`DataTemplate` でグループの「器」を定義します。

### 実装例：グループを「掴める」ようにする

```xml
<DataTemplate DataType="{x:Type local:Datas}">
    <Grid Background="Transparent">
        <local:NodeContainer ItemsSource="{Binding Nodes}" 
                             Width="{Binding ExternalRect.Width}" 
                             Height="{Binding ExternalRect.Height}" 
                             IsHitTestVisible="True" />
    </Grid>
</DataTemplate>

```

---

## 3. 「階層構造が扱いやすくなる」具体的なメリット

この構造にすると、以下の2つの移動が完全に分離できます。

1. **子要素の移動**:
子要素（Rectangleなど）をドラッグすると、その子の `X, Y` が更新されます。これはグループ内での相対的な位置が変わるだけです。
2. **グループ全体の移動**:
グループ自体をドラッグすると、`Datas` クラスの `X, Y` が更新されます。中の子要素の `X, Y` は一切変える必要がありません。

### ここで「ExternalRect」が活きてくる！

ユーザー様が Model に持たせている `_externalRect`（外接矩形）は非常に重要です。
グループ内の全要素を囲む範囲を計算して `ExternalRect` に入れておけば、`DataTemplate` 側でそのサイズを指定することで、**「グループの枠内ならどこを掴んでも移動できる」** という直感的な操作感が作れます。

---

## 4. 注意点：イベントの「横取り」

`ListBox` を入れ子にすると、マウスイベントが「中の ListBox」で止まってしまい、「外の ListBox」に伝わらない（＝グループが動かせない）という問題が起きることがあります。

これを解決するプロフェッショナルな方法は2つです：

* **方法A：透過設定**
中の `NodeContainer` の `Background` を `null` (未設定) にし、`IsHitTestVisible` を調整する。
* **方法B：プレビューイベントの使用**
`PreviewMouseDown` などのトンネルイベントを使って、親側で先にマウス操作を検知する。

---

## まとめ：どう書くのがベスト？

今の `NodeTemplate.xaml` を少しだけ強化するなら、こんなイメージです：

```xml
<DataTemplate DataType="{x:Type local:Datas}">
    <Canvas Background="Transparent" 
            Width="{Binding ExternalRect.Width}" 
            Height="{Binding ExternalRect.Height}">
        <local:NodeContainer ItemsSource="{Binding Nodes}" />
    </Canvas>
</DataTemplate>

```

> [!TIP]
> **「グループ化」の醍醐味**
> この「再帰」が完成すると、`group1` の中に `group2` を入れ、その中に `Rectangle` を入れる……といった「マトリョーシカ状態」になっても、一番親の `group1` を動かすだけで、中身がすべて綺麗についてくるようになります。

次は、実際にマウスでクリックして `X, Y` を書き換える **「DragBehavior」** 的なロジックを C# で書いてみますか？それとも、この XAML の構造をもっと深掘りしますか？


---


# ExternalRectプロパティは要素の回転(変形)表示後のRectに使う予定だったから、別のプロパティを用意したほうが良い？プロパティ名も含めて、もっといい方法があれば指摘して


---


結論から言うと、**「用途が違うので、別のプロパティに分ける」のが大正解**です。

`ExternalRect` を「変形（回転・拡大縮小）を考慮した最終的な見た目の範囲」として予約しておくのであれば、グループとしての「中身の広さ」を管理するプロパティを別に持つことで、計算ロジックがシンプルになり、バグを防げます。

グラフィック系アプリの設計でよく使われるプロパティ構成を提案します。

---

## 1. 推奨されるプロパティ構成

`Datas` クラス（グループ）には、以下の2つの概念を持たせると非常に扱いやすくなります。

| プロパティ名 | 役割 | 意味 |
| --- | --- | --- |
| **ContentBounds** | **内部の領域** | 子要素（Nodes）の $X, Y$ 座標の最小・最大から計算される「中身の詰まっている範囲」。 |
| **ExternalRect** | **外部の領域** | グループ自体の回転やスケールを適用した後の、親の Canvas から見た「占有範囲」。 |

### なぜ分けるのか？

* **計算のタイミングが違う:** `ContentBounds` は「子が動いた時」に更新されますが、`ExternalRect` は「グループ自身が回転・変形した時」に更新されます。
* **ヒットテスト（当たり判定）:** グループを選択する際、`ContentBounds` を使って「中身がある場所ならどこでも掴める」ようにしたり、逆に「外枠（ExternalRect）を掴んでリサイズする」といった使い分けができます。

---

## 2. もっといい方法：計算の自動化

MVVM でこれらを管理する場合、プロパティを単に持つだけでなく、**「子が動いたら親の領域を再計算する」**仕組みが必要です。

### 実装のヒント（C#）

`Datas` クラスに、子要素の変更を監視して `ContentBounds` を更新するメソッドを追加します。

```csharp
public partial class Datas : Data
{
    [ObservableProperty] private Rect _contentBounds;

    // 子要素が動くたびにこれを呼ぶ
    public void UpdateBounds()
    {
        if (Nodes.Count == 0) { ContentBounds = Rect.Empty; return; }

        double minX = Nodes.Min(n => n.X);
        double minY = Nodes.Min(n => n.Y);
        double maxX = Nodes.Max(n => n.X + n.Width);
        double maxY = Nodes.Max(n => n.Y + n.Height);

        ContentBounds = new Rect(minX, minY, maxX - minX, maxY - minY);
        
        // ここで ExternalRect も再計算する（回転などを考慮して）
        // UpdateExternalRect(); 
    }
}

```

---

## 3. DataTemplate での活用例

この `ContentBounds` を使って、グループを「掴みやすく」します。

```xml
<DataTemplate DataType="{x:Type local:Datas}">
    <Grid>
        <Canvas Width="{Binding ContentBounds.Width}" 
                Height="{Binding ContentBounds.Height}">
            <Canvas.RenderTransform>
                <TranslateTransform X="{Binding ContentBounds.X}" Y="{Binding ContentBounds.Y}"/>
            </Canvas.RenderTransform>
            
            <local:NodeContainer ItemsSource="{Binding Nodes}" />
        </Canvas>

        <Border BorderBrush="LightBlue" BorderThickness="1" 
                Visibility="{Binding IsSelected, Converter={StaticResource BooleanToVisibilityConverter}}"
                Width="{Binding ContentBounds.Width}"
                Height="{Binding ContentBounds.Height}" />
    </Grid>
</DataTemplate>

```

---

## 4. プロパティ名のアイデア（一般用語）

もし `ContentBounds` 以外でしっくりくる名前を探しているなら、これらも一般的です：

* **`LocalBounds`**: 自身の座標系での範囲。
* **`VisualBounds`**: 見た目上の範囲。
* **`BoundingBox`**: 3DやCADでよく使われる、最小包含矩形。

### アドバイス

回転（変形）を考慮する場合、WPFの `VisualTreeHelper.GetDescendantBounds()` などを利用して、View側からModelへサイズを書き戻す（逆流させる）手法もありますが、基本的には **Model内で数学的に計算（ジオメトリ計算）する** 方が、単体テストもしやすく、MVVMとして純粋で堅牢な作りになります。

次は、子要素が動いたときに親の `ContentBounds` をどうやって効率よく更新するか（イベントの購読など）について、深掘りしましょうか？それとも「移動ロジック」へ進みますか？結論から言うと、**「用途が違うので、別のプロパティに分ける」のが大正解**です。

`ExternalRect` を「変形（回転・拡大縮小）を考慮した最終的な見た目の範囲」として予約しておくのであれば、グループとしての「中身の広さ」を管理するプロパティを別に持つことで、計算ロジックがシンプルになり、バグを防げます。

グラフィック系アプリの設計でよく使われるプロパティ構成を提案します。

---

## 1. 推奨されるプロパティ構成

`Datas` クラス（グループ）には、以下の2つの概念を持たせると非常に扱いやすくなります。

| プロパティ名 | 役割 | 意味 |
| --- | --- | --- |
| **ContentBounds** | **内部の領域** | 子要素（Nodes）の $X, Y$ 座標の最小・最大から計算される「中身の詰まっている範囲」。 |
| **ExternalRect** | **外部の領域** | グループ自体の回転やスケールを適用した後の、親の Canvas から見た「占有範囲」。 |

### なぜ分けるのか？

* **計算のタイミングが違う:** `ContentBounds` は「子が動いた時」に更新されますが、`ExternalRect` は「グループ自身が回転・変形した時」に更新されます。
* **ヒットテスト（当たり判定）:** グループを選択する際、`ContentBounds` を使って「中身がある場所ならどこでも掴める」ようにしたり、逆に「外枠（ExternalRect）を掴んでリサイズする」といった使い分けができます。

---

## 2. もっといい方法：計算の自動化

MVVM でこれらを管理する場合、プロパティを単に持つだけでなく、**「子が動いたら親の領域を再計算する」**仕組みが必要です。

### 実装のヒント（C#）

`Datas` クラスに、子要素の変更を監視して `ContentBounds` を更新するメソッドを追加します。

```csharp
public partial class Datas : Data
{
    [ObservableProperty] private Rect _contentBounds;

    // 子要素が動くたびにこれを呼ぶ
    public void UpdateBounds()
    {
        if (Nodes.Count == 0) { ContentBounds = Rect.Empty; return; }

        double minX = Nodes.Min(n => n.X);
        double minY = Nodes.Min(n => n.Y);
        double maxX = Nodes.Max(n => n.X + n.Width);
        double maxY = Nodes.Max(n => n.Y + n.Height);

        ContentBounds = new Rect(minX, minY, maxX - minX, maxY - minY);
        
        // ここで ExternalRect も再計算する（回転などを考慮して）
        // UpdateExternalRect(); 
    }
}

```

---

## 3. DataTemplate での活用例

この `ContentBounds` を使って、グループを「掴みやすく」します。

```xml
<DataTemplate DataType="{x:Type local:Datas}">
    <Grid>
        <Canvas Width="{Binding ContentBounds.Width}" 
                Height="{Binding ContentBounds.Height}">
            <Canvas.RenderTransform>
                <TranslateTransform X="{Binding ContentBounds.X}" Y="{Binding ContentBounds.Y}"/>
            </Canvas.RenderTransform>
            
            <local:NodeContainer ItemsSource="{Binding Nodes}" />
        </Canvas>

        <Border BorderBrush="LightBlue" BorderThickness="1" 
                Visibility="{Binding IsSelected, Converter={StaticResource BooleanToVisibilityConverter}}"
                Width="{Binding ContentBounds.Width}"
                Height="{Binding ContentBounds.Height}" />
    </Grid>
</DataTemplate>

```

---

## 4. プロパティ名のアイデア（一般用語）

もし `ContentBounds` 以外でしっくりくる名前を探しているなら、これらも一般的です：

* **`LocalBounds`**: 自身の座標系での範囲。
* **`VisualBounds`**: 見た目上の範囲。
* **`BoundingBox`**: 3DやCADでよく使われる、最小包含矩形。

### アドバイス

回転（変形）を考慮する場合、WPFの `VisualTreeHelper.GetDescendantBounds()` などを利用して、View側からModelへサイズを書き戻す（逆流させる）手法もありますが、基本的には **Model内で数学的に計算（ジオメトリ計算）する** 方が、単体テストもしやすく、MVVMとして純粋で堅牢な作りになります。

次は、子要素が動いたときに親の `ContentBounds` をどうやって効率よく更新するか（イベントの購読など）について、深掘りしましょうか？それとも「移動ロジック」へ進みますか？


---


# 「移動ロジック」を解説して


---


「移動ロジック」の実装ですね。WPFのMVVMパターンにおいて、ドラッグ移動は「UIイベント（マウスの動き）」を「Modelのプロパティ（X, Y）の更新」に変換する作業です。

`ListBox`（NodeContainer）を継承している利点を最大限に活かした、**「複数選択にも対応したドラッグ移動」**の実装方法を解説します。

---

### 1. ロジックの置き場所

カスタムコントロール（`NodeContainer`）の中にマウスイベントのオーバーライドとして記述します。これにより、ViewModelを汚さずに「図形を動かす」というUI特有の振る舞いをカプセル化できます。

### 2. NodeContainer.cs の実装例

以下を `NodeContainer.cs` に追記します。

```csharp
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

public class NodeContainer : ListBox
{
    private Point _startPoint; // ドラッグ開始時のマウス位置
    private bool _isDragging;  // ドラッグ中かどうか

    static NodeContainer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(NodeContainer), 
            new FrameworkPropertyMetadata(typeof(NodeContainer)));
    }

    // マウスが押された時
    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseDown(e);

        // クリックされた要素が ListBoxItem か確認
        var item = ContainerFromElement((DependencyObject)e.OriginalSource) as ListBoxItem;
        if (item == null) return;

        _isDragging = true;
        _startPoint = e.GetPosition(this); // NodeContainer（Canvas）上の座標を取得
        
        // マウスをキャプチャして、枠外に出てもイベントを追えるようにする
        item.CaptureMouse();
    }

    // マウスが動いた時
    protected override void OnPreviewMouseMove(MouseEventArgs e)
    {
        base.OnPreviewMouseMove(e);

        if (!_isDragging) return;

        Point currentPoint = e.GetPosition(this);
        Vector delta = currentPoint - _startPoint; // 移動量を計算

        if (delta.Length > 0)
        {
            // 選択されているすべてのアイテム（Model）を移動させる
            // これにより、単一移動も複数移動も同じロジックで動く
            foreach (var selectedItem in SelectedItems.Cast<Data>())
            {
                selectedItem.X += delta.X;
                selectedItem.Y += delta.Y;
            }

            _startPoint = currentPoint; // 基準点を更新
        }
    }

    // マウスが離された時
    protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseUp(e);

        if (_isDragging)
        {
            var item = ContainerFromElement((DependencyObject)e.OriginalSource) as ListBoxItem;
            item?.ReleaseMouseCapture();
            _isDragging = false;
        }
    }
}

```

---

### 3. この実装のすごいポイント

1. **複数選択対応:** `SelectedItems` をループ回しているので、Ctrlキーなどで複数選んだ状態で一つを動かすと、選んでいるすべての図形が一緒に動きます。
2. **階層構造への対応:** * `Datas`（グループ）を選択して動かすと、その `Datas.X/Y` が更新されます。
* 中の `Rectangle` などは動かしていない（相対座標が変わらない）ため、グループ全体が綺麗に付いてきます。


3. **座標系の不整合を防止:** `e.GetPosition(this)` を使うことで、スクロールやウィンドウサイズに関係なく、常に `NodeContainer` 内の座標系で移動量を計算できます。

---

### 4. 快適に動かすための追加設定

`ListBoxItem` のクリック判定を安定させるため、`ItemContainerStyle`（Generic.xamlの中）に以下の設定を追加することをお勧めします。

```xml
<Style TargetType="ListBoxItem">
    <Setter Property="Background" Value="Transparent" />
    
    <Setter Property="Canvas.Left" Value="{Binding X}" />
    <Setter Property="Canvas.Top" Value="{Binding Y}" />
</Style>

```

### 5. 次に考えるべきこと（スナップ機能など）

移動ロジックができると、次に欲しくなるのは以下のような機能です。

* **グリッドスナップ:** `selectedItem.X += Math.Round(delta.X / 10) * 10;` のようにして、10px単位でカチカチ動かす。
* **境界チェック:** Canvasの端から外に出ないように制限する。
* **Undo/Redo:** 移動が終わったタイミングで、移動前の座標と移動後の座標を記録する。

まずはこのコードで、図形がマウスでスイスイ動く感動を味わってみてください！何かうまく動かない部分はありますか？


---


# 移動はできるけど、移動中にマウスカーソルが別の要素に当たる(通過しようとする)と、移動対象が移ってしまう


---


それはWPFの `ListBox` が持つ「標準の選択ロジック」と、自作した「移動ロジック」が衝突している状態ですね。

マウスが別の要素の上を通過するとき、`ListBox` が「お、新しい項目の上でマウスが動いたな？そっちを選択候補にするか」と余計な気を利かせてしまうのが原因です。

これを解決するには、**「ドラッグ中はイベントを他へ渡さない（ハンドリング済みとする）」** という処理と、**「マウスキャプチャの対象」** を見直す必要があります。

---

### 解決策：`e.Handled = true` と キャプチャの修正

以下の3つのポイントを修正したコードに差し替えてみてください。

#### 1. `OnPreviewMouseDown` でイベントを止める

クリックした瞬間に `e.Handled = true` を設定し、`ListBox` 本来の選択処理を一旦ブロックします。その代わり、自分で選択状態を管理します。

#### 2. キャプチャを「自分（NodeContainer）」で取る

`ListBoxItem` にキャプチャをさせると、マウスがそのアイテムの外に出た時にイベントが不安定になることがあります。親である `NodeContainer` 全体でマウスを捕まえるのが定石です。

---

### 修正版：NodeContainer.cs

```csharp
protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
{
    base.OnPreviewMouseDown(e);

    // 1. クリックされた要素がどの ListBoxItem なのかを探す
    var item = ContainerFromElement((DependencyObject)e.OriginalSource) as ListBoxItem;
    if (item == null) return;

    // 2. ドラッグ開始の準備
    _isDragging = true;
    _startPoint = e.GetPosition(this);

    // 3. 【重要】クリックしたアイテムが未選択なら、それを選択状態にする
    // (Ctrlキーなどを押していない場合の標準的な挙動を自前で再現)
    if (!item.IsSelected)
    {
        if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
        {
            UnselectAll();
        }
        item.IsSelected = true;
    }

    // 4. 【重要】マウスキャプチャをこのコントロール自身で開始する
    this.CaptureMouse();

    // 5. 【重要】イベントをここで「完了」とし、ListBoxの標準ロジックに渡さない
    e.Handled = true; 
}

protected override void OnPreviewMouseMove(MouseEventArgs e)
{
    if (!_isDragging) return;

    Point currentPoint = e.GetPosition(this);
    Vector delta = currentPoint - _startPoint;

    if (delta.Length > 0)
    {
        foreach (var selectedItem in SelectedItems.Cast<Data>())
        {
            selectedItem.X += delta.X;
            selectedItem.Y += delta.Y;
        }
        _startPoint = currentPoint;
    }

    // 移動中もイベントを他へ流さない
    e.Handled = true;
}

protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
{
    if (_isDragging)
    {
        // キャプチャを終了
        this.ReleaseMouseCapture();
        _isDragging = false;
        e.Handled = true;
    }

    base.OnPreviewMouseUp(e);
}

```

---

### なぜこれで直るのか？

1. **`e.Handled = true`**:
WPFのイベントは「トンネル（Preview）」→「バブル」という順で流れます。`PreviewMouseDown` で `Handled = true` にすると、その後の標準的な「選択（SelectionChanged）」イベントが発生しなくなります。これにより、移動中に勝手に他の要素が選ばれるのを防げます。
2. **`this.CaptureMouse()`**:
マウスキャプチャを `NodeContainer` 自身で持つことで、「マウスがどこに動こうが、ボタンを離すまではすべてのマウスイベントは俺（NodeContainer）が処理する！」という状態になります。これで、途中に他の図形があっても無視して移動に集中できます。

---

### 💡 さらに使いやすくするためのヒント

もし「図形以外の背景をクリックしたときに選択を解除したい」場合は、以下を `OnPreviewMouseDown` の冒頭に追加してください。

```csharp
// 背景（Canvas部分）をクリックした場合
if (e.OriginalSource == this || e.OriginalSource is Canvas)
{
    UnselectAll();
    return;
}

```

これで、他の要素に吸い込まれることなく、スムーズにドラッグできるようになるはずです。試してみてください！

次は、**「ドラッグ中の図形を一番手前に表示する（ZIndex）」** や **「グリッドに吸い付かせる」** などの調整が必要になりますか？


---


