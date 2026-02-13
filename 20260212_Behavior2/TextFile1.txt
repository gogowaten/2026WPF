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