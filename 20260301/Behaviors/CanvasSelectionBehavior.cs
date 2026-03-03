using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace _20260301.Behaviors
{

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
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
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

            if (selectableElement != null && selectableElement.DataContext is Data clickedData)
            {
                bool isCtrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
                Service.Select(clickedData, isCtrl);
                e.Handled = true;
                AssociatedObject.Focus();
            }
            else
            {  
                Service.ClearSelection();
            }
        }


        //private void OnMouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (Service == null) { return; }

        //    // 添付プロパティのIsSelectableを持つ要素を親方向に遡って探す
        //    DependencyObject? d = e.OriginalSource as DependencyObject;
        //    FrameworkElement? selectableElement = null;

        //    while (d != null && d != AssociatedObject)
        //    {
        //        if (d is FrameworkElement fe && EditorBehavior.GetIsSelectable(fe))
        //        {
        //            selectableElement = fe;
        //            break;
        //        }
        //        d = VisualTreeHelper.GetParent(d);
        //    }

        //    if (selectableElement != null && selectableElement.DataContext is Data clickedData)
        //    {
        //        bool isCtrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        //        Service.Select(clickedData, isCtrl);
        //        e.Handled = true;
        //        AssociatedObject.Focus();
        //    }
        //    else { Service.ClearSelection(); }
        //}



    }

}
