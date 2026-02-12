using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace _20260211
{
    class Class1
    {
    }

    public class CanvasClickBehavior : Behavior<FrameworkElement>
    {
        // コマンドを受け取るための依存関係プロパティ
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(CanvasClickBehavior));

        public ICommand Command { get => (ICommand)GetValue(CommandProperty); set => SetValue(CommandProperty, value); }

        protected override void OnAttached()
        {
            // ここで直接イベントを購読。文字列を使わないのでタイポしない！
            AssociatedObject.MouseLeftButtonDown += OnMouseDown;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseLeftButtonDown -= OnMouseDown;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Command?.CanExecute(e) == true) Command.Execute(e);
        }
    }

}
