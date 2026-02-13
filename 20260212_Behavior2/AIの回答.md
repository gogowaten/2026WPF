

WPFにおける**ビヘイビア (Behavior)** は、簡単に言うと「**再利用可能なアクションのパッケージ**」です。

本来、コントロールに特定の振る舞い（マウスオーバーでアニメーションさせる、Enterキーでフォーカスを移動するなど）を追加するには、コードビハインド（`.xaml.cs`）にイベントハンドラーを書く必要があります。しかし、これではコードが散らかり、他のプロジェクトで使い回すのが大変です。

ビヘイビアを使うことで、**C#のロジックをカプセル化し、XAML上でドラッグ＆ドロップするように適用できる**ようになります。

---

## 1. なぜビヘイビアを使うのか？

* **Viewとロジックの分離**: MVVMパターンを維持しつつ、UI固有の複雑な挙動を実装できます。
* **再利用性**: 一度作れば、ボタンやテキストボックスなど、どのコントロールにも使い回せます。
* **XAMLのクリーン化**: コードビハインドを汚さず、宣言的に振る舞いを記述できます。

---

## 2. 準備：ライブラリの導入

現代のWPF開発では、Microsoftが提供している **`Microsoft.Xaml.Behaviors.Wpf`** というNuGetパッケージを使用するのが標準です。

1. Visual Studioで「NuGetパッケージの管理」を開く。
2. `Microsoft.Xaml.Behaviors.Wpf` をインストールする。

---

## 3. 実装例：マウスが入ったら色が変わるビヘイビア

実際に独自のビヘイビアを作る手順を見てみましょう。

### C# 側 (MouseOverBehavior.cs)

`Behavior<T>` を継承して作成します。`T` には対象とするコントロールの型を指定します。

```csharp
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

public class MouseOverBehavior : Behavior<Border>
{
    // アタッチされた時の処理
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.MouseEnter += OnMouseEnter;
        AssociatedObject.MouseLeave += OnMouseLeave;
    }

    // デタッチ（切り離し）された時の処理（メモリリーク防止）
    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.MouseEnter -= OnMouseEnter;
        AssociatedObject.MouseLeave -= OnMouseLeave;
    }

    private void OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        AssociatedObject.Background = Brushes.LightBlue;
    }

    private void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        AssociatedObject.Background = Brushes.Transparent;
    }
}

```

### XAML 側 (MainWindow.xaml)

作成したビヘイビアをコントロールに登録します。

```xml
<Window ...
        xmlns:i="http://schemas.microsoft.com/xaml/behaviors"
        xmlns:local="clr-namespace:YourProjectName">

    <Grid>
        <Border Width="200" Height="100" BorderBrush="Black" BorderThickness="1">
            <i:Interaction.Behaviors>
                <local:MouseOverBehavior />
            </i:Interaction.Behaviors>
            <TextBlock Text="ここにマウスを乗せてね" HorizontalAlignment="Center" VerticalAlignment="Center"/>
        </Border>
    </Grid>
</Window>

```

---

## 4. よく使われる標準機能：`EventTrigger`

自作しなくても、標準で用意されている便利なビヘイビアもあります。特に「イベントが発生した時にViewModelのコマンドを実行する」という使い方が最も一般的です。

```xml
<Button Content="クリック！">
    <i:Interaction.Triggers>
        <i:EventTrigger EventName="Click">
            <i:InvokeCommandAction Command="{Binding MyCommand}" />
        </i:EventTrigger>
    </i:Interaction.Triggers>
</Button>

```

これにより、コードビハインドを一切通さずに、UIのイベントとViewModelを直結できます。

---

## まとめ

| 要素 | 役割 |
| --- | --- |
| **`Behavior<T>`** | 特定のコントロールに機能を追加するための基底クラス。 |
| **`AssociatedObject`** | ビヘイビアが貼り付けられている実際のコントロール本体。 |
| **`OnAttached / OnDetaching`** | イベントの登録・解除を行う重要なライフサイクルメソッド。 |

ビヘイビアは、「添付プロパティ」よりも複雑な（複数のイベントが絡むような）UI操作を共通化したい時に非常に強力な武器になります。

**次は、具体的なユースケース（例：数字しか入力できないTextBoxなど）に合わせたコードを書いてみましょうか？**




<br><br><br>

---
# 質問
今回の
public class MouseOverBehavior : Behavior<Border>
このビヘイビアはBorder専用なの？
---
<br><br><br>


結論から言うと、今の書き方では **Borderコントロール専用** になっています。

理由は、クラス定義の `Behavior<Border>` の部分で、対象（`AssociatedObject`）を `Border` 型として固定しているからです。

もし、ButtonやGridなど、他のコントロールでも使い回したい場合は、ターゲットをより抽象的なクラスに変更する必要があります。

---

## 汎用的にする方法（共通化）

WPFの多くのコントロールは `FrameworkElement` や `Control` クラスを継承しています。これらを指定することで、幅広いコントロールに適用できるようになります。

### 1. C#側の修正：`FrameworkElement` に変更

背景色（`Background`）を持つコントロール全般で使いたい場合は、`Panel` や `Control` など、そのプロパティを持っている共通の親クラスを指定します。

```csharp
// Border ではなく FrameworkElement や Control を指定する
public class MouseOverBehavior : Behavior<FrameworkElement>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.MouseEnter += OnMouseEnter;
        AssociatedObject.MouseLeave += OnMouseLeave;
    }

    private void OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        // 背景色を変更したいが、FrameworkElement自体はBackgroundプロパティを持っていない。
        // なので、動的にプロパティがあるかチェックしてキャストする。
        if (AssociatedObject is Control control) {
            control.Background = Brushes.LightBlue;
        }
        else if (AssociatedObject is Border border) {
            border.Background = Brushes.LightBlue;
        }
        else if (AssociatedObject is Panel panel) {
            panel.Background = Brushes.LightBlue;
        }
    }
    // ... OnMouseLeave も同様
}

```

### 2. もっと「汎用的」に作るコツ

特定の型に依存したくない場合は、**依存関係プロパティ (Dependency Property)** をビヘイビアに追加して、色などを外側（XAML）から指定できるようにするのが一般的です。

```csharp
public class ColorChangeBehavior : Behavior<FrameworkElement>
{
    // XAMLから色を指定できるようにプロパティを定義
    public Brush HoverColor { get; set; } = Brushes.Yellow;

    // ... 
}

```

---

## 使い分けのガイドライン

| 指定する型 | 特徴 |
| --- | --- |
| **`Behavior<Border>`** | **専用設計**。Border特有の `CornerRadius` などを操作したい場合に最適。 |
| **`Behavior<Control>`** | **ボタンやリスト用**。`Background` や `Foreground` を持つ一般的な部品向け。 |
| **`Behavior<FrameworkElement>`** | **ほぼ全てのUI要素用**。`Width` や `Opacity` など、基本プロパティを操作する場合に便利。 |
| **`Behavior<DependencyObject>`** | **最強の汎用性**。ただし、UIイベント（MouseEnterなど）を扱うにはキャストが必要。 |

### 注意点：キャストが必要な理由

`AssociatedObject` は、`Behavior<T>` で指定した `T` の型として扱われます。

* `Behavior<Border>` と書けば、`AssociatedObject` は最初から `Border` 型。
* `Behavior<FrameworkElement>` と書けば、`AssociatedObject` は `FrameworkElement` 型。

汎用性を高くしすぎると、特定のコントロールにしかないプロパティ（例：`TextBox` の `Text`）にアクセスできなくなるため、**「そのビヘイビアが何を操作したいか」**によって `T` を決めるのがベストです。

---

次は、このビヘイビアをさらに便利にするために、**「XAMLから色を自由に変えられるようにプロパティを追加する方法」**を解説しましょうか？



<br><br><br>

---
# 次は、このビヘイビアをさらに便利にするために、**「XAMLから色を自由に変えられるようにプロパティを追加する方法」**を解説して
---

ビヘイビアをさらに実用的にしましょう！

ビヘイビアに「依存関係プロパティ（Dependency Property）」を追加すると、XAML側から値を渡せるようになります。これにより、ある場所では「赤」、別の場所では「青」といった具合に、同じロジックを使い回しながら見た目だけを変えることができます。

---

## 1. 依存関係プロパティの実装

「マウスが乗った時の色」を外部から設定できるようにします。

### C# 側 (ColorChangeBehavior.cs)

`propdp` と入力して `Tab` キーを2回押すと、依存関係プロパティの雛形が生成されます。

```csharp
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

public class ColorChangeBehavior : Behavior<Control>
{
    // 1. 依存関係プロパティの定義 (MouseOverColor)
    public static readonly DependencyProperty MouseOverColorProperty =
        DependencyProperty.Register(
            nameof(MouseOverColor), 
            typeof(Brush), 
            typeof(ColorChangeBehavior), 
            new PropertyMetadata(Brushes.Transparent));

    public Brush MouseOverColor
    {
        get => (Brush)GetValue(MouseOverColorProperty);
        set => SetValue(MouseOverColorProperty, value);
    }

    // 元の色を保存しておく変数
    private Brush _originalColor;

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.MouseEnter += OnMouseEnter;
        AssociatedObject.MouseLeave += OnMouseLeave;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.MouseEnter -= OnMouseEnter;
        AssociatedObject.MouseLeave -= OnMouseLeave;
    }

    private void OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        // 現在の色を保存してから、設定された色に変える
        _originalColor = AssociatedObject.Background;
        AssociatedObject.Background = MouseOverColor;
    }

    private void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        // 元の色に戻す
        AssociatedObject.Background = _originalColor;
    }
}

```

---

## 2. XAML での使い方

プロパティを追加したことで、XAML側から自由に色を指定できるようになります。

```xml
<StackPanel Spacing="10">
    <Button Content="青くなるボタン" Padding="20">
        <i:Interaction.Behaviors>
            <local:ColorChangeBehavior MouseOverColor="LightBlue" />
        </i:Interaction.Behaviors>
    </Button>

    <Button Content="オレンジになるボタン" Padding="20">
        <i:Interaction.Behaviors>
            <local:ColorChangeBehavior MouseOverColor="Orange" />
        </i:Interaction.Behaviors>
    </Button>
</StackPanel>

```

---

## 3. ここがポイント！

* **`DependencyProperty` を使う理由**:
単なるプロパティ（`public Brush Color { get; set; }`）だと、XAMLで **Binding（バインド）** が使えません。依存関係プロパティにすることで、`MouseOverColor="{Binding SelectedColor}"` のようにViewModelの値と連動させることも可能になります。
* **型の選択**:
今回は `Behavior<Control>` にしました。これにより、`Button`、`TextBox`、`ListBox` など、`Background` プロパティを持つ主要なコントロールのほとんどでこのビヘイビアが使い回せます。

---

## 応用：アニメーションも追加できる

この仕組みを使えば、色を変えるだけでなく、`DoubleAnimation` を使って「ゆっくり色を変える」といった複雑な動きもビヘイビアの中に隠蔽できます。

次は、**「この色変更をアニメーション（ふわっと変わる）にする方法」**に挑戦してみますか？それとも、**「ViewModelのコマンドを呼び出す方法」**を詳しく見たいですか？

<br><br><br>



---
# textboxは色が変わるけど、Buttonは指定した色に変わらない
---

鋭いですね！実はこれ、WPFの**Buttonが持つ「デフォルトのテンプレート（スタイル）」**が原因です。

### なぜ変わらないのか？

WPFのButtonには、マウスが乗ったとき（IsMouseOverがTrueのとき）の挙動が、あらかじめ**「コントロールテンプレート」**の中にガチガチに定義されています。

具体的には、ボタンのテンプレート内に「マウスが乗ったらこの色（薄い青など）にする」というアニメーションやトリガーが仕込まれており、ビヘイビアから `Background` を書き換えても、**テンプレート内のトリガーが上書きして隠してしまう**のです。

---

### 解決策1：テンプレートをシンプルにする

一番手っ取り早くビヘイビアの効果を確認するには、ボタンの「標準の見た目」をリセットすることです。

```xml
<Button Content="これなら変わる">
    <Button.Template>
        <ControlTemplate TargetType="Button">
            <Border Background="{TemplateBinding Background}" Padding="10">
                <ContentPresenter HorizontalAlignment="Center" />
            </Border>
        </ControlTemplate>
    </Button.Template>
    
    <i:Interaction.Behaviors>
        <local:ColorChangeBehavior MouseOverColor="Red" />
    </i:Interaction.Behaviors>
</Button>

```

これで、ビヘイビアが変更した `Background` が正しく反映されます。

---

### 解決策2：ビヘイビア側で「アニメーション」を使う（推奨）

標準ボタンの見た目を維持しつつ色を変えたい場合は、プロパティを直接書き換えるのではなく、**アニメーションで優先的に上書きする**手法がよく取られます。

ただし、WPFの標準ボタンは「マウスオーバー時の色」が内部で複雑に制御されているため、単純なビヘイビアよりも **「Style（スタイル）」の `Trigger**` で書くのがWPFの王道だったりします。

---

### 解決策3：ビヘイビアを「プロパティ変更」から「アニメーション」へ

もし「どうしてもビヘイビアでボタンを光らせたい！」という場合は、前回のコードの `OnMouseEnter` を以下のように書き換えてみてください。

```csharp
private void OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
{
    // 背景色をアニメーションで強制的に変更する
    var animation = new ColorAnimation
    {
        To = ((SolidColorBrush)MouseOverColor).Color,
        Duration = TimeSpan.FromSeconds(0.2) // 0.2秒で変化
    };
    
    // Backgroundの「Color」プロパティに対してアニメーションをかける
    AssociatedObject.Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);
}

```

※これを行うには、ボタンの元の `Background` が `SolidColorBrush` である必要があります。

---

### 補足：WPFの「あるある」です

「TextBoxは動くのに、Buttonは動かない」というのは、WPF学習者が必ず通る道です。

* **TextBox**: マウスが乗っても背景色を勝手に変える強力なトリガーがデフォルトで少ない。
* **Button**: 「マウスが乗った時」「クリックした時」の見た目の変化がテンプレートに細かく定義されている。

**「ボタンの見た目をもっと自由にカスタマイズする方法（コントロールテンプレート）」について詳しく知りたいですか？それとも、「アニメーション」をビヘイビアに組み込む完成版コード**を見てみますか？



<br><br><br>



---

# 具体的なユースケース（例：数字しか入力できないTextBoxなど）に合わせたコードを書いて
---


「数字しか入力できないTextBox」は、ビヘイビアの真骨頂ですね！

これを実装するには、**「テキストが入力される直前」**に内容をチェックし、数字以外なら入力をキャンセルするというロジックをビライビアに封じ込めます。

---

## 実装：NumericInputBehavior

### 1. C# 側 (NumericInputBehavior.cs)

`TextBox` 専用のビヘイビアとして作成します。キーボード入力だけでなく、コピペ（貼り付け）にも対応させるのがポイントです。

```csharp
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;

public class NumericInputBehavior : Behavior<TextBox>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        // 1. キーボード入力の制限
        AssociatedObject.PreviewTextInput += OnPreviewTextInput;
        // 2. スペースキーの制限
        AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
        // 3. 貼り付け（コピペ）の制限
        DataObject.AddPastingHandler(AssociatedObject, OnPaste);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.PreviewTextInput -= OnPreviewTextInput;
        AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
        DataObject.RemovePastingHandler(AssociatedObject, OnPaste);
    }

    // 数字かどうかを判定する正規表現
    private static readonly Regex _regex = new Regex("[^0-9]+"); 

    private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // 入力された文字が数字以外なら、イベントを処理済み(Handled)にして入力を破棄する
        e.Handled = _regex.IsMatch(e.Text);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        // スペースキーはTextCompositionイベントが発生しないため、ここで個別に制限
        if (e.Key == Key.Space)
        {
            e.Handled = true;
        }
    }

    private void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(DataFormats.Text))
        {
            string text = (string)e.DataObject.GetData(DataFormats.Text);
            if (_regex.IsMatch(text))
            {
                // 貼り付けようとしている文字列に数字以外が含まれていればキャンセル
                e.CancelCommand();
            }
        }
        else
        {
            e.CancelCommand();
        }
    }
}

```

---

## 2. XAML 側での使い方

このビヘイビアを適用するだけで、その `TextBox` は数字専用マシンに早変わりします。

```xml
<StackPanel Margin="20" Spacing="10">
    <Label Content="年齢を入力してください（数字のみ）:" />
    <TextBox Width="200" HorizontalAlignment="Left">
        <i:Interaction.Behaviors>
            <local:NumericInputBehavior />
        </i:Interaction.Behaviors>
    </TextBox>
</StackPanel>

```

---

## 3. このビヘイビアのすごいところ

もしこのロジックをコードビハインド（`.xaml.cs`）に書くと、数字専用の入力欄が10個あれば、10個分（あるいは共通メソッドを呼び出す手間）のコードが必要になります。

ビヘイビアなら：

* **カプセル化**: 正規表現やコピペ対策の「面倒なロジック」を隠せる。
* **宣言的**: XAMLを見るだけで「あ、このTextBoxは数字制限がかかっているな」と一目でわかる。
* **クリーン**: ViewModelやViewのコードを一切汚さない。

---

## さらなるステップ：プロパティの追加

これをさらに汎用的にするなら、以下のような **Dependency Property** を追加しても面白いですよ。

* **`AllowDecimal` (bool)**: 小数点を許可するかどうか。
* **`MaxLength` (int)**: 入力できる最大桁数。

**「マイナス記号や小数点も許可する」ような、もう少し複雑な正規表現を使ったバージョンに改良してみますか？**


<br><br><br>



---
# 日本語入力をオンにすると文字も入力できてしまうのと、
<StackPanel Margin="20" Spacing="10"> これのspacing="10"はエラーになる
---


おっと、失礼しました！ご指摘の通り、WPFの標準 `StackPanel` には `Spacing` プロパティは存在しません（WinUIやAvaloniaにはありますが、WPFでは `Margin` で調整するのが通例です）。

また、**「日本語入力（IME）」**は、確定するまで `PreviewTextInput` イベントが発生しないため、すり抜けてしまいますね。これはWPFの「あるある」な落とし穴です。

これらを解決した「完全版」のコードに修正しましょう！

---

## 1. 日本語入力（IME）を無効化する

最も確実なのは、ビヘイビアの中で**「そのTextBoxではIMEをOFFにする」**という設定を強制することです。

### 修正版：NumericInputBehavior.cs

```csharp
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;

public class NumericInputBehavior : Behavior<TextBox>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        
        // --- IME（日本語入力）を強制的に無効化 ---
        InputMethod.SetIsInputMethodEnabled(AssociatedObject, false);

        AssociatedObject.PreviewTextInput += OnPreviewTextInput;
        AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
        DataObject.AddPastingHandler(AssociatedObject, OnPaste);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.PreviewTextInput -= OnPreviewTextInput;
        AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
        DataObject.RemovePastingHandler(AssociatedObject, OnPaste);
    }

    private static readonly Regex _regex = new Regex("[^0-9]+");

    private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = _regex.IsMatch(e.Text);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        // スペースを禁止
        if (e.Key == Key.Space) e.Handled = true;
    }

    private void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(DataFormats.Text))
        {
            string text = (string)e.DataObject.GetData(DataFormats.Text);
            if (_regex.IsMatch(text)) e.CancelCommand();
        }
        else
        {
            e.CancelCommand();
        }
    }
}

```

---

## 2. XAML の修正（Spacingエラーの回避）

WPFの `StackPanel` には `Spacing` がないので、代わりに各要素に `Margin` を設定するか、`Grid` などを使います。

```xml
<StackPanel Margin="20">
    <Label Content="年齢を入力してください（数字のみ）:" Margin="0,0,0,5"/>
    
    <TextBox Width="200" HorizontalAlignment="Left">
        <i:Interaction.Behaviors>
            <local:NumericInputBehavior />
        </i:Interaction.Behaviors>
    </TextBox>
    
    <TextBlock Text="※IMEは自動的にオフになります" FontSize="10" Foreground="Gray" Margin="0,5,0,0"/>
</StackPanel>

```

---

## 💡 解説：なぜ `InputMethod.SetIsInputMethodEnabled` なのか？

日本語入力がオンだと、キーを叩いた瞬間は「未確定文字列」として扱われ、`PreviewTextInput` がスルーされます。
確定した瞬間にまとめてテキストが入りますが、そのタイミングで止めるのはかなり複雑なロジックが必要です。

そのため、**「数字しか入れない場所なら、最初から日本語入力をさせない（IMEを無効にする）」**のが、UX（ユーザー体験）的にも実装のシンプルさ的にもベストな選択となります。

---

## 次のステップ

これで「数字しか入らない完璧なTextBox」ができましたね！

もし「**全角数字を半角に自動変換して受け入れたい**」とか「**マイナス記号だけは許したい**」といった、さらにマニアックな要望があれば、それもビヘイビアで解決できます。どうしますか？


<br><br><br>


---
# DataObject.AddPastingHandler(AssociatedObject, OnPaste); これの
DataObjectについて解説してほしい
---


`DataObject.AddPastingHandler` という記述、初見だと「なぜ `TextBox` のイベントじゃないの？」と不思議に思いますよね。

実はこれ、WPFの**「添付イベント (Attached Event)」**という仕組みを利用しています。詳しく紐解いていきましょう。

---

## 1. DataObject クラスとは？

`System.Windows.DataObject` は、一言で言えば**「クリップボードやドラッグ＆ドロップでやり取りされるデータの入れ物」**です。

WPFでは、テキストだけでなく、画像、ファイル、カスタムオブジェクトなど、多様な形式のデータをやり取りします。その際、データがどのような形式（プレーンテキストなのか、HTMLなのか等）であるかを管理するのがこのクラスの役割です。

---

## 2. なぜ `TextBox.Paste` イベントではないのか？

驚くべきことに、WPFの `TextBox` には標準で **`Paste` という名前のイベントが存在しません。**

代わりに、WPFの「コマンドシステム」が背後で動いています。

1. ユーザーが `Ctrl + V` を押す。
2. `ApplicationCommands.Paste` というコマンドが実行される。
3. `TextBox` がそれを受け取って内部で処理する。

この「コマンドが実行されて、データが貼り付けられる直前」のタイミングを横取りするために、`DataObject` クラスが定義している **`Pasting` 添付イベント** を利用します。

---

## 3. `AddPastingHandler` の仕組み

```csharp
DataObject.AddPastingHandler(AssociatedObject, OnPaste);

```

この一行は、**「`AssociatedObject`（TextBox）で『貼り付け』が発生したとき、私の `OnPaste` メソッドを呼んでね」**と予約する特殊な書き方です。

### 引数の解説

* **第1引数 (`AssociatedObject`)**: イベントを監視したい対象（ターゲット）。
* **第2引数 (`OnPaste`)**: 実際に実行したいメソッド（ハンドラー）。

---

## 4. `OnPaste` の中身を深掘り

実装例で書いたコードをもう一度見てみましょう。

```csharp
private void OnPaste(object sender, DataObjectPastingEventArgs e)
{
    // 1. 貼り付けようとしているデータの中に「テキスト」があるか確認
    if (e.DataObject.GetDataPresent(DataFormats.Text))
    {
        // 2. そのテキストを取り出す
        string text = (string)e.DataObject.GetData(DataFormats.Text);

        // 3. もし数字以外が含まれていたら...
        if (_regex.IsMatch(text))
        {
            // 4. 「この貼り付け操作自体を無効（キャンセル）」にする！
            e.CancelCommand();
        }
    }
}

```

* **`e.DataObject`**: いまクリップボードから流れてこようとしている「データの塊」そのものです。
* **`e.CancelCommand()`**: これを呼ぶことで、TextBoxに文字が表示される前に、貼り付け処理そのものを「なかったこと」にできます。

---

## まとめ

* `DataObject` は、クリップボードなどの**「データの運び屋」**。
* `AddPastingHandler` は、TextBoxに**「貼り付け専用のセンサー」**を後付けするイメージ。
* これを使う理由は、**TextBox自体に Paste イベントがないから**。

ビヘイビアを使わずにコードビハインドで書くと、この「登録」と「解除」の管理が面倒ですが、ビヘイビアの `OnAttached` / `OnDetaching` でこれを行うのは、非常にスマートな実装パターンと言えます。

**「ドラッグ＆ドロップ」を制限したい場合も、実はこれと似たような仕組みを使うんですよ。そちらも興味ありますか？**


<br><br><br>


---
# 依存関係プロパティとして以下を追加して
* AllowDecimal (bool): 小数点を許可するかどうか。
* MaxLength (int): 入力できる最大桁数。
---


承知いたしました。`AllowDecimal` と `MaxLength` を追加して、より高機能な数値入力ビヘイビアに進化させましょう。

WPFの `TextBox` には標準の `MaxLength` プロパティがありますが、ビヘイビア側でこれを制御することで、「貼り付け」時も含めた厳密な文字数制限が可能になります。

---

## 修正版：NumericInputBehavior.cs

```csharp
using Microsoft.Xaml.Behaviors;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public class NumericInputBehavior : Behavior<TextBox>
{
    // --- 依存関係プロパティ ---

    // 小数点を許可するか
    public static readonly DependencyProperty AllowDecimalProperty =
        DependencyProperty.Register(nameof(AllowDecimal), typeof(bool), typeof(NumericInputBehavior), new PropertyMetadata(false));

    public bool AllowDecimal
    {
        get => (bool)GetValue(AllowDecimalProperty);
        set => SetValue(AllowDecimalProperty, value);
    }

    // 最大桁数（0は制限なし）
    public static readonly DependencyProperty MaxLengthProperty =
        DependencyProperty.Register(nameof(MaxLength), typeof(int), typeof(NumericInputBehavior), new PropertyMetadata(0));

    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    // --- イベント制御 ---

    protected override void OnAttached()
    {
        base.OnAttached();
        InputMethod.SetIsInputMethodEnabled(AssociatedObject, false);
        AssociatedObject.PreviewTextInput += OnPreviewTextInput;
        AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
        DataObject.AddPastingHandler(AssociatedObject, OnPaste);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.PreviewTextInput -= OnPreviewTextInput;
        AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
        DataObject.RemovePastingHandler(AssociatedObject, OnPaste);
    }

    private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // 1. 入力後の全体テキストを予測
        string fullText = GetFullTextAfterInput(e.Text);

        // 2. バリデーション
        e.Handled = !IsValid(fullText);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space) e.Handled = true;
    }

    private void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(DataFormats.Text))
        {
            string pasteText = (string)e.DataObject.GetData(DataFormats.Text);
            string fullText = GetFullTextAfterInput(pasteText);

            if (!IsValid(fullText)) e.CancelCommand();
        }
        else
        {
            e.CancelCommand();
        }
    }

    // --- ロジック補助 ---

    // 入力後の文字列がどうなるかをシミュレーションする
    private string GetFullTextAfterInput(string inputText)
    {
        string currentText = AssociatedObject.Text;
        int selectionStart = AssociatedObject.SelectionStart;
        int selectionLength = AssociatedObject.SelectionLength;

        // 選択範囲を消して新しい文字を挿入した状態を作る
        return currentText.Remove(selectionStart, selectionLength).Insert(selectionStart, inputText);
    }

    // 文字列が設定条件を満たしているかチェック
    private bool IsValid(string text)
    {
        // 空文字は許可（削除操作などのため）
        if (string.IsNullOrEmpty(text)) return true;

        // 桁数チェック
        if (MaxLength > 0 && text.Length > MaxLength) return false;

        // 形式チェック（正規表現）
        string pattern = AllowDecimal ? @"^[0-9]*\.?[0-9]*$" : @"^[0-9]*$";
        return Regex.IsMatch(text, pattern);
    }
}

```

---

## XAMLでの使用例

依存関係プロパティとして定義したため、以下のように属性で挙動を切り替えられます。

```xml
<StackPanel Margin="20">
    <Label Content="郵便番号 (5桁制限):" />
    <TextBox Width="150" HorizontalAlignment="Left" Margin="0,0,0,10">
        <i:Interaction.Behaviors>
            <local:NumericInputBehavior AllowDecimal="False" MaxLength="5" />
        </i:Interaction.Behaviors>
    </TextBox>

    <Label Content="金額（小数点可）:" />
    <TextBox Width="150" HorizontalAlignment="Left">
        <i:Interaction.Behaviors>
            <local:NumericInputBehavior AllowDecimal="True" MaxLength="10" />
        </i:Interaction.Behaviors>
    </TextBox>
</StackPanel>

```

---

## 💡 解説と工夫した点

1. **`GetFullTextAfterInput` の導入**:
単に「入力された1文字」を検査するのではなく、**「入力した結果、テキストボックス全体がどうなるか」**を予測して判定するようにしました。これにより、「`.` が2回入力される」といった不正な状態を確実に防げます。
2. **`MaxLength` の二重ガード**:
TextBox自体のプロパティではなくビヘイビアで判定することで、コピペで長い文字列が放り込まれるのも未然に防いでいます。
3. **正規表現の動的切り替え**:
`AllowDecimal` の値によって、使用する正規表現を `^[0-9]*$`（整数のみ）と `^[0-9]*\.?[0-9]*$`（小数点1つまで許可）で使い分けています。

これで、かなり実用的な「数値入力専用コンポーネント」になりましたね。

**例えば、「入力後に値が確定したら（LostFocus時）、カンマ区切りに自動整形する」といった機能を追加することも可能ですが、興味はありますか？**