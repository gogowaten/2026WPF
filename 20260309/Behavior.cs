using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace _20260309
{

    public class MouseOverBehavior : Behavior<Grid>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseEnter += AssociatedObject_MouseEnter;
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
        }
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseEnter -= AssociatedObject_MouseEnter;
            AssociatedObject.MouseLeave -= AssociatedObject_MouseLeave;
        }

        private void AssociatedObject_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            AssociatedObject.Background = Brushes.Transparent;
        }

        private void AssociatedObject_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            AssociatedObject.Background = new SolidColorBrush(Color.FromArgb(30, 255, 0, 0));
        }
    }


    internal class Behavior
    {
    }
}
