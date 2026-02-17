using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Automation.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Diagnostics;

namespace _20260213_WpfControlLibrary1
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class MyNumericUpDown : UserControl
    {
        public MyNumericUpDown()
        {
            InitializeComponent();
            txtValue.Text = Value.ToString("F" + Decimals);

            // 貼り付けイベントのハンドラを登録
            DataObject.AddPastingHandler(txtValue, OnPaste);


            // フォーカスを得た瞬間にテキスト全選択にする
            txtValue.GotFocus += (s, e) => txtValue.SelectAll();
            txtValue.PreviewMouseLeftButtonDown += (s, e) =>
            {
                if (!txtValue.IsFocused)
                {
                    txtValue.Focus();
                    e.Handled = true;// クリックでフォーカスを得た瞬間にテキスト全選択にする
                }
            };

            // IME無効化
            InputMethod.SetIsInputMethodEnabled(txtValue, false);

        }

        // 貼り付け時の文字列判定
        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                // 貼り付けようとしている文字列チェック、先頭にマイナス記号があってもokな正規表現
                //if (!Regex.IsMatch(text, "^-[0-9]+$"))

                text = text.Trim('\r', '\n');
                if (!Regex.IsMatch(text, @"^[-0-9]*\.?[0-9]*$"))
                {
                    e.CancelCommand();
                }
            }
            else { e.CancelCommand(); }
        }

        #region 依存関係プロパティ

        #region デザイン

        // ボタンの幅
        public double MyButtonWidth
        {
            get { return (double)GetValue(MyButtonWidthProperty); }
            set { SetValue(MyButtonWidthProperty, value); }
        }

        public static readonly DependencyProperty MyButtonWidthProperty =
                    DependencyProperty.Register(nameof(MyButtonWidth), typeof(double), typeof(MyNumericUpDown), new PropertyMetadata(20.0));




        public Thickness MyBorderThickness
        {
            get { return (Thickness)GetValue(MyBorderThicknessProperty); }
            set { SetValue(MyBorderThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyBorderThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyBorderThicknessProperty =
            DependencyProperty.Register(nameof(MyBorderThickness), typeof(Thickness), typeof(MyNumericUpDown), new PropertyMetadata(new Thickness(1)));




        // 文字色
        public Brush MyFontColor
        {
            get { return (Brush)GetValue(MyFontColorProperty); }
            set { SetValue(MyFontColorProperty, value); }
        }

        public static readonly DependencyProperty MyFontColorProperty =
            DependencyProperty.Register(nameof(MyFontColor), typeof(Brush), typeof(MyNumericUpDown), new PropertyMetadata(Brushes.Black));


        // ボタンの△の色
        public Brush MyMarkerColor
        {
            get { return (Brush)GetValue(MyMarkerColorProperty); }
            set { SetValue(MyMarkerColorProperty, value); }
        }

        public static readonly DependencyProperty MyMarkerColorProperty =
            DependencyProperty.Register(nameof(MyMarkerColor), typeof(Brush), typeof(MyNumericUpDown), new PropertyMetadata(Brushes.Black));


        // 枠の色
        public Brush MyWakuColor
        {
            get { return (Brush)GetValue(MyWakuColorProperty); }
            set { SetValue(MyWakuColorProperty, value); }
        }

        public static readonly DependencyProperty MyWakuColorProperty =
                    DependencyProperty.Register(nameof(MyWakuColor), typeof(Brush), typeof(MyNumericUpDown), new PropertyMetadata(Brushes.Olive));



        // TextBoxのTextAlignmentを外に公開するして使用するために、新設
        public TextAlignment MyTextAlignment
        {
            get { return (TextAlignment)GetValue(MyTextAlignmentProperty); }
            set { SetValue(MyTextAlignmentProperty, value); }
        }

        public static readonly DependencyProperty MyTextAlignmentProperty =
            DependencyProperty.Register(nameof(MyTextAlignment), typeof(TextAlignment), typeof(MyNumericUpDown), new PropertyMetadata(TextAlignment.Right));



        // TextBoxのPaddingプロパティを外に公開して使用するために
        // UserControlのPaddingプロパティを上書き
        // これでUserControlにはPaddingが適用されなくなり、TextBoxにだけ適用される
        public new Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        public new static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register(nameof(Padding), typeof(Thickness), typeof(MyNumericUpDown), new PropertyMetadata(new Thickness(2.0)));

        #endregion デザイン


        // 左側に表示する文字列 (例：￥)、接頭辞
        public string Prefix
        {
            get { return (string)GetValue(PrefixProperty); }
            set { SetValue(PrefixProperty, value); }
        }

        public static readonly DependencyProperty PrefixProperty =
            DependencyProperty.Register(nameof(Prefix), typeof(string), typeof(MyNumericUpDown), new PropertyMetadata(string.Empty));


        // 右側に表示する文字列 (例：°C)、接尾辞
        public string Suffix
        {
            get { return (string)GetValue(SuffixProperty); }
            set { SetValue(SuffixProperty, value); }
        }

        public static readonly DependencyProperty SuffixProperty =
            DependencyProperty.Register(nameof(Suffix), typeof(string), typeof(MyNumericUpDown), new PropertyMetadata(string.Empty));



        // 表示する小数点以下の桁数
        //public int Decimals
        //{
        //    get { return (int)GetValue(DecimalsProperty); }
        //    set { SetValue(DecimalsProperty, value); }
        //}

        //public static readonly DependencyProperty DecimalsProperty =
        //    DependencyProperty.Register(nameof(Decimals), typeof(int), typeof(MyNumericUpDown), new PropertyMetadata(1));

        public int Decimals
        {
            get { return (int)GetValue(DecimalsProperty); }
            set { SetValue(DecimalsProperty, value); }
        }

        public static readonly DependencyProperty DecimalsProperty =
            DependencyProperty.Register(nameof(Decimals), typeof(int), typeof(MyNumericUpDown), new FrameworkPropertyMetadata(1, OnDecimalChanged));

        private static void OnDecimalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MyNumericUpDown nume)
            {
                nume.txtValue.Text = nume.Value.ToString("F" + e.NewValue);
            }
        }

        // 変化量
        public decimal Step
        {
            get { return (decimal)GetValue(StepProperty); }
            set { SetValue(StepProperty, value); }
        }

        public static readonly DependencyProperty StepProperty =
            DependencyProperty.Register(nameof(Step), typeof(decimal), typeof(MyNumericUpDown), new PropertyMetadata(0.1m));


        // 値
        public decimal Value
        {
            get { return (decimal)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(decimal), typeof(MyNumericUpDown),
                new FrameworkPropertyMetadata(
                    0m,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnValueChanged, // 値変更直後に実行するメソッド
                    CoerceValue)); // 値の最終判定、強制変更

        // 値変更直後に実行するメソッド
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MyNumericUpDown)d;
            decimal oldValue = (decimal)e.OldValue;
            decimal newValue = (decimal)e.NewValue;

            //control.txtValue.Text = e.NewValue.ToString();
            control.txtValue.Text = newValue.ToString("F" + control.Decimals);


            // 値変更ボタンの有効状態を切り替える
            control.btnUp.IsEnabled = newValue < control.Maximum;
            control.btnDown.IsEnabled = newValue > control.Minimum;

            // 独自イベントのValueChandedを発生させる
            control.RaiseValueChangedEvent(oldValue, newValue);

        }

        // 「強制（CoerceValueCallback）」
        // 入ってきた値を強制的に範囲内に収めて返す
        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            var ctrl = (MyNumericUpDown)d;
            decimal value = (decimal)baseValue;

            // 数値を範囲に収めるクランプ
            if (value < ctrl.Minimum) { return ctrl.Minimum; }
            if (value > ctrl.Maximum) { return ctrl.Maximum; }
            //return int.Clamp(value, ctrl.Minimum, ctrl.Maximum);
            return value;
        }


        // 最小値
        public decimal Minimum
        {
            get { return (decimal)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(decimal), typeof(MyNumericUpDown), new PropertyMetadata(-10m));


        // 最大値
        public decimal Maximum
        {
            get { return (decimal)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(decimal), typeof(MyNumericUpDown), new PropertyMetadata(10m));

        #endregion 依存関係プロパティ



        #region 独自イベント

        // 独自のイベントを 「ルーティングイベント (RoutedEvent)」 として定義するのが標準的です。これにより、普通の Button の Click イベントと同じように、XAML上で ValueChanged="MyHandler" と書けるようになります。

        // Value変更時に発生させるイベント、これをOnValueChangedメソッドの中で発生させる
        // イベントの登録
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(ValueChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<decimal>),
            typeof(MyNumericUpDown));

        // 外部からハンドラの登録と削除できるようにするプロパティ
        public event RoutedPropertyChangedEventHandler<decimal> ValueChanged
        {
            add => AddHandler(ValueChangedEvent, value);
            remove => RemoveHandler(ValueChangedEvent, value);
        }

        // イベントを発生させるための補助メソッド
        private void RaiseValueChangedEvent(decimal oldValue, decimal newValue)
        {
            var args = new RoutedPropertyChangedEventArgs<decimal>(oldValue, newValue)
            {
                RoutedEvent = ValueChangedEvent
            };
            RaiseEvent(args);
        }

        #endregion 独自イベント



        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            //if (Value < Maximum) { Value++; }
            decimal newValue = Value + Step;
            if (newValue > Maximum) { newValue = Maximum; }
            Value = newValue;
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            //if (Value > Minimum) { Value--; }
            decimal newValue = Value - Step;
            if (newValue < Minimum) { newValue = Minimum; }
            Value = newValue;
        }

        // ユーザーがキーボードで -10 と打とうとしたとき、最初に - を入力した瞬間、TryParse は失敗します。このままだと「不正な入力」として消されてしまう可能性があるため、TextChanged イベントを少し工夫します。
        private void txtValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = txtValue.Text;

            // [-]だけのときは、次に数字が来るのを待つために Value の更新をスキップする
            if (text == "-") { return; }

            //// 入力された文字列を数値に変換
            //if (decimal.TryParse(text, out decimal result)) { Value = result; }
        }

        // PreviewTextInput は「これから入力される1文字」しか見ません。そのため、「1-2」のように数字の途中にマイナスを入れられるのを防ぎたい場合は、以下のように「現在のカーソル位置」をチェックします。
        // TextBoxへの入力時
        private void txtValue_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            //// 入力を許可する文字は、数字、マイナス記号、ドット
            //// その内のマイナス記号とドットは、すでにある場合は許可しない
            //string cha = e.Text;
            //bool isAllowed = Regex.IsMatch(cha, "[0-9.-]");
            //if (cha == "." && txtValue.Text.Contains('.')) { isAllowed = false; }
            //if (cha == "-" && txtValue.Text.Contains('-')) { isAllowed = false; }
            //e.Handled = !isAllowed;

            string cha = e.Text;
            string fullText = GetFullTextAfterInput(txtValue, cha);
            bool result = IsValid(fullText);
            if (result)
            {
                e.Handled = false;
            }
            //e.Handled = !result;
            else { e.Handled = true; }
        }

        // TextBoxへの文字入力後の文字列を返す
        private string GetFullTextAfterInput(TextBox box, string input)
        {
            int ss = box.SelectionStart;
            int sl = box.SelectionLength;
            string cuText = box.Text;
            return cuText.Remove(ss, sl).Insert(ss, input);
        }

        // 文字列が設定条件を満たしているかの判定
        private bool IsValid(string text)
        {
            // 文字列
            if (string.IsNullOrEmpty(text)) { return true; }

            if(text == "-") { return true; }    

            // "-" か "." が1個より多い場合は通さない
            if (text.Count('-') > 1 || text.Count('.') > 1) { return false; }

            // 数値に変換できるかの判定
            if (!decimal.TryParse(text, out decimal result)) { return false; }

            // 最小値、最大値を超えていないかの判定
            if (result > Maximum || result < Minimum) { return false; }

            // 念の為、
            return Regex.IsMatch(text, @"^[-0-9]*\.?[0-9]*$");
        }

        // キー入力直前
        private void txtValue_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // スペースキー入力を無効化
            if (e.Key == Key.Space) { e.Handled = true; }
        }

        // ロストフォーカス時
        private void txtValue_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtValue.Text, out _))
            {
                // 値が数値じゃなければ、今のValueプロパティの数値に入れ替える
                txtValue.Text = Value.ToString("F" + Decimals);
            }
        }

        // コントロール全体でのマウスホイール回転時
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);// 基本クラスのイベントも一応発生させる

            if (!IsFocused && !IsKeyboardFocusWithin) { return; }
            if (e.Delta > 0)
            {
                //if (Value < Maximum) { Value++; }
                decimal newvValue = Value + Step;
                if (newvValue > Maximum) { newvValue = Maximum; }
                Value = newvValue;
            } // 上回転
            if (e.Delta < 0)
            {
                //if (Value > Minimum) { Value--; }
                decimal newvValue = Value - Step;
                if (newvValue < Minimum) { newvValue = Minimum; }
                Value = newvValue;
            }

            // イベントをここで完了させ、親のスクロールなどが動かないようにする
            e.Handled = true;
        }

        // TextBox上でのマウスホイール
        private void txtValue_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // TextBox自身のスクロール動作をキャンセルする
            e.Handled = true;

            // 自作した OnMouseWheel ロジックを再利用するために、新しいイベントとして発生させる
            // あるいは、ここで直接 Value を書き換えてもOKです
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = MouseWheelEvent,
                Source = sender
            };
            //eventArg.RoutedEvent = MouseWheel;
            // UserControl（自分自身）にイベントを投げ直す
            this.RaiseEvent(eventArg);
        }

        private void TextBlock_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _ = txtValue.Focus();
        }
    }

}
