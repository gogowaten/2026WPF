using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace _20260212_Behavior2
{
    // 数字入力だけを通す
    public class NumericInputBehavior : Behavior<TextBox>
    {
        #region 依存関係プロパティ
        // 小数点の許可
        public bool AllowDecimal
        {
            get { return (bool)GetValue(AllowDecimalProperty); }
            set { SetValue(AllowDecimalProperty, value); }
        }

        public static readonly DependencyProperty AllowDecimalProperty =
                    DependencyProperty.Register(nameof(AllowDecimal), typeof(bool), typeof(NumericInputBehavior), new PropertyMetadata(false));


        // 入力最大桁数(0は制限無し)
        public int MaxLength
        {
            get { return (int)GetValue(MaxLengthProperty); }
            set { SetValue(MaxLengthProperty, value); }
        }

        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register(nameof(MaxLength), typeof(int), typeof(NumericInputBehavior), new PropertyMetadata(0));

        #endregion 依存関係プロパティ



        // Behavior添付時に各種イベントを購読登録
        protected override void OnAttached()
        {
            base.OnAttached();
            InputMethod.SetIsInputMethodEnabled(AssociatedObject, false); // IME無効化
            AssociatedObject.PreviewTextInput += OnPreviewTextInput; // テキスト入力制限
            AssociatedObject.PreviewKeyDown += OnPreviewKeyDown; // スペースキー入力制限用
            DataObject.AddPastingHandler(AssociatedObject, OnPaste); // 貼り付け制限
            // DataObjectは一言で言えば**「クリップボードやドラッグ＆ドロップでやり取りされるデータの入れ物」**です。
            // DataObject.AddPastingHandlerを使う理由は
            // TextBoxにはPasteというイベントがないからで
            // かわりにDataObject.AddPastingHandlerを使って、指定したメソッドを予約している
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewTextInput -= OnPreviewTextInput;
            AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
            DataObject.RemovePastingHandler(AssociatedObject, OnPaste);
        }

        // ロジック補助
        // 入力後の文字列がどうなるかをシミュレーションする
        private string GetFullTextAfterInput(string inputText)
        {
            string currentText = AssociatedObject.Text;
            int selectionStart = AssociatedObject.SelectionStart;
            int selectionLength = AssociatedObject.SelectionLength;

            // 選択範囲を消して新しい文字を挿入した状態を作る
            return currentText.Remove(selectionStart, selectionLength).Insert(selectionStart, inputText);
        }

        // 文字列が設定条件を見対しているか判定
        private bool IsValid(string text)
        {
            // 文字列は許可 (削除操作などのため)
            if (string.IsNullOrEmpty(text)) return true;

            // 桁数判定
            if (MaxLength > 0 && text.Length > MaxLength) return false;

            // 形式判定 (正規表現)
            string pattern = AllowDecimal ? @"^[0-9]*\.?[0-9]*$" : @"^[0-9]*$";
            return Regex.IsMatch(text, pattern);
        }

        // 数字かどうかを判定する正規表現
        //private static readonly Regex _regex = new("[^0-9]+");

        // 文字入力直前
        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 入力後の全体の文字列を予測
            string fullText = GetFullTextAfterInput(e.Text);

            // 文字列を制限で判定して、
            // 通らなければイベントを処理済みにすることで、入力を破棄する
            e.Handled = !IsValid(fullText);
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // スペースキーはTextCompositionイベントが発生しないため、ここで個別に制限
            if (e.Key == Key.Space) { e.Handled = true; }
        }

        // 貼り付け時の制限
        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                string pasteText = (string)e.DataObject.GetData(DataFormats.Text);
                // もし貼り付けた場合の全体の文字列を取得
                string fullText = GetFullTextAfterInput(pasteText);

                // 制限判定をして通らなければ、貼り付けをキャンセル
                if (!IsValid(fullText)) e.CancelCommand();
            }
            else { e.CancelCommand(); }
        }
    }


    // コントロールの背景色
    public class ColorChangeBehavior : Behavior<Control>
    {
        // 依存関係プロパティの定義 (MouseOverColor)
        public Brush MouseOverColor
        {
            get { return (Brush)GetValue(MouseOverColorProperty); }
            set { SetValue(MouseOverColorProperty, value); }
        }

        public static readonly DependencyProperty MouseOverColorProperty =
            DependencyProperty.Register(nameof(MouseOverColor), typeof(Brush), typeof(ColorChangeBehavior), new PropertyMetadata(Brushes.Transparent));

        // 元の色を保存しておく用
        private Brush? _originalColor;

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

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            // 現在の色を保存してから、設定された色の変える
            _originalColor = AssociatedObject.Background;
            AssociatedObject.Background = MouseOverColor;
        }

        private void OnMouseLeave(object seder, MouseEventArgs e)
        {
            // 元の色に戻す
            AssociatedObject.Background = _originalColor;
        }
    }
}
