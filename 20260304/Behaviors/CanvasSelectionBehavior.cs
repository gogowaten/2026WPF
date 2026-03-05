using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace _20260304.Behaviors
{
    // CanvasElementControl
    public class TestBehavior : Behavior<CanvasElementControl>
    {

        public EditorService Service
        {
            get { return (EditorService)GetValue(ServiceProperty); }
            set { SetValue(ServiceProperty, value); }
        }
        public static readonly DependencyProperty ServiceProperty =
            DependencyProperty.Register(nameof(Service), typeof(EditorService), typeof(TestBehavior));

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var ori = e.OriginalSource;
            var sor = e.Source;
        }
    }
    // ItemsControl専用のBehavior

    public class CanvasSelectionBehavior : Behavior<ItemsControl>
    {
        // 外部から EditorServiceを注入できるように依存関係プロパティとして定義
        public EditorService Service
        {
            get { return (EditorService)GetValue(ServiceProperty); }
            set { SetValue(ServiceProperty, value); }
        }
        public static readonly DependencyProperty ServiceProperty =
            DependencyProperty.Register(nameof(Service), typeof(EditorService), typeof(CanvasSelectionBehavior));


        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            AssociatedObject.KeyDown += OnKeyDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            AssociatedObject.KeyDown -= OnKeyDown;
        }



        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (Service is null) { return; }

            switch (e.Key)
            {
                case Key.F2:
                    if (Service.ActiveItem is GroupData group)
                    {
                        Service.EnterGroup(group);
                        e.Handled = true;
                    }
                    break;
                case Key.Escape:
                    if (Service.SelectedItems.Any())
                    {
                        Service.ClearSelection();
                    }
                    else { Service.EscapeGroup(); }
                    e.Handled = true;
                    break;
                case Key.A:
                    // Ctrl + A ：現在の階層の全選択
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        SelectAllCurrentLayer();
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void SelectAllCurrentLayer()
        {
            // 現在のEditingGroupに属する要素をすべて選択
            // ※ItemsControlのItemsSourceから現在の階層のDataを抽出してService経由で選択

        }


        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Service == null) { return; }

            // 添付プロパティのIsSelectableを持つ要素を親方向に遡って探す
            DependencyObject? d = e.OriginalSource as DependencyObject;
            FrameworkElement? selectableElement = null;

            while (d != null && d != AssociatedObject)
            {
                if (d is FrameworkElement fe && EditorBehavior.GetIsSelectable(fe))
                {
                    selectableElement = fe;
                    break;
                }
                d = VisualTreeHelper.GetParent(d);
            }

            // 要素が見つかった場合
            if (selectableElement != null && selectableElement.DataContext is Data clickedData)
            {
                // ダブルクリック判定
                if (e.ClickCount == 2 && clickedData is GroupData group)
                {
                    Service.EnterGroup(group);
                    e.Handled = true;
                    return;
                }

                // 選択状態を更新
                bool isCtrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
                Service.Select(clickedData, isCtrl);
                e.Handled = true;
                AssociatedObject.Focus();
            }
            else
            {
                // 背景クリックで選択解除、ダブルクリックで上の階層へ
                if (e.ClickCount == 2) { Service.EscapeGroup(); }
                else { Service.ClearSelection(); }

            }
        }




    }
}
