using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace _20260301.Behaviors
{
    public static class EditorBehavior
    {
        // CanvasElementControlなど、クリック対象にセットする添付プロパティ
        public static readonly DependencyProperty IsSelectableProperty =
            DependencyProperty.RegisterAttached("IsSelectable", typeof(bool), typeof(EditorBehavior), new PropertyMetadata(false));

        public static bool GetIsSelectable(DependencyObject obj) => (bool)obj.GetValue(IsSelectableProperty);

        public static void SetIsSelectable(DependencyObject obj, bool value) => obj.SetValue(IsSelectableProperty, value);

    }

}
