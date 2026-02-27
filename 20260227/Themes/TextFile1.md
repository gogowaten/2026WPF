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