using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;


namespace _20260309
{
    public class MyBehavior : Behavior<ContentControl>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            var neko = AssociatedObject;
            var dc = AssociatedObject.DataContext;
        }
    }

    public class MouseOverBehavior : Behavior<Grid>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseEnter += AssociatedObject_MouseEnter;
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
            AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_PreviewMouseLeftButtonDown;

        }

        private void AssociatedObject_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseEnter -= AssociatedObject_MouseEnter;
            AssociatedObject.MouseLeave -= AssociatedObject_MouseLeave;
        }

        private void AssociatedObject_MouseLeave(object sender, MouseEventArgs e)
        {
            AssociatedObject.Background = Brushes.Transparent;
        }

        private void AssociatedObject_MouseEnter(object sender, MouseEventArgs e)
        {
            AssociatedObject.Background = new SolidColorBrush(Color.FromArgb(30, 255, 0, 0));
        }
    }


    internal class Behavior
    {
    }
}
