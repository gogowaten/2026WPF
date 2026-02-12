using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace _20260212_Behavior2
{
    public class NumericInputBehavior : Behavior<TextBox>
    {
        // Behavior添付時に各種イベントを購読登録
        protected override void OnAttached()
        {
            base.OnAttached();
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

        // 数字かどうかを判定する正規表現
        private static readonly Regex _regex = new("[^0-9]+");

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 入力された文字が数字以外なら、イベントを処理済みにして入力を破棄する
            e.Handled = _regex.IsMatch(e.Text);
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
                string text = (string)e.DataObject.GetData(DataFormats.Text);
                // 貼り付けようとしている文字列に数字以外が含まれていればキャンセル
                if (_regex.IsMatch(text)) { e.CancelCommand(); }
            }
            else { e.CancelCommand(); }
        }
    }



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
