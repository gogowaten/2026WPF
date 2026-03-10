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
        
        public UIElement MyContent
        {
            get { return (UIElement)GetValue(MyContentProperty); }
            set { SetValue(MyContentProperty, value); }
        }
        public static readonly DependencyProperty MyContentProperty =
            DependencyProperty.Register(nameof(MyContent), typeof(UIElement), typeof(TThumb), new PropertyMetadata(null));

        static TThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TThumb), new FrameworkPropertyMetadata(typeof(TThumb)));
        }
        public TThumb()
        {

        }

        //public override void OnApplyTemplate()
        //{
        //    base.OnApplyTemplate();
        //    if(GetTemplateChild("PART_Content") is ContentControl cc)
        //    {
        //        cc.Content = MyContent;
        //    }
        //}
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
