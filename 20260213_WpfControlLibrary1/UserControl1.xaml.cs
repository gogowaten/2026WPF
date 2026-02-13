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
            txtValue.Text = Value.ToString();

            // 貼り付けイベントのハンドラを登録
            DataObject.AddPastingHandler(txtValue, OnPaste);
        }

        // 貼り付け時の文字列判定
        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                // 貼り付けようとしている文字列が数字だけでなければ、貼り付けをキャンセル
                if (!Regex.IsMatch(text, "^[0-9]+$"))
                {
                    e.CancelCommand();
                }
            }
            else { e.CancelCommand(); }
        }

        #region 依存関係プロパティ





        public Brush MyFontColor
        {
            get { return (Brush)GetValue(MyFontColorProperty); }
            set { SetValue(MyFontColorProperty, value); }
        }

        public static readonly DependencyProperty MyFontColorProperty =
            DependencyProperty.Register(nameof(MyFontColor), typeof(Brush), typeof(MyNumericUpDown), new PropertyMetadata(Brushes.Black));



        public Brush MyMarkerColor
        {
            get { return (Brush)GetValue(MyMarkerColorProperty); }
            set { SetValue(MyMarkerColorProperty, value); }
        }

        public static readonly DependencyProperty MyMarkerColorProperty =
            DependencyProperty.Register(nameof(MyMarkerColor), typeof(Brush), typeof(MyNumericUpDown), new PropertyMetadata(Brushes.Black));



        public Brush MyWakuColor
        {
            get { return (Brush)GetValue(MyWakuColorProperty); }
            set { SetValue(MyWakuColorProperty, value); }
        }

        public static readonly DependencyProperty MyWakuColorProperty =
                    DependencyProperty.Register(nameof(MyWakuColor), typeof(Brush), typeof(MyNumericUpDown), new PropertyMetadata(Brushes.Red));



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




        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(int), typeof(MyNumericUpDown),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnValueChanged, // 値変更直後に実行するメソッド
                    CoerceValue)); // 値の最終判定、強制変更

        // 値変更直後に実行するメソッド
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MyNumericUpDown)d;
            control.txtValue.Text = e.NewValue.ToString();

            // 値変更ボタンの有効状態を切り替える
            control.btnUp.IsEnabled = (int)e.NewValue < control.Maximum;
            control.btnDown.IsEnabled = (int)e.NewValue > control.Minimum;
        }

        // 「強制（CoerceValueCallback）」
        // 入ってきた値を強制的に範囲内に収めて返す
        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            var ctrl = (MyNumericUpDown)d;
            int value = (int)baseValue;

            // 数値を範囲に収めるクランプ
            if (value < ctrl.Minimum) { return ctrl.Minimum; }
            if (value > ctrl.Maximum) { return ctrl.Maximum; }
            //return int.Clamp(value, ctrl.Minimum, ctrl.Maximum);
            return value;
        }


        // 最小値
        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(int), typeof(MyNumericUpDown), new PropertyMetadata(0));


        // 最大値
        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(int), typeof(MyNumericUpDown), new PropertyMetadata(100));

        #endregion 依存関係プロパティ



        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            if (Value < Maximum) { Value++; }
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            if (Value > Minimum) { Value--; }
        }

        private void txtValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 入力された文字列を数値に変換
            if (int.TryParse(txtValue.Text, out int result)) { Value = result; }
        }

        private void txtValue_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //// 数値以外を入力できないように
            //e.Handled = !int.TryParse(e.Text, out _);

            // 数字以外なら入力をキャンセル
            e.Handled = !Regex.IsMatch(e.Text, "[0-9]");
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
                txtValue.Text = Value.ToString();
            }
        }

        // コントロール全体でのマウスホイール回転時
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);// 基本クラスのイベントも一応発生させる

            if (!IsFocused && !IsKeyboardFocusWithin) { return; }
            if (e.Delta > 0) { if (Value < Maximum) { Value++; } } // 上回転
            if (e.Delta < 0) { if (Value > Minimum) { Value--; } }

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
    }

}
