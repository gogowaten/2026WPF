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
            if (MyBitmapSource is null) { return; }

            // 表示されているピクセルの範囲測定、どのピクセルからどのピクセルまで
            int startX = (int)(MyScroll.HorizontalOffset / MyPixelSize);
            int endX = (int)((MyScroll.HorizontalOffset + MyScroll.ViewportWidth) / MyPixelSize) + 1;
            if (endX > MyBitmapSource.PixelWidth) { endX = MyBitmapSource.PixelWidth; }
            int startY = (int)(MyScroll.VerticalOffset / MyPixelSize);
            int endY = (int)((MyScroll.VerticalOffset + MyScroll.ViewportHeight) / MyPixelSize) + 1;
            if (endY > MyBitmapSource.PixelHeight) { endY = MyBitmapSource.PixelHeight; }

            Typeface typeface = new Typeface("ＭＳ ゴシック");
            double emSize = MyPixelSize / 5.0; // フォントの描画サイズ？
            double pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    CroppedBitmap crop = new(MyBitmapSource, new Int32Rect(x, y, 1, 1));
                    byte[] pixels = new byte[4];
                    crop.CopyPixels(pixels, 4, 0);
                    string argbText = $"A {pixels[3]}\nR {pixels[2]}\nG {pixels[1]}\nB {pixels[0]}\n";
                    FormattedText formattedText = new(
                        argbText,
                        CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        emSize,
                        MyBackColorBrush,
                        pixelsPerDip);
                    Point textPos = new(x * MyPixelSize + 2, y * MyPixelSize + 2);
                    drawingContext.DrawText(formattedText, textPos);

                    formattedText.SetForegroundBrush(MyForeColorBrush);
                    textPos = new(x * MyPixelSize + 1, y * MyPixelSize + 1);
                    drawingContext.DrawText(formattedText, textPos);
                }
            }
        }


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
            DependencyProperty.Register(nameof(MyForeColorBrush), typeof(Brush), typeof(DrawTextARGB),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(200, 255, 255, 255))));

        public Brush MyBackColorBrush
        {
            get { return (Brush)GetValue(MyBackColorBrushProperty); }
            set { SetValue(MyBackColorBrushProperty, value); }
        }
        public static readonly DependencyProperty MyBackColorBrushProperty =
            DependencyProperty.Register(nameof(MyBackColorBrush), typeof(Brush), typeof(DrawTextARGB),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(200, 0, 0, 0))));

        //public double MyEmSize
        //{
        //    get { return (double)GetValue(MyEmSizeProperty); }
        //    set { SetValue(MyEmSizeProperty, value); }
        //}
        //public static readonly DependencyProperty MyEmSizeProperty =
        //    DependencyProperty.Register(nameof(MyEmSize), typeof(double), typeof(DrawTextARGB), new PropertyMetadata(10.0));



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



        //public Rect MyDrawBounds
        //{
        //    get { return (Rect)GetValue(MyDrawBoundsProperty); }
        //    set { SetValue(MyDrawBoundsProperty, value); }
        //}
        //public static readonly DependencyProperty MyDrawBoundsProperty =
        //    DependencyProperty.Register(nameof(MyDrawBounds), typeof(Rect), typeof(DrawTextARGB), new PropertyMetadata(Rect.Empty));




    }
}
