using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace _20260621
{
    public class DrawTextARGB : FrameworkElement
    {

        public DrawTextARGB()
        {

        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (MyScroll is null || MyScroll.ViewportWidth == 0 || MyScroll.ViewportHeight == 0) { return; }

            //if (Rect.Empty == MyDrawBounds) { return; }
            if (MyBitmapSource is null) { return; }

            var vw = MyScroll.ViewportWidth;
            var vh = MyScroll.ViewportHeight;
            int startX = (int)(MyScroll.HorizontalOffset / MyPixelSize);
            int endX = (int)((MyScroll.HorizontalOffset + MyScroll.ViewportWidth) / MyPixelSize) + 1;
            int startY = (int)(MyScroll.VerticalOffset / MyPixelSize);
            int endY = (int)((MyScroll.VerticalOffset + MyScroll.ViewportHeight) / MyPixelSize) + 1;

            Int32Rect cropRect = new(0, 0, 1, 1);
            double emSize = MyPixelSize / 5.0;

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    CroppedBitmap crop = new(MyBitmapSource, new Int32Rect(x,y,1,1));
                    byte[] pixels = new byte[40];
                    crop.CopyPixels(pixels, 4, 0);
                    string argbText = $"A {pixels[3]}\nR {pixels[2]}\nG {pixels[1]}\nB {pixels[0]}\n";
                    //string argbText = $"A 255\nR 255\nG 255\nB 255\n";
                    FormattedText formattedText = new(
                        argbText,
                        CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("ＭＳ ゴシック"),
                        emSize,
                        MyForeColorBrush,
                        pixelsPerDip: VisualTreeHelper.GetDpi(this).PixelsPerDip);

                    Point textPos = new(x * MyPixelSize + 2, y * MyPixelSize + 2);
                    drawingContext.DrawText(formattedText, textPos);

                }
            }
        }

        //protected override void OnRender(DrawingContext drawingContext)
        //{
        //    base.OnRender(drawingContext);

        //    for (int y = 0; y < 5; y++)
        //    {
        //        for (int x = 0; x < 5; x++)
        //        {
        //            string argbText = $"A 255\nR 255\nG 255\nB 255\n";
        //            FormattedText formattedText = new(
        //                argbText,
        //                CultureInfo.InvariantCulture,
        //                FlowDirection.LeftToRight,
        //                new Typeface("ＭＳ ゴシック"),
        //                7.5,
        //                Brushes.Black,
        //                pixelsPerDip: VisualTreeHelper.GetDpi(this).PixelsPerDip);

        //            Point textPos = new(x * 30 + 2, y * 30 + 2);
        //            drawingContext.DrawText(formattedText, textPos);

        //        }
        //    }
        //}


        public Image MyImage
        {
            get { return (Image)GetValue(MyImageProperty); }
            set { SetValue(MyImageProperty, value); }
        }
        public static readonly DependencyProperty MyImageProperty =
            DependencyProperty.Register(nameof(MyImage), typeof(Image), typeof(DrawTextARGB), new PropertyMetadata(null));


        public ScrollViewer MyScroll
        {
            get { return (ScrollViewer)GetValue(MyScrollProperty); }
            set { SetValue(MyScrollProperty, value); }
        }
        public static readonly DependencyProperty MyScrollProperty =
            DependencyProperty.Register(nameof(MyScroll), typeof(ScrollViewer), typeof(DrawTextARGB), new PropertyMetadata(null));

        public double MyPixelSize
        {
            get { return (double)GetValue(MyPixelSizeProperty); }
            set { SetValue(MyPixelSizeProperty, value); }
        }
        public static readonly DependencyProperty MyPixelSizeProperty =
            DependencyProperty.Register(nameof(MyPixelSize), typeof(double), typeof(DrawTextARGB), new PropertyMetadata(50.0));

        public Brush MyForeColorBrush
        {
            get { return (Brush)GetValue(MyForeColorBrushProperty); }
            set { SetValue(MyForeColorBrushProperty, value); }
        }
        public static readonly DependencyProperty MyForeColorBrushProperty =
            DependencyProperty.Register(nameof(MyForeColorBrush), typeof(Brush), typeof(DrawTextARGB), new PropertyMetadata(Brushes.Gray));

        public double MyEmSize
        {
            get { return (double)GetValue(MyEmSizeProperty); }
            set { SetValue(MyEmSizeProperty, value); }
        }
        public static readonly DependencyProperty MyEmSizeProperty =
            DependencyProperty.Register(nameof(MyEmSize), typeof(double), typeof(DrawTextARGB), new PropertyMetadata(10.0));



        //public ImageSource MyImageSource
        //{
        //    get { return (ImageSource)GetValue(MyImageSourceProperty); }
        //    set { SetValue(MyImageSourceProperty, value); }
        //}
        //public static readonly DependencyProperty MyImageSourceProperty =
        //    DependencyProperty.Register(nameof(MyImageSource), typeof(ImageSource), typeof(DrawTextARGB), new PropertyMetadata(null));

        public BitmapSource MyBitmapSource
        {
            get { return (BitmapSource)GetValue(MyBitmapSourceProperty); }
            set { SetValue(MyBitmapSourceProperty, value); }
        }
        public static readonly DependencyProperty MyBitmapSourceProperty =
            DependencyProperty.Register(nameof(MyBitmapSource), typeof(BitmapSource), typeof(DrawTextARGB), new PropertyMetadata(null));



        public Rect MyDrawBounds
        {
            get { return (Rect)GetValue(MyDrawBoundsProperty); }
            set { SetValue(MyDrawBoundsProperty, value); }
        }
        public static readonly DependencyProperty MyDrawBoundsProperty =
            DependencyProperty.Register(nameof(MyDrawBounds), typeof(Rect), typeof(DrawTextARGB), new PropertyMetadata(Rect.Empty));




    }
}
