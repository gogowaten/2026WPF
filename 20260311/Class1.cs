using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace _20260311
{
    public class TTTextBlock : TextBlock
    {

        public Size MySize
        {
            get { return (Size)GetValue(MySizeProperty); }
            set { SetValue(MySizeProperty, value); }
        }
        public static readonly DependencyProperty MySizeProperty =
            DependencyProperty.Register(nameof(MySize), typeof(Size), typeof(TTTextBlock), new PropertyMetadata(new Size()));

        public TTTextBlock()
        {
            //SizeChanged += TTTextBlock_SizeChanged;
            
        }

        public void SetSize()
        {
            this.Width = DesiredSize.Width;
            this.Height = DesiredSize.Height;
        }
       
        private void TTTextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var wi = ActualWidth;
            var he = ActualHeight;
            if(this.DataContext is TextData data)
            {
               var  sou = e.Source;
                var ori = e.OriginalSource;
                var ss = e.WidthChanged;
                var hh = e.HeightChanged;
                var ns = e.NewSize;
                var de = this.DesiredSize;

                data.Width = DesiredSize.Width;
                data.Height= DesiredSize.Height;
                e.Handled = true;
            }
        }


        
        
    }
    internal class Class1
    {
    }
}
