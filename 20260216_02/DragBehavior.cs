using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace _20260216_02
{
    public static class DragBehavior
    {
        public static bool GetIsEnabled(UIElement element) => (bool)element.GetValue(IsEnabledProperty);

        public static void SetIsEnabled(UIElement element, bool value) => element.SetValue(IsEnabledProperty, value);

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(DragBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if ((bool)e.NewValue)
                {
                    element.PreviewMouseLeftButtonDown += OnMouseDown;
                    element.PreviewMouseMove += OnMouseMove;
                    element.PreviewMouseLeftButtonUp += OnMouseUp;
                }
                else
                {
                    element.PreviewMouseLeftButtonDown -= OnMouseDown;
                    element.PreviewMouseMove -= OnMouseMove;
                    element.PreviewMouseLeftButtonUp -= OnMouseUp;
                }
            }
        }

        private static bool _isDragging;
        private static Point _startMousePos;
        private static Point _startElementPos;

        // 親パネル取得
        private static Panel? GetParentPanal(UIElement element)
        {
            DependencyObject parent = element;
            while (parent != null)
            {
                if (parent is Panel p) { return p; }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        private static void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is UIElement element)
            {
                var panel = GetParentPanal(element);
                if (panel == null) { return; }

                _isDragging = true;
                _startMousePos = e.GetPosition(panel);
                _startElementPos = NodeProps.GetPosition(element);

                element.CaptureMouse();
            }
        }

        private static void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && sender is UIElement element)
            {
                Panel? panel = GetParentPanal(element);
                if (panel == null) { return; }

                Point currentMousePos = e.GetPosition(panel);
                Vector delta = currentMousePos - _startMousePos;

                Point newPos = _startElementPos + delta;

                // AttachedPropertyを更新 (UIのレイアウト用)
                NodeProps.SetPosition(element, newPos);

                // ViewModelを更新
                if (element is FrameworkElement fe && fe.DataContext is NodeViewModel vm)
                {
                    vm.X = newPos.X;
                    vm.Y = newPos.Y;
                }

                //NodeProps.SetPosition(element, _startElementPos + delta);
            }
        }

        private static void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is UIElement element)
            {
                _isDragging = false;
                element.ReleaseMouseCapture();
            }
        }
    }

}
