using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace _20260316
{
    public class TTTextBlock : TextBlock
    {
        private Size _oldDesiredSize;// LayoutUpdate用

        public double MyWidth
        {
            get { return (double)GetValue(MyWidthProperty); }
            set { SetValue(MyWidthProperty, value); }
        }
        public static readonly DependencyProperty MyWidthProperty =
            DependencyProperty.Register(nameof(MyWidth), typeof(double), typeof(TTTextBlock), new PropertyMetadata(0.0));

        public double MyHeight
        {
            get { return (double)GetValue(MyHeightProperty); }
            set { SetValue(MyHeightProperty, value); }
        }
        public static readonly DependencyProperty MyHeightProperty =
            DependencyProperty.Register(nameof(MyHeight), typeof(double), typeof(TTTextBlock), new PropertyMetadata(0.0));

        public TTTextBlock()
        {
            SizeChanged += TTTextBlock_SizeChanged;
            //LayoutUpdated += TTTextBlock_LayoutUpdated;
            // LayoutUpdateでもDesiredSizeの変更が取得できるけど効率は？
        }

        private void TTTextBlock_LayoutUpdated(object? sender, EventArgs e)
        {
            Size newSize = DesiredSize;
            if(newSize != _oldDesiredSize)
            {
                _oldDesiredSize = newSize;
                var neko = DesiredSize;
            }
        }

        private void TTTextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MyWidth = ActualWidth;
            MyHeight= ActualHeight;
        }
        
        
    }


    internal class Class1
    {
    }
}
