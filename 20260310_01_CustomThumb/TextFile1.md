# WPFのThumbのCustomControlの作り方

例えば以下のようなXAMLはめんどくさい
```XAML
    <Thumb>
      <Thumb.Template>
        <ControlTemplate>
          <Ellipse Width="30" Height="30" Fill="Yellow"/>
        </ControlTemplate>
      </Thumb.Template>
    </Thumb>
```
これを以下のように書きたい場合は、どのようなCustomControl(CustomThumb)を作れば良い？
```XAML
    <local:CustomThumb>
      <Ellipse Width="30" Height="30" Fill="Yellow"/>
    </local:CustomThumb>
```


---


WPFで`Thumb`の中に直接要素を書きたいというニーズ、よく分かります。標準の`Thumb`は`ContentControl`を継承しておらず、外見を自由に変えるには毎回`ControlTemplate`をいじる必要があるのが少し不便ですよね。

お望みの「中身を直感的に書き込めるThumb」を作るには、**`ContentControl` を継承して `Thumb` の機能を組み込む**か、**`Thumb` を継承して `Content` プロパティを追加する**のが正攻法です。

今回は、後者の「`Thumb` を継承しつつ、中身（Content）を受け取れるようにする」方法をご紹介します。

---

### 1. CustomThumb.cs の作成

まず、`Content` プロパティ（依存関係プロパティ）を定義したクラスを作成します。

```csharp
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

namespace YourNamespace
{
    // Content属性をXAMLで直感的に書けるようにする属性
    [ContentProperty(nameof(Content))]
    public class CustomThumb : Thumb
    {
        static CustomThumb()
        {
            // デフォルトのスタイルをこのクラス向けに適用する
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomThumb), 
                new FrameworkPropertyMetadata(typeof(CustomThumb)));
        }

        // 汎用的な中身を入れるためのプロパティ
        public object Content
        {
            get => (object)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(nameof(Content), typeof(object), typeof(CustomThumb), new PropertyMetadata(null));
    }
}

```

### 2. Generic.xaml でのスタイル定義

次に、`Themes/Generic.xaml`（または `App.xaml` やリソース辞書）で、`Content` プロパティを表示するための `ControlTemplate` を一度だけ定義します。

```xml
<Style TargetType="{x:Type local:CustomThumb}">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type local:CustomThumb}">
                <ContentPresenter Content="{TemplateBinding Content}" />
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>

```

---

### これでどう書けるようになるか

この準備をしておけば、あとはやりたかった形式で書くだけです。

```xml
<local:CustomThumb DragDelta="OnDragDelta">
    <Ellipse Width="30" Height="30" Fill="Yellow" Stroke="Orange" StrokeThickness="2"/>
</local:CustomThumb>

```

### このアプローチのメリット

* **直感的:** `ControlTemplate` の入れ子構造を意識せずに済みます。
* **機能維持:** `Thumb` を継承しているため、`DragDelta` や `DragStarted` などのイベントはそのまま使えます。
* **柔軟性:** `Ellipse` だけでなく、`Grid` や `Image` など、好きなものを放り込めます。

### ヒント

もし、配置する要素によって `Thumb` のヒットテスト（クリック判定）が効かなくなる場合は、`Ellipse` などの背景が透明（`Fill="Transparent"`）であることを確認してください。

この `CustomThumb` を使って、どのようなUI（例えばスライダーやノードエディタなど）を作成される予定ですか？もし必要であれば、ドラッグ移動の実装コードなどもお出しできますよ。


---


# ささっきのGeneric.xamlのなかで
```XAML
          <ContentPresenter Content="{TemplateBinding MyContent}"/>
これは
          <ContentControl Content="{TemplateBinding MyContent}"/>

```

と書いても動くけど、どこが違うの？ContentPresenterとContentControlの違いを解説して



---


鋭いところに気がつきましたね！結論から言うと、**「役割（レイヤー）」**が違います。

`ContentControl` を使っても動くのは、`ContentControl` が内部的に `ContentPresenter` を持っているからですが、今回のような「テンプレート（中身の器）を作る」場面では、**`ContentPresenter` を使うのが正解**です。

それぞれの違いを整理して解説します。

---

### 1. ContentControl は「データを持つ器」

`ContentControl` は、`Content` というデータ（文字列、ボタン、図形など）を保持するための**「完成品としてのコントロール」**です。

* **構造:** 内部にデフォルトの `ControlTemplate` を持っています。
* **役割:** ユーザーが UI の一部としてそのまま配置して使うものです（`Button`, `Label`, `Window` などはすべてこれの仲間です）。

### 2. ContentPresenter は「データを表示する窓」

`ContentPresenter` は、受け取ったコンテンツを**「実際に画面に描画する（流し込む）ための部品」**です。

* **構造:** テンプレートを持たず、非常に軽量です。
* **役割:** 主に `ControlTemplate` の中で**「ここに中身を表示してね」という場所を指定する**ために使われます。

---

### なぜ今回のケースで ContentPresenter が適しているのか？

今回の `CustomThumb` の `ControlTemplate` 内で比較してみましょう。

#### ContentPresenter の場合（推奨）

```xml
<ControlTemplate TargetType="{x:Type local:CustomThumb}">
    <ContentPresenter Content="{TemplateBinding Content}" />
</ControlTemplate>

```

* **動作:** `CustomThumb.Content` に入れた `Ellipse` が、そのままそこに配置されます。余計な装飾は一切入りません。

#### ContentControl の場合

```xml
<ControlTemplate TargetType="{x:Type local:CustomThumb}">
    <ContentControl Content="{TemplateBinding Content}" />
</ControlTemplate>

```

* **動作:** `CustomThumb` の中に「もう一つのコントロール（ContentControl）」が作られ、その中に `Ellipse` が入ります。
* **無駄:** `ContentControl` には独自の `Padding` や `Background` などのプロパティがあり、意図しない余白やスタイルが干渉する可能性があります。また、描画のために余計なオブジェクトが生成されるため、パフォーマンス的にもわずかにオーバーヘッドがあります。

---

### 違いのまとめ表

| 特徴 | ContentControl | ContentPresenter |
| --- | --- | --- |
| **主な用途** | 独立したコントロールとして使用 | **ControlTemplate の中**で使用 |
| **テンプレート** | 自身が Template を持つ | 自身は Template を持たない |
| **オーバーヘッド** | 重い（様々な機能が付随するため） | **軽い**（表示に特化しているため） |
| **今回の目的** | 不適切（入れ子構造が深くなる） | **適切（中身を素直に表示する）** |

> [!TIP]
> **「カスタムコントロールのテンプレートを作るときは ContentPresenter を使う」**と覚えておけば間違いありません。逆に、プログラムから動的にコンテンツを表示する場所を画面にポンと置きたいときは `ContentControl` を使うのが一般的です。

この `ContentPresenter` を使ったカスタマイズ、次はマウスオーバーで色を変えるなどの「トリガー」を追加してみたくなりますか？それとも、このままドラッグ機能を実装していきますか？