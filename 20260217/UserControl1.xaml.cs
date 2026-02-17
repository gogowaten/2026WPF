using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20260217
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class MyNumeUD : UserControl
    {
        public MyNumeUD()
        {
            InitializeComponent();

            // IME無効化
            InputMethod.SetIsInputMethodEnabled(txtValue, false);

            txtValue.Text = Decimal2Text(Value, MyDecimals);

            // 貼り付けイベントのハンドラを登録
            DataObject.AddPastingHandler(txtValue, OnPaste);

            // TextBoxがフォーカス得た瞬間にテキスト全選択にする
            txtValue.GotFocus += (s, e) => { txtValue.SelectAll(); };

        }

        #region 依存関係プロパティ

        #region 要

        // 値
        public decimal Value
        {
            get { return (decimal)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(decimal), typeof(MyNumeUD),
                new FrameworkPropertyMetadata(
                    0m,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnValueChanged, // 値変更直後に実行するメソッド
                    CoerceValue)); // 値の最終判定、強制変更

        // 値変更直後に実行するメソッド
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MyNumeUD)d;
            decimal oldValue = (decimal)e.OldValue;
            decimal newValue = (decimal)e.NewValue;

            //control.txtValue.Text = e.NewValue.ToString();
            //control.txtValue.Text = newValue.ToString("F" + control.MyDecimals);
            control.txtValue.Text = Decimal2Text(newValue, control.MyDecimals);


            //// 値変更ボタンの有効状態を切り替える
            //control.btnUp.IsEnabled = newValue < control.Maximum;
            //control.btnDown.IsEnabled = newValue > control.Minimum;

            //// 独自イベントのValueChandedを発生させる
            //control.RaiseValueChangedEvent(oldValue, newValue);

        }

        // 「強制（CoerceValueCallback）」
        // 入ってきた値を強制的に範囲内に収めて返す
        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            var ctrl = (MyNumeUD)d;
            decimal value = (decimal)baseValue;

            // 数値を範囲に収めるクランプ
            if (value < ctrl.MyMinimum) { return ctrl.MyMinimum; }
            if (value > ctrl.MyMaximum) { return ctrl.MyMaximum; }

            return value;
        }
        #endregion 要

        #region その他依存プロパティ



        public decimal MyMinimum
        {
            get { return (decimal)GetValue(MyMinimumProperty); }
            set { SetValue(MyMinimumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyMinimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyMinimumProperty =
            DependencyProperty.Register(nameof(MyMinimum), typeof(decimal), typeof(MyNumeUD), new PropertyMetadata(-100m));



        public decimal MyMaximum
        {
            get { return (decimal)GetValue(MyMaximumProperty); }
            set { SetValue(MyMaximumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyMuximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyMaximumProperty =
            DependencyProperty.Register(nameof(MyMaximum), typeof(decimal), typeof(MyNumeUD), new PropertyMetadata(100m));



        public decimal MyStep
        {
            get { return (decimal)GetValue(MyStepProperty); }
            set { SetValue(MyStepProperty, value); }
        }

        public static readonly DependencyProperty MyStepProperty =
            DependencyProperty.Register(nameof(MyStep), typeof(decimal), typeof(MyNumeUD), new PropertyMetadata(1m));



        public int MyDecimals
        {
            get { return (int)GetValue(MyDecimalsProperty); }
            set { SetValue(MyDecimalsProperty, value); }
        }

        public static readonly DependencyProperty MyDecimalsProperty =
            DependencyProperty.Register(nameof(MyDecimals), typeof(int), typeof(MyNumeUD), new FrameworkPropertyMetadata(1, OnMyDecimalsChanged));

        private static void OnMyDecimalsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MyNumeUD ud)
            {
                ud.txtValue.Text = Decimal2Text(ud.Value, ud.MyDecimals);
            }
        }

        #endregion その他依存プロパティ

        #region デザイン系


        public TextAlignment MyTextAlignment
        {
            get { return (TextAlignment)GetValue(MyTextAlignmentProperty); }
            set { SetValue(MyTextAlignmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyTextAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyTextAlignmentProperty =
            DependencyProperty.Register(nameof(MyTextAlignment), typeof(TextAlignment), typeof(MyNumeUD), new PropertyMetadata(TextAlignment.Right));


        #endregion デザイン系

        #region その他

        // 左側に表示する文字列 (例：￥)、接頭辞
        public string MyPrefix
        {
            get { return (string)GetValue(MyPrefixProperty); }
            set { SetValue(MyPrefixProperty, value); }
        }

        public static readonly DependencyProperty MyPrefixProperty =
            DependencyProperty.Register(nameof(MyPrefix), typeof(string), typeof(MyNumeUD), new PropertyMetadata(string.Empty));


        // 右側に表示する文字列 (例：°C)、接尾辞
        public string MySuffix
        {
            get { return (string)GetValue(MySuffixProperty); }
            set { SetValue(MySuffixProperty, value); }
        }

        public static readonly DependencyProperty MySuffixProperty =
            DependencyProperty.Register(nameof(MySuffix), typeof(string), typeof(MyNumeUD), new PropertyMetadata(string.Empty));

        #endregion その他
        #endregion 依存関係プロパティ

        #region キー入力イベント


        // キー入力時
        // スペースキー入力を無効化
        private void txtValue_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space) { e.Handled = true; }
        }

        // PreviewTextInput は「これから入力される1文字」しか見ません。そのため、「1-2」のように数字の途中にマイナスを入れられるのを防ぎたい場合は、以下のように「現在のカーソル位置」をチェックします。
        // 文字入力時
        private void txtValue_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string fullText = GetFullTextAfterInput(txtValue, e.Text);
            bool result = IsValid(fullText);
            if (result) { e.Handled = false; } // 通常処理
            else { e.Handled = true; }// 入力文字をキャンセル
        }
        #endregion キー入力イベント

        #region メソッド


        // decimalを指定桁数でstringに変換する。
        // 指定桁数がマイナスのときはそのまま変換する
        private static string Decimal2Text(decimal value, int digits)
        {
            if (digits < 0) { return value.ToString(); }
            else { return value.ToString("F" + digits); }
        }


        // 文字列が設定条件を満たしているかの判定
        private bool IsValid(string text)
        {
            // 文字列
            if (string.IsNullOrEmpty(text)) { return true; }

            // マイナス記号が先頭以外にある
            if (text.Contains('-') && !text.StartsWith('-')) { return false; }

            // "-" か "." が1個より多い場合は通さない
            if (text.Count('-') > 1 || text.Count('.') > 1) { return false; }

            // 最小値、最大値を超えていないかの判定
            //if (result > Maximum || result < Minimum) { return false; }

            // 念の為、
            return Regex.IsMatch(text, @"^[-0-9]*\.?[0-9]*$");
        }

        /// <summary>
        /// TextBoxへの文字入力後の文字列を返す
        /// </summary>
        /// <remarks>TextBox でテキストが選択されていない場合、入力文字列は現在のカーソル位置に挿入されます。
        /// このメソッドは TextBox コントロール自体を変更せず、結果のテキストを文字列として返します。</remarks>
        /// <param name="box">TextBox コントロール。</param>
        /// <param name="input">挿入する文字列。</param>
        /// <returns>入力が適用された後の文字列。</returns>
        private static string GetFullTextAfterInput(TextBox box, string input)
        {
            int ss = box.SelectionStart;
            int sl = box.SelectionLength;
            string cuText = box.Text;
            return cuText.Remove(ss, sl).Insert(ss, input);
        }
        #endregion メソッド

        #region 特殊イベント

        // 貼り付け時の文字列判定
        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                // 貼り付けようとしている文字列チェック、
                // 数字以外に先頭にマイナス記号があってもok、ドットがあってもok
                text = text.Trim('\r', '\n'); // 改行文字削除
                if (!Regex.IsMatch(text, @"^[-0-9]*\.?[0-9]*$"))
                {
                    e.CancelCommand();
                }
            }
            else { e.CancelCommand(); }
        }

        #endregion 特殊イベント

        #region TextBoxでのイベント

        // TextBoxロストフォーカス時
        // 文字列が数値に変換できない場合は、Valueの値を表示する
        private void txtValue_LostFocus(object sender, RoutedEventArgs e)
        {
            // 入力されている文字列をdecimalに変換
            if (decimal.TryParse(txtValue.Text, out decimal dd))
            {
                // 小数表示桁数が指定より大きい場合は四捨五入
                if (MyDecimals < txtValue.Text.Length - txtValue.Text.IndexOf('.') - 1 && MyDecimals >= 0)
                {
                    dd = decimal.Round(dd, MyDecimals, MidpointRounding.AwayFromZero);
                }
                Value = dd;// 値を更新
                txtValue.Text = Decimal2Text(dd, MyDecimals); // Textも更新
            }
            else
            {
                // decimalに変換できない文字列の場合は、Valueを表示して終了
                txtValue.Text = Decimal2Text(Value, MyDecimals);
            }
        }

        private void txtValue_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void txtValue_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!txtValue.IsFocused)
            {
                txtValue.Focus();
                e.Handled = true;
            }
        }

        private void txtValue_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {

        }
        #endregion TextBoxでのイベント

        #region コントロール全体でのイベント

        // マウスホイール回転
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);// 基本クラスのイベントも一応発生させる

            // フォーカスがなければ何もしない
            if (!IsFocused && !IsKeyboardFocusWithin) { return; }

            if (e.Delta > 0) { Value += MyStep; }// 上回転
            if (e.Delta < 0) { Value -= MyStep; }

        }
        #endregion コントロール全体でのイベント
        private void TextBlock_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _ = txtValue.Focus();
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            decimal nValue = Value + MyStep;
            if (nValue > MyMaximum) { nValue = MyMaximum; }
            Value = nValue;
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            decimal nValue = Value - MyStep;
            if (nValue < MyMinimum) { nValue = MyMinimum; }
            Value = nValue;
        }


    }

}
