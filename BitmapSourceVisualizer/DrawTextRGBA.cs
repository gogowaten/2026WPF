using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BitmapSourceVisualizer
{
    public enum DrawTextType { None = 0, RGBA, HSVA }
    public abstract class DrawTextOnRender : FrameworkElement
    {
        internal bool CanOnRenderText()
        {
            if (MyLimitDrawScale > MyPixelSize) { return false; }
            if (Visibility == Visibility.Collapsed || Visibility == Visibility.Hidden) { return false; }
            if (MyScroll is null || MyScroll.ViewportWidth == 0 || MyScroll.ViewportHeight == 0) { return false; }
            if (MyBitmapSource is null) { return false; }
            if (MyIsStopDraw) { return false; }

            return true;
        }


        #region 依存関係プロパティ

        public DrawTextType MyDrawTextType
        {
            get { return (DrawTextType)GetValue(MyDrawTextTypeProperty); }
            set { SetValue(MyDrawTextTypeProperty, value); }
        }
        public static readonly DependencyProperty MyDrawTextTypeProperty =
            DependencyProperty.Register(nameof(MyDrawTextType), typeof(DrawTextType), typeof(DrawTextOnRender), new PropertyMetadata(DrawTextType.RGBA, OnMyDrawTextType));
        private static void OnMyDrawTextType(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DrawTextOnRender me)
            {
                me.InvalidateVisual();
            }
        }

        public bool MyIsStopDraw
        {
            get { return (bool)GetValue(MyIsStopDrawProperty); }
            set { SetValue(MyIsStopDrawProperty, value); }
        }
        public static readonly DependencyProperty MyIsStopDrawProperty =
            DependencyProperty.Register(nameof(MyIsStopDraw), typeof(bool), typeof(DrawTextOnRender), new PropertyMetadata(false));



        // 表示するために必要最低限のピクセルの拡大率
        // 目安は50にした、あまり小さくすると文字が読めないし、
        // 表示個数が多すぎて処理が重くなる
        public int MyLimitDrawScale
        {
            get { return (int)GetValue(MyLimitDrawScaleProperty); }
            set { SetValue(MyLimitDrawScaleProperty, value); }
        }
        public static readonly DependencyProperty MyLimitDrawScaleProperty =
            DependencyProperty.Register(nameof(MyLimitDrawScale), typeof(int), typeof(DrawTextOnRender), new PropertyMetadata(50));

        public ScrollViewer MyScroll
        {
            get { return (ScrollViewer)GetValue(MyScrollProperty); }
            set { SetValue(MyScrollProperty, value); }
        }
        public static readonly DependencyProperty MyScrollProperty =
            DependencyProperty.Register(nameof(MyScroll), typeof(ScrollViewer), typeof(DrawTextOnRender), new PropertyMetadata(null));

        public double MyPixelSize
        {
            get { return (double)GetValue(MyPixelSizeProperty); }
            set { SetValue(MyPixelSizeProperty, value); }
        }
        public static readonly DependencyProperty MyPixelSizeProperty =
            DependencyProperty.Register(nameof(MyPixelSize), typeof(double), typeof(DrawTextOnRender), new PropertyMetadata(50.0));

        public Brush MyForeColorBrush
        {
            get { return (Brush)GetValue(MyForeColorBrushProperty); }
            set { SetValue(MyForeColorBrushProperty, value); }
        }
        public static readonly DependencyProperty MyForeColorBrushProperty =
            DependencyProperty.Register(nameof(MyForeColorBrush), typeof(Brush), typeof(DrawTextOnRender),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(200, 255, 255, 255))));

        public Brush MyBackColorBrush
        {
            get { return (Brush)GetValue(MyBackColorBrushProperty); }
            set { SetValue(MyBackColorBrushProperty, value); }
        }
        public static readonly DependencyProperty MyBackColorBrushProperty =
            DependencyProperty.Register(nameof(MyBackColorBrush), typeof(Brush), typeof(DrawTextOnRender),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(200, 0, 0, 0))));


        public BitmapSource MyBitmapSource
        {
            get { return (BitmapSource)GetValue(MyBitmapSourceProperty); }
            set { SetValue(MyBitmapSourceProperty, value); }
        }
        public static readonly DependencyProperty MyBitmapSourceProperty =
            DependencyProperty.Register(nameof(MyBitmapSource), typeof(BitmapSource), typeof(DrawTextOnRender), new PropertyMetadata(null));


        #endregion 依存関係プロパティ

    }

    // ScrollViewerの中でImageコントロールを使って画像を表示しているとき、ピクセルの色ARGB表示するクラス
    // ScrollViewer
    //  ┗ Grid
    //    ┣ Image
    //    ┗ DrawTextRGBA

    // 必要なバインド
    //MyBitmapSource="{Binding MyBitmapSource}"
    //MyScroll="{Binding ElementName=MyScroll}"
    //MyPixelSize="{Binding ImageScale}"/>

    // 必要なイベント時の処理
    // ScrollViewerのScrollChangedを購読、自身のInvalidateVisualを実行して、OnRenderを実行させる
    //private void MyScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
    //{
    //    // 描画更新、OnRenderが実行される
    //    MyDraw.InvalidateVisual();
    //}

    // 描画するのに必要な、画像の拡大率は初期値は50倍にしてある
    //         public int MyLimitDrawScale
    // が、それ



    public class DrawTextRGBA : DrawTextOnRender
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (CanOnRenderText() == false) { return; }


            // 1. Imageコントロールの現在の拡大サイズと位置を取得
            // 2. スクロール領域から「今見えているピクセル範囲」を計算
            int startX = (int)(MyScroll.HorizontalOffset / MyPixelSize);
            int endX = (int)((MyScroll.HorizontalOffset + MyScroll.ViewportWidth) / MyPixelSize) + 1;
            if (endX > MyBitmapSource.PixelWidth) { endX = MyBitmapSource.PixelWidth; }
            int startY = (int)(MyScroll.VerticalOffset / MyPixelSize);
            int endY = (int)((MyScroll.VerticalOffset + MyScroll.ViewportHeight) / MyPixelSize) + 1;
            if (endY > MyBitmapSource.PixelHeight) { endY = MyBitmapSource.PixelHeight; }



            Typeface typeface = new("ＭＳ ゴシック");
            double emSize = MyPixelSize / 5.0; // フォントの描画サイズ？
            double pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

            // テキスト描画
            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    CroppedBitmap crop = new(MyBitmapSource, new Int32Rect(x, y, 1, 1));
                    byte[] pixels = new byte[4];
                    crop.CopyPixels(pixels, 4, 0); // Bgra32前提なのでstrigeは4で決め打ち
                    string argbText = $"R {pixels[2]}\nG {pixels[1]}\nB {pixels[0]}\nA {pixels[3]}";
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

                    // 同じ文字列を違う色で1ドットずらして描画
                    formattedText.SetForegroundBrush(MyForeColorBrush);
                    textPos = new(x * MyPixelSize + 1, y * MyPixelSize + 1);
                    drawingContext.DrawText(formattedText, textPos);
                }
            }

        }

    }

    public class DrawTextPixelColorValue : DrawTextOnRender
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (CanOnRenderText() == false) { return; }
            if(MyDrawTextType == DrawTextType.None) { return; }

            // 1. Imageコントロールの現在の拡大サイズと位置を取得
            // 2. スクロール領域から「今見えているピクセル範囲」を計算
            int startX = (int)(MyScroll.HorizontalOffset / MyPixelSize);
            int endX = (int)((MyScroll.HorizontalOffset + MyScroll.ViewportWidth) / MyPixelSize) + 1;
            if (endX > MyBitmapSource.PixelWidth) { endX = MyBitmapSource.PixelWidth; }
            int startY = (int)(MyScroll.VerticalOffset / MyPixelSize);
            int endY = (int)((MyScroll.VerticalOffset + MyScroll.ViewportHeight) / MyPixelSize) + 1;
            if (endY > MyBitmapSource.PixelHeight) { endY = MyBitmapSource.PixelHeight; }



            Typeface typeface = new("ＭＳ ゴシック");
            double emSize = MyPixelSize / 5.0; // フォントの描画サイズ？
            double pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

            // テキスト描画
            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    CroppedBitmap crop = new(MyBitmapSource, new Int32Rect(x, y, 1, 1));
                    byte[] pixels = new byte[4];
                    crop.CopyPixels(pixels, 4, 0); // Bgra32前提なのでstrigeは4で決め打ち
                    string text = "";
                    if(MyDrawTextType == DrawTextType.RGBA)
                    {
                        text = $"R {pixels[2]}\nG {pixels[1]}\nB {pixels[0]}\nA {pixels[3]}";
                    }
                    else if(MyDrawTextType == DrawTextType.HSVA)
                    {
                        double a = pixels[3] / 255.0;
                        (double h, double s, double v) = MathHSV.Rgb2hsv(pixels[2], pixels[1], pixels[0]);
                        text = $"H {h:F0}\nS {s:P0}\nV {v:P0}\nA {a:P0}";
                    }
                    
                    FormattedText formattedText = new(
                        text,
                        CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        emSize,
                        MyBackColorBrush,
                        pixelsPerDip);
                    Point textPos = new(x * MyPixelSize + 2, y * MyPixelSize + 2);
                    drawingContext.DrawText(formattedText, textPos);

                    // 同じ文字列を違う色で1ドットずらして描画
                    formattedText.SetForegroundBrush(MyForeColorBrush);
                    textPos = new(x * MyPixelSize + 1, y * MyPixelSize + 1);
                    drawingContext.DrawText(formattedText, textPos);
                }
            }

        }

    }


}
