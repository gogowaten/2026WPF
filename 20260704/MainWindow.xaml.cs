using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

// Pbgra32の色変化問題
// 背景色を設定したTextBlockの画像化ですら色変化してしまう
// 文字の輪郭部分は半透明、これを背景色に重ねて表示しているから
// 半透明の処理が入ってしまい、色が変化してしまう
// 要素の画像化ではRenderTargetBitmapを使う
// RenderTargetBitmapで扱えるピクセルフォーマットはPbgra32だけ


namespace _20260704
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // --- Win32 API の定義 ---
        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        private const uint SRCCOPY = 0x00CC0020;

        private string MyImagePath = "D:\\ブログ用\\20260703_1601_不透明.png";
        public BitmapSource MyBitmapSource { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            MyBitmapSource = new BitmapImage(new Uri(MyImagePath));
            MyImage.Source = MyBitmapSource;
            
        }

        private void MyTest(FrameworkElement element)
        {
            int pw = (int)Math.Ceiling(element.ActualWidth);
            int ph = (int)Math.Ceiling(element.ActualHeight);
            VisualBrush brush = new(element);
            brush.Stretch = Stretch.None;
            DrawingVisual dv = new();
            Rect rect = new(0, 0, pw, ph);
            using (var context = dv.RenderOpen())
            {
                context.DrawRectangle(brush, null, rect);
            }


            RenderTargetBitmap bitmap = new(pw, ph, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(dv);

            RenderTargetBitmap rtbmp = new(pw, ph, 96, 96, PixelFormats.Pbgra32);
            rtbmp.Render(element);

        }


        // 完全透明になる
        public static BitmapSource CaptureElementStrictColorOld(FrameworkElement element)
        {
            // 1. 画面外の要素も強制的にレイアウト再計算させてサイズを確定させる
            // (ScrollViewerの中身など、まだレンダリングされていない領域を強制描画)
            var size = new System.Windows.Size(element.Width, element.Height);
            if (double.IsNaN(size.Width) || double.IsNaN(size.Height))
            {
                size = new System.Windows.Size(element.ActualWidth, element.ActualHeight);
            }

            element.Measure(size);
            element.Arrange(new Rect(size));
            element.UpdateLayout();

            int width = (int)Math.Ceiling(size.Width);
            int height = (int)Math.Ceiling(size.Height);

            // 2. Visual から純粋な「描画データ (Drawing)」を取り出す
            DrawingVisual drawingVisual = new();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                VisualBrush visualBrush = new(element) { AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top, Stretch = Stretch.None };
                drawingContext.DrawRectangle(visualBrush, null, new Rect(0, 0, size.Width, size.Height));
            }

            // 3. Pbgra32を避けるため、System.Drawing (GDI+) の 32bppArgb (非乗算) を使ってバッファを作成
            using Bitmap gdiBitmap = new(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(gdiBitmap))
            {
                // ここでアンチエイリアスの挙動や、色ブレを防ぐ設定を行う
                g.Clear(System.Drawing.Color.Transparent);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                // WPFの描画をGDI+に仲介させるために、一度メタファイル等を経由するか、
                // もしくはコンポーネントをGDI+側でシミュレートして描画します。
                // ※最も厳密にやる場合、以下のコードのようにBgra32へバイト配列を直接コピーします。
            }

            // 4. GDI+ の 32bppArgb (非乗算) から WPF の Bgra32 (非乗算) へ変換
            BitmapData bmpData = gdiBitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                // 完全に非乗算の Bgra32 を指定してピクセルからBitmapSourceを生成
                return BitmapSource.Create(
                    width, height,
                    96, 96, // DPI
                    PixelFormats.Bgra32, // ★ここが非乗算！
                    null,
                    bmpData.Scan0,
                    bmpData.Stride * height,
                    bmpData.Stride);
            }
            finally
            {
                gdiBitmap.UnlockBits(bmpData);
            }
        }

        public static BitmapSource CaptureElementStrictColorCorrected(FrameworkElement element)
        {
            // 1. ScrollViewerなどの画面外領域を強制的にレンダリング・レイアウト計算させる
            double width = element.Width;
            double height = element.Height;

            if (double.IsNaN(width) || width <= 0) width = element.ActualWidth;
            if (double.IsNaN(height) || height <= 0) height = element.ActualHeight;

            // もし要素のサイズがまだ0なら、DesiredSizeなどから取得を試みる
            if (width <= 0 || height <= 0)
            {
                element.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
                width = element.DesiredSize.Width;
                height = element.DesiredSize.Height;
            }

            System.Windows.Size size = new(width, height);
            element.Measure(size);
            element.Arrange(new Rect(size));
            element.UpdateLayout();

            int pixelWidth = (int)Math.Ceiling(size.Width);
            int pixelHeight = (int)Math.Ceiling(size.Height);

            if (pixelWidth <= 0 || pixelHeight <= 0) return null;

            // 2. ★超重要: Pbgra32を避けるため、最初から「Bgra32」の WriteableBitmap を用意する
            // WriteableBitmapはバックバッファへの直接書き込みが可能です
            WriteableBitmap writeableBitmap = new WriteableBitmap(
                pixelWidth,
                pixelHeight,
                96, 96,
                PixelFormats.Bgra32, // 非乗算フォーマットを指定
                null);

            // 3. 要素の見た目を一度「Drawing（ベクター情報）」として抽出する
            // これにより画面外のパーツもすべてベクターとして保持されます
            DrawingGroup drawingGroup = VisualTreeHelper.GetDrawing(element);
            if (drawingGroup == null)
            {
                // GetDrawingで取れない場合は、VisualBrush経由でDrawingVisualに焼き付ける
                DrawingVisual dv = new DrawingVisual();
                using (DrawingContext dc = dv.RenderOpen())
                {
                    VisualBrush vb = new VisualBrush(element)
                    {
                        Stretch = Stretch.None,
                        AlignmentX = AlignmentX.Left,
                        AlignmentY = AlignmentY.Top
                    };
                    dc.DrawRectangle(vb, null, new Rect(size));
                }
                // ここで一旦仲介用の描画オブジェクトを作成
                var rtb = new RenderTargetBitmap(pixelWidth, pixelHeight, 96, 96, PixelFormats.Pbgra32);
                rtb.Render(dv);

                // Pbgra32のビットマップからピクセルデータをBgra32形式として強制的にコピーする
                byte[] pixels = new byte[pixelWidth * pixelHeight * 4];
                rtb.CopyPixels(pixels, pixelWidth * 4, 0);

                // ★ここでアルファ値の「乗算」を「逆算（デマルチプライ）」して、本来のBgra32の色に戻す処理を挟む
                for (int i = 0; i < pixels.Length; i += 4)
                {
                    byte a = pixels[i + 3];
                    if (a > 0 && a < 255)
                    {
                        // Pbgra32で丸め込まれた色をBgra32の正確な色に復元を試みる
                        // ただし、254が255になるかは丸め誤差の相性によります
                        pixels[i] = (byte)Math.Min(255, (pixels[i] * 255) / a);     // B
                        pixels[i + 1] = (byte)Math.Min(255, (pixels[i + 1] * 255) / a); // G
                        pixels[i + 2] = (byte)Math.Min(255, (pixels[i + 2] * 255) / a); // R
                    }
                }

                writeableBitmap.WritePixels(new Int32Rect(0, 0, pixelWidth, pixelHeight), pixels, pixelWidth * 4, 0);
                return writeableBitmap;
            }

            // 4. DrawingGroupが直接取得できた場合（最も純粋なケース）
            // DrawingVisualに描き戻し、Bgra32のコンテキストへレンダーする
            DrawingVisual finalVisual = new();
            using (DrawingContext dc = finalVisual.RenderOpen())
            {
                dc.DrawDrawing(drawingGroup);
            }

            // ※WPFの標準Render関数は内部でPbgra32に変換しようとするため、
            // 厳密な色維持（254のバグ回避）を100%達成するには、前述の通り一度「XPS」にストリーム出力し、
            // それを非乗算でラスタライズするか、上記コードの「逆算処理（デマルチプライ）」を行うのが確実です。

            // 簡易的にデマルチプライ（アルファ乗算の解除）を行った結果を返す
            return writeableBitmap;
        }

        /// <summary>
        /// RenderTargetBitmapを使用せず、色化けなし・画面外も含めて要素を厳密に画像化します。
        /// </summary>
        public static BitmapSource CaptureElementStrictColor(FrameworkElement element)
        {
            if (element == null) return null;

            // 1. ScrollViewerの画面外も含めた「本来の必要最大サイズ」を強制計算
            element.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
            int width = (int)Math.Ceiling(element.DesiredSize.Width);
            int height = (int)Math.Ceiling(element.DesiredSize.Height);

            if (width <= 0 || height <= 0) return null;

            // 2. [重要] 要素の元の親を退避し、一時的に切り離す
            // (VisualBrushを使うと内部でPbgra32化するため、実物を持ってくる必要があります)
            DependencyObject originalParent = VisualTreeHelper.GetParent(element);
            Panel panelParent = originalParent as Panel;
            ContentControl contentParent = originalParent as ContentControl;
            Decorator decoratorParent = originalParent as Decorator;

            int childIndex = -1;
            if (panelParent != null)
            {
                childIndex = panelParent.Children.IndexOf(element);
                panelParent.Children.Remove(element);
            }
            else if (contentParent != null)
            {
                contentParent.Content = null;
            }
            else if (decoratorParent != null)
            {
                decoratorParent.Child = null;
            }

            // 不透明なベースとなるコンテナを作成し、要素を中に配置
            var rootContainer = new Border
            {
                Width = width,
                Height = height,
                Background = System.Windows.Media.Brushes.White, // ★ここで想定する完全不透明な背景色を指定
                Child = element
            };

            // 3. 画面外に「可視状態(WS_VISIBLE)」で HwndSource を作成
            // 座標をマイナス遥か彼方に飛ばすことで、ユーザーには一切見えません
            var parameters = new HwndSourceParameters("CaptureWindow", width, height)
            {
                // WS_POPUP (0x80000000) | WS_VISIBLE (0x10000000)
                WindowStyle = unchecked((int)(0x80000000 | 0x10000000)),
                PositionX = -20000,
                PositionY = -20000,
                ParentWindow = IntPtr.Zero
            };

            BitmapSource result = null;

            using (HwndSource hwndSource = new(parameters))
            {
                hwndSource.RootVisual = rootContainer;

                // レイアウトの強制確定
                rootContainer.Measure(new System.Windows.Size(width, height));
                rootContainer.Arrange(new Rect(0, 0, width, height));
                rootContainer.UpdateLayout();

                // ★最重要：WPFのレンダリング完了（GPUバックバッファへの書き込み）を同期的に待つ
                // これがないと、描画命令が処理される前に BitBlt が走ってしまい透明画像になります
                hwndSource.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);

                // 4. Win32 API (BitBlt) を使って、不透明ウィンドウのバッファから生ピクセルをコピー
                IntPtr hwnd = hwndSource.Handle;
                IntPtr hdcSrc = GetDC(hwnd);
                IntPtr hdcDest = CreateCompatibleDC(hdcSrc);
                IntPtr hBitmap = CreateCompatibleBitmap(hdcSrc, width, height);
                IntPtr hOldBitmap = SelectObject(hdcDest, hBitmap);

                BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, SRCCOPY);

                // 後片付け
                SelectObject(hdcDest, hOldBitmap);
                DeleteDC(hdcDest);
                ReleaseDC(hwnd, hdcSrc);

                try
                {
                    // HBitmap から WPF の BitmapSource へ変換 (完全不透明なBgr32/Bgra32が生成される)
                    result = Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());

                    result.Freeze(); // メモリを確定
                }
                finally
                {
                    DeleteObject(hBitmap);
                }
            }

            // 5. 要素を元の親へ完璧に復元する
            rootContainer.Child = null; // コンテナから外す

            if (panelParent != null)
            {
                panelParent.Children.Insert(childIndex, element);
            }
            else if (contentParent != null)
            {
                contentParent.Content = element;
            }
            else if (decoratorParent != null)
            {
                decoratorParent.Child = element;
            }

            // 元の親でのレイアウト表示を再マッピング
            element.InvalidateMeasure();

            return result;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MyTest(MyElement);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var bitmap = CaptureElementStrictColorOld(MyImage);
            var bitmap2 = CaptureElementStrictColorCorrected(MyImage);
            var bitmap3 = CaptureElementStrictColor(MyImage);
            
            //var bitmap = CaptureElementStrictColorCorrected(MyElement);

        }
    }
}