WPFにおいて、描画された要素（TextBlock）のサイズをデータ（TextBlockData）へ逆方向に反映させるには、いくつかの壁があります。

1. **Bindingの方向**: デフォルトの`Width`バインディングは「データからUI」への一方通行になりがちです。
2. **値の種類**: `DesiredSize`や`ActualWidth`は読み取り専用プロパティであり、直接Bindingで書き戻すことができません。
3. **Canvasの特性**: Canvas内に配置された要素は、自身でサイズを決定（Auto）しますが、それを明示的にデータへ通知する仕組みが必要です。

これらを解決するための現実的なアプローチを2つ紹介します。

---

### 方法1：OneWayToSource バインディングを使う（おすすめ）

`TextBlock` の `ActualWidth` を直接 `Width` プロパティに書き戻すことはできませんが、**`ActualWidth` をソースとして、Data側のプロパティに `OneWayToSource` で流し込む**ことは可能です。

`Generic.xaml` の `DataTemplate` を以下のように修正します。

```xaml
<DataTemplate DataType="{x:Type local:TextBlockData}">
    <TextBlock x:Name="textBlock"
               Text="{Binding Text}" 
               FontSize="{Binding FontSize}" 
               FontFamily="{Binding FontName}"
               local:SizeObserver.Observe="True"
               local:SizeObserver.ObservedWidth="{Binding Width, Mode=OneWayToSource}"
               local:SizeObserver.ObservedHeight="{Binding Height, Mode=OneWayToSource}"/>
</DataTemplate>

```

ただし、`ActualWidth` は依存関係プロパティではないため、標準のBindingでは変更を検知できません。そのため、以下のような「添付プロパティ」を作成して利用するのが一般的です。

#### 補助クラス：SizeObserver.cs

```csharp
public static class SizeObserver
{
    public static readonly DependencyProperty ObserveProperty = DependencyProperty.RegisterAttached(
        "Observe", typeof(bool), typeof(SizeObserver), new FrameworkPropertyMetadata(OnObserveChanged));

    public static readonly DependencyProperty ObservedWidthProperty = DependencyProperty.RegisterAttached(
        "ObservedWidth", typeof(double), typeof(SizeObserver), new FrameworkPropertyMetadata(0.0));

    // Get/Setメソッドは省略 (propaで生成してください)

    private static void OnObserveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FrameworkElement fe && (bool)e.NewValue)
        {
            fe.SizeChanged += (s, args) =>
            {
                SetObservedWidth(fe, fe.ActualWidth);
                SetObservedHeight(fe, fe.ActualHeight);
            };
        }
    }
}

```

---

### 方法2：コードビハインドで SizeChanged をハンドルする

もし「一度だけ取得できればいい」あるいは「添付プロパティは複雑すぎる」と感じる場合は、`TestControl` 自体でサイズ変更を検知し、`Content`（つまり `TextBlockData`）に値を代入します。

`TestControl.cs` に以下を追加します。

```csharp
public class TestControl : ContentControl
{
    // ...既存のコード...

    public TestControl()
    {
        // 自身のサイズが変わったときに中身のデータへ反映する
        this.SizeChanged += (s, e) =>
        {
            if (this.Content is TextBlockData data)
            {
                // ContentPresenter経由で描画された実際のサイズをデータに反映
                data.Width = e.NewSize.Width;
                data.Height = e.NewSize.Height;
            }
        };
    }
}

```

**注意点**:
`Canvas` 内で `TestControl` に `Width`/`Height` を指定していない場合、`TestControl` のサイズは中の `TextBlock` に合わせて広がります。その広がったサイズを `SizeChanged` でキャッチして `MyData.Width` に書き戻すという流れになります。

---

### どちらを使うべきか？

* **MVVMの純粋性を保ちたい場合**: **方法1**。一度 `SizeObserver` を作ってしまえば、他のコントロールでも使い回せます。
* **手っ取り早く動かしたい場合**: **方法2**。`TestControl` という専用のクラスがすでにあるため、そこにロジックを集約するのが簡単です。

実装後、`MainWindow` 右側の `TextBlock` に `MyData.Width` の値が表示されるようになります。

他に、特定のタイミング（ボタンクリック時など）で取得したい、といったご要望はありますか？