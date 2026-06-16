using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;

// Visual Studio用、BitmapSourceVisualizerに「ファイルに保存」と「コピー」を追加した - 午後わてんのブログ
// https://gogowaten.hatenablog.com/entry/2026/05/20/233726

namespace BitmapSourceVisualizer
{
    /// <summary>
    /// BitmapSourceVisualizerWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class BitmapSourceVisualizerWindow : Window
    {
        private Point MyPoint; // マウスドラッグ移動処理で使う
        public BitmapSource MyBitmapSource;
        private readonly double ImageScaleMin = 0.01; // 拡大率下限
        private readonly double ImageScaleMax = 50.0; // 拡大率上限
        private double MyPreHScroll; // スクロール位置の記録用
        private double MyPreVScroll;


        // ネイティブオブジェクト解放用のAPI
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);


        public BitmapSourceVisualizerWindow()
        {
            InitializeComponent();
            ImageControl.ContextMenu = CreateContextMenu();
            DataContext = this;
            MyTextBlockScale.FontSize = this.FontSize * 1.5;
            IsBackground.FontSize = this.FontSize * 1.5;
        }


        #region 依存関係プロパティ



        // 取得した色の表示用（マウスカーソル位置のピクセルの色）
        public SolidColorBrush MySolidColorBrush
        {
            get { return (SolidColorBrush)GetValue(MySolidColorBrushProperty); }
            set { SetValue(MySolidColorBrushProperty, value); }
        }
        public static readonly DependencyProperty MySolidColorBrushProperty =
            DependencyProperty.Register(nameof(MySolidColorBrush), typeof(SolidColorBrush), typeof(Window), new PropertyMetadata(null));

        // 取得した色の表示用（マウスカーソル位置のピクセルの色）
        public Color MyColor
        {
            get { return (Color)GetValue(MyColorProperty); }
            set { SetValue(MyColorProperty, value); }
        }
        public static readonly DependencyProperty MyColorProperty =
            DependencyProperty.Register(nameof(MyColor), typeof(Color), typeof(Window), new PropertyMetadata(Colors.Transparent));


        // 画像のピクセル座標
        public int MyPixelX
        {
            get { return (int)GetValue(MyPixelXProperty); }
            set { SetValue(MyPixelXProperty, value); }
        }
        public static readonly DependencyProperty MyPixelXProperty =
            DependencyProperty.Register(nameof(MyPixelX), typeof(int), typeof(Window), new PropertyMetadata(0));

        public int MyPixelY
        {
            get { return (int)GetValue(MyPixelYProperty); }
            set { SetValue(MyPixelYProperty, value); }
        }
        public static readonly DependencyProperty MyPixelYProperty =
            DependencyProperty.Register(nameof(MyPixelY), typeof(int), typeof(Window), new PropertyMetadata(0));

        public Point MyImageClickPoint
        {
            get { return (Point)GetValue(MyImageClickPointProperty); }
            set { SetValue(MyImageClickPointProperty, value); }
        }
        public static readonly DependencyProperty MyImageClickPointProperty =
            DependencyProperty.Register(nameof(MyImageClickPoint), typeof(Point), typeof(Window), new PropertyMetadata(null));


        public double MyImageScale
        {
            get { return (double)GetValue(MyImageScaleProperty); }
            set { SetValue(MyImageScaleProperty, value); }
        }
        public static readonly DependencyProperty MyImageScaleProperty =
            DependencyProperty.Register(nameof(MyImageScale), typeof(double), typeof(BitmapSourceVisualizerWindow), new PropertyMetadata(1.0, OnMyImageScale));

        private static void OnMyImageScale(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BitmapSourceVisualizerWindow main)
            {
                if (main.ImageControl.IsMouseOver)
                {
                    AdjustOffset(main.MyScroll, main.MyBitmapSource, (double)e.NewValue, main.MyPixelX, main.MyPixelY);
                }
            }
        }

        private static void AdjustOffset(ScrollViewer scroll, BitmapSource bmp, double scale, int currentXPos, int currentYPos)
        {
            var bmpViewSize = bmp.PixelWidth * scale;
            var maxOffset = bmpViewSize - scroll.ActualWidth;
            var ratePos = currentXPos / bmp.PixelWidth;
            var pos = maxOffset * ratePos;
            scroll.ScrollToHorizontalOffset(pos);

            bmpViewSize = bmp.PixelHeight * scale;
            maxOffset = bmpViewSize - scroll.ActualHeight;
            ratePos = currentYPos / bmp.PixelHeight;
            pos = maxOffset * ratePos;
            scroll.ScrollToVerticalOffset(pos);
        }
        #endregion 依存関係プロパティ

        public void SetImage(BitmapSource bitmap)
        {
            MyBitmapSource = bitmap;
            ImageControl.Source = bitmap;
            Title = $"BitmapSource Visualizer - {bitmap.PixelWidth} x {bitmap.PixelHeight}";
        }

        /// <summary>
        /// 右クリックメニュー作成
        /// </summary>
        /// <returns></returns>
        private ContextMenu CreateContextMenu()
        {
            ContextMenu menu = new();
            MenuItem item = new() { Header = "コピー(等倍)" };
            menu.Items.Add(item);
            item.Click += (s, e) =>
            {
                if (MyBitmapSource is not null)
                {
                    BitmapToPngImageToClipboard(MyBitmapSource);
                }
            };

            item = new() { Header = "保存(png)" };
            menu.Items.Add(item);
            item.Click += (s, e) =>
            {
                if (MyBitmapSource is not null)
                {
                    SaveBitmapSource(MyBitmapSource);
                }
            };

            menu.Items.Add(new Separator());

            item = new() { Header = "コピー(拡大後)" };
            menu.Items.Add(item);
            item.Click += (s, e) =>
            {
                if (MyBitmapSource is not null)
                {
                    CopyToClipboardExterior(ImageControl);
                }
            };

            item = new() { Header = "保存(拡大後)(png)" };
            menu.Items.Add(item);
            item.Click += (s, e) =>
            {
                if (MyBitmapSource is not null)
                {
                    SaveExteriorToImageFile();
                }
            };

            return menu;
        }

        #region 画像処理



        private void SaveBitmapSource(BitmapSource bitmap)
        {
            Microsoft.Win32.SaveFileDialog dialog = new()
            {
                AddExtension = true,
                DefaultExt = "png",
            };

            if (dialog.ShowDialog() == true)
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using FileStream stream = new(dialog.FileName, FileMode.Create, FileAccess.Write);
                encoder.Save(stream);
            }
        }

        //アルファ値を失わずに画像のコピペできた、.NET WPFのClipboard - 午後わてんのブログ
        //        https://gogowaten.hatenablog.com/entry/2021/02/10/134406
        //より
        private static void BitmapToPngImageToClipboard(BitmapSource source)
        {
            //画像をPNGにエンコード
            PngBitmapEncoder pngEnc = new();
            pngEnc.Frames.Add(BitmapFrame.Create(source));
            //エンコードした画像をMemoryStreamにSava
            using var ms = new System.IO.MemoryStream();
            pngEnc.Save(ms);
            //MemoryStreamをクリップボードにコピー
            Clipboard.SetData("PNG", ms);
        }

        // 要素からBitmap作成
        public static RenderTargetBitmap MakeBitmapFromElement(double width, double height, FrameworkElement item)
        {
            int w = (int)width;
            int h = (int)height;
            double dpi = 96.0 * PresentationSource.FromVisual(item).CompositionTarget.TransformFromDevice.M11;
            RenderTargetBitmap bmp = new(w, h, dpi, dpi, PixelFormats.Pbgra32);
            bmp.Render(item);
            return bmp;
        }

        // 完成版：要素からBitmap作成、LayoutTransformによる回転拡大対応
        public static RenderTargetBitmap MakeBitmapFromLayoutTransformElement(FrameworkElement element)
        {
            double dpi = 96.0 * PresentationSource.FromVisual(element).CompositionTarget.TransformFromDevice.M11;
            Rect bounds = new(0, 0, element.ActualWidth, element.ActualHeight); // 元のBounds
            Rect ltBounds = element.LayoutTransform.TransformBounds(bounds); // 変形後のBounds
            DrawingVisual dv = new();
            dv.Offset = new Vector(-ltBounds.X, -ltBounds.Y);

            using (DrawingContext context = dv.RenderOpen())
            {
                VisualBrush brush = new(element) { Stretch = Stretch.None };
                context.DrawRectangle(brush, null, ltBounds);
            }
            RenderTargetBitmap bmp =
                new(MyCeiling(ltBounds.Width), MyCeiling(ltBounds.Height), dpi, dpi, PixelFormats.Pbgra32);
            bmp.Render(dv);
            return bmp;
        }


        // doubleを切り上げてintに変換
        public static int MyCeiling(double value)
        {
            return (int)Math.Ceiling(value);
        }

        #endregion 画像処理


        private void ButtonCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource is not null)
            {
                BitmapToPngImageToClipboard(MyBitmapSource);
            }
        }



        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveBitmapSource(MyBitmapSource);
        }




        private void ButtonSetScale_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag.ToString(), out int scale))
            {
                MyImageScale = scale;
            }
        }

        private void ButtonSetMathScale_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && double.TryParse(button.Tag.ToString(), out double scale))
            {
                MyImageScale = GetClampedImageScale(MyImageScale * scale);
                //MyImageScale = Clamp(MyImageScale * scale, ImageScaleMin, ImageScaleMax);
            }
        }

        private double GetClampedImageScale(double scale)
        {
            return Clamp(scale, ImageScaleMin, ImageScaleMax);
        }

        // クランプ。値を上限下限内に収めて返す
        private static double Clamp(double value, double min, double max)
        {
            if (min > max) { (max, min) = (min, max); }

            double result = value;
            if (value < min) { result = min; }
            else if (value > max) { result = max; }

            return result;
        }


        // LayoutTransformによる変形後の要素からBitmap作成して、クリップボードにコピー
        private void ButtonCopyToClipboardExterior_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboardExterior(ImageControl);

        }

        // LayoutTransformによる変形後の要素からBitmap作成して、クリップボードにコピー
        public static void CopyToClipboardExterior(FrameworkElement element)
        {
            // 処理に伴うメモリ量と処理続行の確認してから
            if (CheckMemoryAndConfirmConsent(element))
            {
                BitmapToPngImageToClipboard(MakeBitmapFromLayoutTransformElement(element));
            }
        }

        // 変形後の要素を画像としてファイルに保存
        private void ButtonSaveExterior_Click(object sender, RoutedEventArgs e)
        {
            SaveExteriorToImageFile();
        }

        private void SaveExteriorToImageFile()
        {
            // 処理に伴うメモリ量と処理続行の確認してから
            if (CheckMemoryAndConfirmConsent(ImageControl))
            {
                SaveBitmapSource(MakeBitmapFromLayoutTransformElement(ImageControl));
            }
        }

        #region チェック系

        /// <summary>
        /// 要素のBitmap化の処理で1GB以上のメモリを使用する場合にtrueを返す
        /// LayoutTransformにより変形された要素に対応
        /// RenderTransformにより変形された要素には未対応
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static bool IsOver1GigaByte(FrameworkElement element)
        {
            Rect bounds = new(0, 0, element.ActualWidth, element.ActualHeight); // 元のBounds
            var ltBounds = element.LayoutTransform.TransformBounds(bounds); // 変形後のBounds

            if (ltBounds.Width * ltBounds.Height > 1000 * 1000 * 1000)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 要素のBitmap化で1GB以上のメモリを使用する場合の処理続行の確認
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static bool CheckMemoryAndConfirmConsent(FrameworkElement element)
        {
            if (IsOver1GigaByte(element))
            {
                if (MessageBox.Show("使用メモリが1GBを超えるけど、処理続行する？", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    return true;
                }
                else { return false; }
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 空きメモリ容量取得、MB
        /// </summary>
        /// <returns></returns>
        public float GetFreeMemoryCapacity()
        {
            float result = 0;
            using (System.Diagnostics.PerformanceCounter ramcounter = new("Memory", "Available MBytes"))
            {
                float availableMemoryMB = ramcounter.NextValue();
                result = availableMemoryMB;
                Debug.WriteLine($"空きメモリ：{availableMemoryMB} MB");
            }
            return result;
        }
        #endregion チェック系

        #region マウスホイールでの倍率変更

        // マウスホイールでの倍率変更
        private void MyScroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ChangeScaleWithMouseWheel(e.Delta);

            //// 拡大率変更
            //if (ChangeScaleWithMouseWheel(e.Delta))
            //{
            //    // スクロール位置調整
            //    AdjustScrollOffset();
            //}
            e.Handled = true;


        }

        // マウスホイールでの倍率変更
        private bool ChangeScaleWithMouseWheel(int delta)
        {
            // 今の倍率と増減値
            // 20～：10
            // 1～20：1
            // 0.01～1：2倍、半分
            var resultScale = MyImageScale;
            // 拡大時
            if (delta > 0)
            {
                // 今の倍率が
                // 20以上なら+10
                // 1以上なら+1
                // 1未満なら2倍にする
                if (resultScale >= 20.0) { resultScale += 10.0; }
                else if (resultScale >= 1) { resultScale++; }
                else if (resultScale > 0.5) { resultScale = (int)(resultScale * 2.0); }
                else { resultScale *= 2; }
            }
            // 縮小時
            else
            {
                // 今の倍率が
                // 20より大きいなら-10
                // 2以上なら-1してから小数以下を切り捨て
                if (resultScale > 20.0) { resultScale -= 10.0; }
                else if (resultScale >= 2) { resultScale = (int)(resultScale - 1.0); }
                // 2未満1より大きいなら1.0にする
                else if (resultScale > 1) { resultScale = 1.0; }
                // 1以下なら半分にする
                else { resultScale = resultScale / 2.0; }

            }
            var clamped = GetClampedImageScale(resultScale);
            if (clamped == MyImageScale) { return false; }
            MyImageScale = clamped;
            return true;

        }


        #endregion マウスホイールでの倍率変更

        #region マウスドラッグ移動でスクロールバーを移動させる



        // 通常のMouseDownでは反応しないので、Preview版でクリック座標を記録
        private void ImageControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MyPoint = e.GetPosition(this);
            ImageControl.CaptureMouse();
        }

        //      WPF、ScrollViewerの中の要素をマウスドラッグ移動しているように見せかける - 午後わてんのブログ
        //https://gogowaten.hatenablog.com/entry/15755956

        // ボタンを離した時
        private void ImageControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ImageControl.Cursor = Cursors.Arrow;
            ImageControl.ReleaseMouseCapture();
        }

        // ドラッグ移動時
        // マウスの移動距離をスクロールバーに加算する
        private void ImageControl_MouseMove(object sender, MouseEventArgs e)
        {
            //マウスドラッグ移動の距離だけスクロールさせるには
            //(直前のカーソル位置 - 今のカーソル位置) + (スクロールバーのOffset位置)
            //この値をSetOffsetする
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ImageControl.Cursor = Cursors.ScrollAll;//カーソル形状を変更
                //今のマウスの座標
                var nowPoint = e.GetPosition(this);
                //マウスの移動距離＝直前の座標と今の座標の差
                var xd = MyPoint.X - nowPoint.X;
                var yd = MyPoint.Y - nowPoint.Y;
                //xd *= 2;//2倍速
                //yd *= 2;

                //移動距離＋今のスクロール位置
                xd += MyScroll.HorizontalOffset;
                yd += MyScroll.VerticalOffset;

                //スクロール位置の指定
                MyScroll.ScrollToHorizontalOffset(xd);
                MyScroll.ScrollToVerticalOffset(yd);

                MyPoint = nowPoint;//直前の座標を今の座標に変更
            }

            // マウスカーソル位置を記録してから、カーソル位置のピクセルの色を取得
            MyImageClickPoint = e.GetPosition(ImageControl);
            UpdateMyColor();
        }

        // WPFとVB.NET、表示した画像をクリックした場所の色の取得はややこしい - 午後わてんのブログ
        // https://gogowaten.hatenablog.com/entry/13952774
        // カーソル位置のピクセルの色を取得
        private void UpdateMyColor()
        {
            int px = (int)MyImageClickPoint.X;
            int py = (int)MyImageClickPoint.Y;
            if (px >= MyBitmapSource.PixelWidth) { px = MyBitmapSource.PixelWidth - 1; }
            if (py >= MyBitmapSource.PixelHeight) { py = MyBitmapSource.PixelHeight - 1; }

            if (px != MyPixelX || py != MyPixelY)
            {
                MyPixelX = px; // マウスカーソル位置のピクセル座標
                MyPixelY = py;
                CroppedBitmap cropBmp = new(MyBitmapSource, new Int32Rect(MyPixelX, MyPixelY, 1, 1));
                FormatConvertedBitmap bgraBmp = new(cropBmp, PixelFormats.Bgra32, null, 0);
                byte[] pixels = new byte[40];
                bgraBmp.CopyPixels(pixels, 4, 0);
                MyColor = Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
                MySolidColorBrush = new SolidColorBrush(MyColor);
            }
        }

        #endregion マウスドラッグ移動でスクロールバーを移動させる

        #region スクロール系、拡大率変更時に位置調整

        //// スクロール追従
        //// スクロール位置調整の調整、割合を保つ
        //private void AdjustScrollOffset()
        //{
        //    if (MyScroll.ScrollableWidth > 0)
        //    {
        //        var actSize = MyScroll.ActualWidth;
        //        var bmpSize = MyBitmapSource.PixelWidth * MyImageScale;
        //        var maxScroll = bmpSize - actSize;
        //        if (maxScroll < 0) { maxScroll = 0; }

        //        var offset = maxScroll * MyPreHScroll; // スクロール最大値 * 位置の割合
        //        MyScroll.ScrollToHorizontalOffset(offset);
        //    }

        //    if (MyScroll.ScrollableHeight > 0)
        //    {
        //        var actSize = MyScroll.ActualHeight;
        //        var bmpSize = MyBitmapSource.PixelHeight * MyImageScale;
        //        var maxScroll = bmpSize - actSize;
        //        if (maxScroll < 0) { maxScroll = 0; }

        //        var offset = maxScroll * MyPreVScroll; // スクロール最大値 * 位置の割合
        //        MyScroll.ScrollToVerticalOffset(offset);
        //    }
        //}

        private void AdjustScrollOffset2()
        {
            if (MyScroll.ScrollableWidth > 0)
            {
                var actSize = MyScroll.ActualWidth;
                var bmpSize = MyBitmapSource.PixelWidth * MyImageScale;
                var maxScroll = bmpSize - actSize;
                if (maxScroll < 0) { maxScroll = 0; }

                var offset = maxScroll * MyPreHScroll; // スクロール最大値 * 位置の割合
                MyScroll.ScrollToHorizontalOffset(offset);
            }

            if (MyScroll.ScrollableHeight > 0)
            {
                var actSize = MyScroll.ActualHeight;
                var bmpSize = MyBitmapSource.PixelHeight * MyImageScale;
                var maxScroll = bmpSize - actSize;
                if (maxScroll < 0) { maxScroll = 0; }

                var offset = maxScroll * MyPreVScroll; // スクロール最大値 * 位置の割合
                MyScroll.ScrollToVerticalOffset(offset);
            }
        }



        // スクロール時
        private void MyScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // スクロール位置を記録
            if (MyScroll.ScrollableWidth > 0)
            {
                MyPreHScroll = MyScroll.HorizontalOffset / MyScroll.ScrollableWidth;
            }
            else { MyPreHScroll = 0; }
            if (MyScroll.ScrollableHeight > 0)
            {
                MyPreVScroll = MyScroll.VerticalOffset / MyScroll.ScrollableHeight;
            }
            else { MyPreVScroll = 0; }
        }

        #endregion スクロール系
    }
}
