using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20260309
{
    [ContentProperty(nameof(MyContent))]
    public class TThumb : Thumb
    {

        public FrameworkElement MyContent
        {
            get { return (FrameworkElement)GetValue(MyContentProperty); }
            set { SetValue(MyContentProperty, value); }
        }
        public static readonly DependencyProperty MyContentProperty =
            DependencyProperty.Register(nameof(MyContent), typeof(FrameworkElement), typeof(TThumb), new PropertyMetadata(null));


        public bool MyIsSelected
        {
            get { return (bool)GetValue(MyIsSelectedProperty); }
            set { SetValue(MyIsSelectedProperty, value); }
        }
        public static readonly DependencyProperty MyIsSelectedProperty =
            DependencyProperty.Register(nameof(MyIsSelected), typeof(bool), typeof(TThumb), new PropertyMetadata(false));



        static TThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TThumb), new FrameworkPropertyMetadata(typeof(TThumb)));
        }
        public TThumb()
        {
            DragDelta += TThumb_DragDelta;
        }

        private void TThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var left = Canvas.GetLeft(this);
            Canvas.SetLeft(this, Canvas.GetLeft(this) + e.HorizontalChange);
            Canvas.SetTop(this, Canvas.GetTop(this) + e.VerticalChange);
            var left2 = Canvas.GetLeft(this);
            if (MyIsSelected)
            {
            }
        }
    }

    public class AAA : ItemsControl
    {

        static AAA()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AAA), new FrameworkPropertyMetadata(typeof(AAA)));
        }
        public AAA()
        {

        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            var sou = e.Source;
            var ori = e.OriginalSource;

        }
    }

    public class CustomControl1 : Control
    {
        static CustomControl1()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomControl1), new FrameworkPropertyMetadata(typeof(CustomControl1)));
        }
    }
}
