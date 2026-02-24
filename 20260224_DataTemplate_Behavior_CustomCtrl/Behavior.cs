using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace _20260224_DataTemplate_Behavior_CustomCtrl
{
    public class Behavior { }

    public class MouseDragBehavior : Behavior<FrameworkElement>
    {
        private bool _isDragging;
        private Point _startMousePos;
        private double _startItemX, _startItemY;

        #region 依存関係プロパティ

        // ドラッグ移動の有効無効の切り替え用
        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(MouseDragBehavior), new PropertyMetadata(true));
        #endregion 依存関係プロパティ

        // Behaviorを要素にアタッチした時の処理
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseDown += AssociatedObject_MouseDown;
            AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            AssociatedObject.MouseUp += AssociatedObject_MouseUp;
        }

        // Behaviorを要素から解除した時の処理
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
            AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
            AssociatedObject.MouseUp -= AssociatedObject_MouseUp;
        }

        private void AssociatedObject_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            AssociatedObject.ReleaseMouseCapture();
        }

        private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !IsEnabled) { return; }

            if (AssociatedObject.DataContext is Item item)
            {
                var currentPos = e.GetPosition(Application.Current.MainWindow);
                var diff = currentPos - _startMousePos;

                item.X = _startItemX + diff.X;
                item.Y = _startItemY + diff.Y;
            }
        }

        private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled) { return; }

            if (AssociatedObject.DataContext is Item item)
            {
                _isDragging = true;
                _startMousePos = e.GetPosition(Application.Current.MainWindow); //
                _startItemX = item.X;
                _startItemY = item.Y;

                AssociatedObject.CaptureMouse();
                e.Handled = true;
            }
        }
    }
}
