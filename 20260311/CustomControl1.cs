using System;
using System.Collections.Generic;
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

namespace _20260311
{
    [ContentProperty(nameof(MyContent))]
    public class CustomThumb : Thumb
    {

        public FrameworkElement MyContent
        {
            get { return (FrameworkElement)GetValue(MyContentProperty); }
            set { SetValue(MyContentProperty, value); }
        }
        public static readonly DependencyProperty MyContentProperty =
            DependencyProperty.Register(nameof(MyContent), typeof(FrameworkElement), typeof(CustomThumb), new PropertyMetadata(null));


        public bool MyIsSelected
        {
            get { return (bool)GetValue(MyIsSelectedProperty); }
            set { SetValue(MyIsSelectedProperty, value); }
        }
        public static readonly DependencyProperty MyIsSelectedProperty =
            DependencyProperty.Register(nameof(MyIsSelected), typeof(bool), typeof(CustomThumb), new PropertyMetadata(false));



        static CustomThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomThumb), new FrameworkPropertyMetadata(typeof(CustomThumb)));
        }
        public CustomThumb()
        {
            DragDelta += TThumb_DragDelta;
            PreviewMouseLeftButtonDown += CustomThumb_PreviewMouseLeftButtonDown;
        }

        private void CustomThumb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var sou = e.Source;
            var ori = e.OriginalSource;
            var dc = this.DataContext;
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
            e.Handled = true;
        }
    }

    public class AAAItemsCtrl : ItemsControl
    {

        static AAAItemsCtrl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AAAItemsCtrl), new FrameworkPropertyMetadata(typeof(AAAItemsCtrl)));
        }
        public AAAItemsCtrl()
        {

        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            var sou = e.Source;
            var ori = e.OriginalSource;

        }
    }

  
}
