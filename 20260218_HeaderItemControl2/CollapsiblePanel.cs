using System;
using System.Collections.Generic;
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

namespace _20260218_HeaderItemControl2
{

    public class CollapsiblePanel : HeaderedItemsControl
    {
        static CollapsiblePanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CollapsiblePanel), new FrameworkPropertyMetadata(typeof(CollapsiblePanel)));
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register(
                nameof(IsExpanded),
                typeof(bool),
                typeof(CollapsiblePanel),
                new PropertyMetadata(true));

        public bool IsExpanded
        {
            get => (bool)GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_Header") is UIElement headerElement)
            {
                headerElement.MouseLeftButtonUp += (s, e) =>
                {
                    IsExpanded = !IsExpanded;
                };
            }
        }

    }
}
