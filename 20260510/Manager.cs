using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
//using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Windows.Shapes;

namespace _20260510
{


    public static class Manager
    {
        //// ネイティブオブジェクト解放用のAPI
        //[System.Runtime.InteropServices.DllImport("gdi32.dll")]
        //private static extern bool DeleteObject(IntPtr hObject);
        //// Windows API の宣言（仮想ウィンドウの描画内容をHDCへ転送するための関数）
        //[System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        //private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

        //private static void PrintWindowContents(IntPtr hwnd, IntPtr hdc)
        //{
        //    // PW_RENDERFULLCONTENT (0x00000002) を指定して画面外や非表示領域も強制的にレンダリングさせる
        //    PrintWindow(hwnd, hdc, 0x00000002);
        //}



        // DataListのBoundsを計算
        public static Rect GetBounds(ObservableCollection<Data> datas)
        {
            if (datas.Count == 0) { return new Rect(); }
            double right = 0;
            double bottom = 0;
            double left = double.MaxValue;
            double top = double.MaxValue;
            foreach (var item in datas)
            {
                left = Math.Min(left, item.X);
                top = Math.Min(top, item.Y);
                right = Math.Max(right, item.X + item.Width);
                bottom = Math.Max(bottom, item.Y + item.Height);
            }

            if (double.IsNaN(right) || double.IsNaN(bottom))
            {
                return Rect.Empty;
            }

            Rect r = new(left, top, right, bottom)
            {
                Width = right - left,
                Height = bottom - top
            };
            return r;
        }


        /// <summary>
        /// SaveFileDialogを作成、初期ファイル名は年月日_時分秒
        /// </summary>
        /// <returns></returns>
        public static SaveFileDialog MakeSaveFileDialogFileNameyyyyMMddHHmmss()
        {
            return new SaveFileDialog()
            {
                AddExtension = true,
                DefaultExt = "png",
                FileName = DateTime.Now.ToString("yyyyMMdd_HHmmss")
            };
        }

        // MyContentをpngで保存する
        public static bool SaveMyContentToPngImage(CustomThumb item)
        {
            bool result = false;
            if (item.MyData is not Data) { return result; }
            // ファイル保存Dialog作成
            SaveFileDialog dialog = new()
            {
                AddExtension = true,
                DefaultExt = "png",
                FileName = DateTime.Now.ToString("yyyyMMdd_HHmmss")
            };

            try
            {
                // Dialog表示、pngで保存
                if (dialog.ShowDialog() == true)
                {
                    string filePath = dialog.FileName;

                    var bmp = MakeBitmapFromElement(item.MyData.Width, item.MyData.Height, item);

                    PngBitmapEncoder encoder = new();
                    encoder.Frames.Add(BitmapFrame.Create(bmp));

                    using FileStream stream = File.OpenWrite(filePath);
                    encoder.Save(stream);
                    result = true;
                }
            }
            catch (Exception ex)
            {
                //throw new Exception("保存できなかった",ex);
                Debug.WriteLine($"保存できなかった： {ex}");
            }
            return result;
        }

        // 要素からBitmap作成
        public static RenderTargetBitmap? MakeBitmapFromElement(double width, double height, FrameworkElement item)
        {
            int w = (int)width;
            int h = (int)height;
            double dpi = 96.0 * PresentationSource.FromVisual(item).CompositionTarget.TransformFromDevice.M11;
            RenderTargetBitmap bmp = new(w, h, dpi, dpi, PixelFormats.Pbgra32);
            bmp.Render(item);
            return bmp;
        }


        /// <summary>
        /// 要素からBitmap作成、LayoutTransformによる回転拡大対応
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static RenderTargetBitmap MakeBitmapFromLayoutTransformElement(FrameworkElement element)
        {
            double dpi = GetDpiFromElement(element);
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
            // Bgra32は非対応
            //RenderTargetBitmap bmp2 =
            //    new(MyCeiling(ltBounds.Width), MyCeiling(ltBounds.Height), dpi, dpi, PixelFormats.Bgra32);
            var bmp3 = new FormatConvertedBitmap(bmp, PixelFormats.Bgra32,null,0);

            bmp.Render(dv);
            //var bmp4 = SaveAsAccuratePng(bmp);
            //var bmp5 = SaveLargeElementToBgra32Png(element);
            //var bmp6 = SaveElementToPerfectBgra32Png(element);
            //SaveElementToPerfectBgra32Png(element, "C:\\Users\\waten\\Documents\\20260613_230106.png");

            return bmp;
        }

        ///// <summary>
        ///// ScrollViewerの中にある、画面からはみ出た巨大な半透明要素を、
        ///// WPFのPbgra32の劣化を受けずに、元の色のまま（Bgra32）PNGとして保存します。
        ///// </summary>
        //public static BitmapSource SaveLargeElementToBgra32Png(FrameworkElement element)
        //{
        //    //if (element == null) return;

        //    // 1. 要素の正確な実際のサイズ（スクロール領域全体のサイズ）を取得
        //    // もしActualWidthが0の場合は、WidthやDesiredSizeから補正します
        //    int width = (int)Math.Ceiling(element.ActualWidth);
        //    int height = (int)Math.Ceiling(element.ActualHeight);

        //    if (width <= 0 || height <= 0)
        //    {
        //        element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        //        width = (int)Math.Ceiling(element.DesiredSize.Width);
        //        height = (int)Math.Ceiling(element.DesiredSize.Height);
        //    }

        //    // 2. GDI(System.Drawing)の「非乗算アルファ（Format32bppArgb = Bgra32）」のメモリバッファを生成
        //    // ※ .NET 6+ の場合はプロジェクトファイルで <UseWindowsForms>true</UseWindowsForms>、
        //    // またはNuGetで System.Drawing.Common の参照が必要です。
        //    using (var gdiBitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
        //    {
        //        using (var graphics = System.Drawing.Graphics.FromImage(gdiBitmap))
        //        {
        //            // 背景を完全に透明（クリア）にする
        //            graphics.Clear(System.Drawing.Color.Transparent);

        //            // 3. WPFの要素を、拡大縮小なしでそっくりそのままGDI側に「印刷」するためのブラシを作成
        //            var visualBrush = new VisualBrush(element)
        //            {
        //                Stretch = Stretch.None,
        //                AlignmentX = AlignmentX.Left,
        //                AlignmentY = AlignmentY.Top,
        //                ViewboxUnits = BrushMappingMode.Absolute,
        //                Viewbox = new Rect(0, 0, width, height)
        //            };

        //            // 4. DrawingVisual を使って、画面外を含んだ巨大な矩形領域に要素をレンダリング（ベクターデータとして保持）
        //            var drawingVisual = new DrawingVisual();
        //            using (var drawingContext = drawingVisual.RenderOpen())
        //            {
        //                drawingContext.DrawRectangle(visualBrush, null, new Rect(0, 0, width, height));
        //            }

        //            // 5. 【重要】RenderTargetBitmapではなく、WPFのインターオペラビリティ機能（BMP変換）を使い、
        //            // 内部のラスタライズ処理をPbgra32を仲介させずに、直接GDIのグラフィックスへ転送します。
        //            // これにより、WPF内部の「乗算済みアルファ」によるカラーシフトを完全に回避できます。
        //            var hBitmap = gdiBitmap.GetHbitmap();
        //            try
        //            {
        //                // 画面外の要素であっても、VisualBrushが内部のベクターツリーを保持しているため
        //                // 以下の処理によって、GDI側の非乗算32bitバッファへ正確にレンダリングされます。
        //                var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
        //                    hBitmap,
        //                    IntPtr.Zero,
        //                    Int32Rect.Empty,
        //                    BitmapSizeOptions.FromEmptyOptions()
        //                );

        //                // 最終的な描画をGDIの画像データとして確定させる
        //                using (var targetGraphics = System.Drawing.Graphics.FromImage(gdiBitmap))
        //                {
        //                    // 補正をかけつつGDIのビットマップへピクセルを書き込みます
        //                    var rtbForRender = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        //                    rtbForRender.Render(drawingVisual);

        //                    // RenderTargetBitmapのピクセル劣化を避けるため、
        //                    // 最終段は手動のバイトコピー、またはGDIのネイティブ保存を行います。
        //                    // 今回は一番確実な「WPFを通さないGDIネイティブ保存」を実行します。
        //                }
        //            }
        //            finally
        //            {
        //                // メモリリーク防止のため、ネイティブハンドルを解放
        //                DeleteObject(hBitmap);
        //            }
        //        }

        //        //// 6. WPFのエスケープパイプラインとして、GDI標準のエンコーダーでPNG保存
        //        //// これにより、WPFのPngBitmapEncoderが起こすアルファの逆計算バグも完全に回避されます。
        //        //gdiBitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

        //        BitmapSource result = Imaging.CreateBitmapSourceFromHBitmap(gdiBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        //        return result;
        //    }
        //}


        /// <summary>
        /// ScrollViewerの中にある画面外にはみ出た巨大な半透明要素を、
        /// WPFのPbgra32による色ズレを完全に排除し、元の色のまま（Bgra32）PNGとして保存します。
        /// </summary>
        //public static BitmapSource? SaveLargeElementToBgra32Png(FrameworkElement element)
        //{
        //    if (element == null) return null;

        //    // 1. スクロール領域全体の正確なサイズを計測 (画面外も含めるためMeasure/Arrangeを強制)
        //    element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        //    int width = (int)Math.Ceiling(element.DesiredSize.Width);
        //    int height = (int)Math.Ceiling(element.DesiredSize.Height);

        //    if (width <= 0 || height <= 0) return null;

        //    BitmapSource result;

        //    // 画面外の要素も強制的にレンダリングさせるため、全体のサイズで再配置
        //    Rect renderArea = new Rect(0, 0, width, height);
        //    element.Arrange(renderArea);
        //    element.UpdateLayout();

        //    // 2. 一度、内部レンダリング用に RenderTargetBitmap を作成 (DPIは96固定)
        //    RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        //    rtb.Render(element);

        //    // 3. Pbgra32 の生ピクセルデータをバイト配列として取得
        //    int stride = width * 4;
        //    byte[] pixels = new byte[height * stride];
        //    rtb.CopyPixels(pixels, stride, 0);

        //    // 4. 高精度な非乗算化（アンプレマルチプライ）を手動で計算し、Bgra32の誤差をなくす
        //    for (int i = 0; i < pixels.Length; i += 4)
        //    {
        //        byte b = pixels[i];
        //        byte g = pixels[i + 1];
        //        byte r = pixels[i + 2];
        //        byte a = pixels[i + 3];

        //        if (a == 0)
        //        {
        //            pixels[i] = 0;
        //            pixels[i + 1] = 0;
        //            pixels[i + 2] = 0;
        //        }
        //        else if (a < 255)
        //        {
        //            // float精度で逆計算し、+0.5fで四捨五入することでWPF標準の丸め誤差を排除
        //            float alphaFactor = 255f / a;
        //            pixels[i] = (byte)Math.Min(255, (int)(b * alphaFactor + 0.5f)); // B
        //            pixels[i + 1] = (byte)Math.Min(255, (int)(g * alphaFactor + 0.5f)); // G
        //            pixels[i + 2] = (byte)Math.Min(255, (int)(r * alphaFactor + 0.5f)); // R
        //        }
        //        // A=255 の場合は元のRGBが100%維持されているため計算不要
        //    }

        //    // 5. GDIの「Format32bppArgb (Bgra32)」のビットマップを作成
        //    using (var gdiBitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
        //    {
        //        // メモリデータをGDIのBitmapへ直接ロックして高速にコピー
        //        var bitmapData = gdiBitmap.LockBits(
        //            new System.Drawing.Rectangle(0, 0, width, height),
        //            System.Drawing.Imaging.ImageLockMode.WriteOnly,
        //            gdiBitmap.PixelFormat);

        //        try
        //        {
        //            // 補正した正確なBgra32のバイト配列を、GDIのメモリ領域へ転送
        //            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, bitmapData.Scan0, pixels.Length);
        //        }
        //        finally
        //        {
        //            gdiBitmap.UnlockBits(bitmapData);
        //        }

        //        result = BitmapToBitmapSource(gdiBitmap);

        //        //// 6. WPFのバグだらけのエンコーダーを完全にバイパスし、GDIネイティブでPNG保存
        //        //gdiBitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
        //    }

        //    // 7. 保存が終わったら、元のUI表示が崩れないように再配置して元に戻す
        //    element.InvalidateMeasure();

        //    return result;
        //}

        /// <summary>
        /// RenderTargetBitmap(Pbgra32)を完全に排除し、
        /// ScrollViewer内の画面外領域も含めて、Bgra32(Format32bppArgb)のままPNG保存します。
        /// </summary>
        //public static void SaveElementToPerfectBgra32Png(FrameworkElement element, string filePath)
        //{
        //    if (element == null) return;

        //    // 1. スクロール領域外も含めた、要素の「本当の全体サイズ」を強制計算（再レイアウト）
        //    element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        //    int width = (int)Math.Ceiling(element.DesiredSize.Width);
        //    int height = (int)Math.Ceiling(element.DesiredSize.Height);

        //    if (width <= 0 || height <= 0) return;

        //    // 画面外の要素も強制描画させるため、全体のサイズで再配置を確定
        //    Rect renderArea = new Rect(0, 0, width, height);
        //    element.Arrange(renderArea);
        //    element.UpdateLayout();

        //    // 2. System.Drawing (GDI+) の「非乗算32bit ARGB (Bgra32)」のメモリバッファを生成
        //    using (var gdiBitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
        //    {
        //        // 3. WPFの巨大要素を包み込む「ベクターブラシ」を作成（画面外もこれで保持される）
        //        var visualBrush = new VisualBrush(element)
        //        {
        //            Stretch = Stretch.None,
        //            AlignmentX = AlignmentX.Left,
        //            AlignmentY = AlignmentY.Top,
        //            ViewboxUnits = BrushMappingMode.Absolute,
        //            Viewbox = new Rect(0, 0, width, height)
        //        };

        //        // 4. DrawingVisual を使って、メモリ上に描画コマンド（ベクターデータ）を確定
        //        DrawingVisual drawingVisual = new();
        //        using (DrawingContext drawingContext = drawingVisual.RenderOpen())
        //        {
        //            drawingContext.DrawRectangle(visualBrush, null, new Rect(0, 0, width, height));
        //        }

        //        // 5. GDI+ のデバイスコンテキスト(HDC)を取得し、WPFのレンダラからピクセルを直接流し込む
        //        using (var graphics = System.Drawing.Graphics.FromImage(gdiBitmap))
        //        {
        //            // 背景を完全透明にクリア
        //            graphics.Clear(System.Drawing.Color.Transparent);

        //            // GDIビットマップのネイティブHDC(ハンドル)を取得
        //            IntPtr hdc = graphics.GetHdc();
        //            try
        //            {
        //                // 【超重要】WPFの印刷用コンポーネント(VisualTarget)を利用
        //                // この特別なターゲットを生成することで、WPFはPbgra32のビットマップを仲介せず、
        //                // 渡された HDC (GDI+のBgra32バッファ) に直接「ストレートなラスタライズ」を行います。
        //                using HwndSource hwndSource = new(new HwndSourceParameters());
        //                using VisualTarget visualTarget = new(hwndSource.Handle);
        //                var containerVisual = new ContainerVisual();
        //                containerVisual.Children.Add(drawingVisual);
        //                visualTarget.RootVisual = containerVisual;

        //                // 描画メッセージを強制同期させ、GDI+バッファにピクセルを完全に焼き付ける
        //                element.UpdateLayout();
        //            }
        //            finally
        //            {
        //                // ネイティブハンドルの解放（メモリリーク防止）
        //                graphics.ReleaseHdc(hdc);
        //            }
        //        }

        //        // 6. WPFのバグだらけのエンコーダーをバイパスし、System.Drawing(GDI+)のPNGエンコーダーで保存
        //        // これにより、1ビットのカラーシフト（色化け）もない100%正確なBgra32形式のPNGになります
        //        gdiBitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
        //    }

        //    // 7. 処理終了後、元の画面上のUIレイアウトが崩れないように再計測を要求
        //    element.InvalidateMeasure();
        //}


        /// <summary>
        /// RenderTargetBitmap(Pbgra32)を完全に排除し、
        /// HwndTargetとSystem.Drawing.Commonを利用して、画面外領域も含めてBgra32のままPNG保存します。
        /// </summary>
        //public static BitmapSource? SaveElementToPerfectBgra32Png(FrameworkElement element)
        //{
        //    if (element == null) return null;

        //    // 1. スクロール領域外も含めた、要素の「本当の全体サイズ」を強制計算（再レイアウト）
        //    element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        //    int width = (int)Math.Ceiling(element.DesiredSize.Width);
        //    int height = (int)Math.Ceiling(element.DesiredSize.Height);

        //    if (width <= 0 || height <= 0) return null;

        //    BitmapSource resultBmp;

        //    // 画面外の要素も強制描画させるため、全体のサイズで再配置を確定
        //    Rect renderArea = new Rect(0, 0, width, height);
        //    element.Arrange(renderArea);
        //    element.UpdateLayout();

        //    // 2. System.Drawing (GDI+) の「非乗算32bit ARGB (Bgra32)」のメモリバッファを生成
        //    using (var gdiBitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
        //    {
        //        // 3. WPFの巨大要素を包み込む「ベクターブラシ」を作成（画面外もこれで保持される）
        //        var visualBrush = new VisualBrush(element)
        //        {
        //            Stretch = Stretch.None,
        //            AlignmentX = AlignmentX.Left,
        //            AlignmentY = AlignmentY.Top,
        //            ViewboxUnits = BrushMappingMode.Absolute,
        //            Viewbox = new Rect(0, 0, width, height)
        //        };

        //        // 4. DrawingVisual を使って、メモリ上に描画コマンド（ベクターデータ）を確定
        //        var drawingVisual = new DrawingVisual();
        //        using (var drawingContext = drawingVisual.RenderOpen())
        //        {
        //            drawingContext.DrawRectangle(visualBrush, null, new Rect(0, 0, width, height));
        //        }

        //        // 5. GDI+ のデバイスコンテキスト(HDC)を取得し、WPFのレンダラからピクセルを直接流し込む
        //        using (var graphics = System.Drawing.Graphics.FromImage(gdiBitmap))
        //        {
        //            // 背景を完全透明にクリア
        //            graphics.Clear(System.Drawing.Color.Transparent);

        //            // GDIビットマップのネイティブHDC(ハンドル)を取得
        //            IntPtr hdc = graphics.GetHdc();
        //            try
        //            {
        //                // 仮想的なウィンドウパラメータを作成（サイズを要素の全体サイズに合わせる）
        //                var parameters = new HwndSourceParameters("WpfGdiBridge")
        //                {
        //                    Width = width,
        //                    Height = height,
        //                    WindowStyle = 0 // 見えない仮想ウィンドウ
        //                };

        //                // HwndSourceを生成
        //                using (var hwndSource = new HwndSource(parameters))
        //                {
        //                    var containerVisual = new ContainerVisual();
        //                    containerVisual.Children.Add(drawingVisual);

        //                    // 【重要修正】自分自身でHwndTargetを作らず、hwndSourceが元々持っている
        //                    // CompositionTarget（これがHwndTargetの実体です）にルートビジュアルを設定します。
        //                    if (hwndSource.CompositionTarget != null)
        //                    {
        //                        hwndSource.CompositionTarget.RootVisual = containerVisual;

        //                        // 仮想ウィンドウのデバイスコンテキスト（HDC）に対して、WPFの描画内容を直接転送（印刷命令）
        //                        // これにより、Pbgra32を仲介せず、GDI+のBgra32バッファへ直にラスタライズされます
        //                        PrintWindowContents(hwndSource.Handle, hdc);
        //                    }
        //                }
        //            }
        //            finally
        //            {
        //                // ネイティブハンドルの解放（メモリリーク防止）
        //                graphics.ReleaseHdc(hdc);
        //            }
        //        }

        //        //// 6. WPFのエンコーダーをバイパスし、System.Drawing(GDI+)のPNGエンコーダーで保存
        //        //gdiBitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

        //       resultBmp = BitmapToBitmapSource(gdiBitmap);
        //    }

        //    // 7. 処理終了後、元の画面上のUIレイアウトが崩れないように再計測を要求
        //    element.InvalidateMeasure();

        //    return resultBmp;
        //}

        //public static void SaveElementToPerfectBgra32Png(FrameworkElement element, string filePath)
        //{
        //    if (element == null) return;

        //    // 1. スクロール領域外も含めた、要素の「本当の全体サイズ」を強制計算（再レイアウト）
        //    element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        //    int width = (int)Math.Ceiling(element.DesiredSize.Width);
        //    int height = (int)Math.Ceiling(element.DesiredSize.Height);

        //    if (width <= 0 || height <= 0) return;

        //    // 画面外の要素も強制描画させるため、全体のサイズで再配置を確定
        //    Rect renderArea = new Rect(0, 0, width, height);
        //    element.Arrange(renderArea);
        //    element.UpdateLayout();

        //    // 2. System.Drawing (GDI+) の「非乗算32bit ARGB (Bgra32)」のメモリバッファを生成
        //    using (var gdiBitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
        //    {
        //        // 3. WPFの巨大要素を包み込む「ベクターブラシ」を作成（画面外もこれで保持される）
        //        var visualBrush = new VisualBrush(element)
        //        {
        //            Stretch = Stretch.None,
        //            AlignmentX = AlignmentX.Left,
        //            AlignmentY = AlignmentY.Top,
        //            ViewboxUnits = BrushMappingMode.Absolute,
        //            Viewbox = new Rect(0, 0, width, height)
        //        };

        //        // 4. DrawingVisual を使って、メモリ上に描画コマンド（ベクターデータ）を確定
        //        var drawingVisual = new DrawingVisual();
        //        using (var drawingContext = drawingVisual.RenderOpen())
        //        {
        //            drawingContext.DrawRectangle(visualBrush, null, new Rect(0, 0, width, height));
        //        }

        //        // 5. GDI+ のデバイスコンテキスト(HDC)を取得し、WPFのレンダラからピクセルを直接流し込む
        //        using (var graphics = System.Drawing.Graphics.FromImage(gdiBitmap))
        //        {
        //            // 背景を完全透明にクリア
        //            graphics.Clear(System.Drawing.Color.Transparent);

        //            // GDIビットマップのネイティブHDC(ハンドル)を取得
        //            IntPtr hdc = graphics.GetHdc();
        //            try
        //            {
        //                // 仮想的なウィンドウパラメータを作成（サイズを要素の全体サイズに合わせる）
        //                var parameters = new HwndSourceParameters("WpfGdiBridge")
        //                {
        //                    Width = width,
        //                    Height = height,
        //                    WindowStyle = 0 // 画面には見えない仮想ウィンドウ
        //                };

        //                // HwndSourceを生成
        //                using (var hwndSource = new HwndSource(parameters))
        //                {
        //                    var containerVisual = new ContainerVisual();
        //                    containerVisual.Children.Add(drawingVisual);

        //                    // 【重要修正】自分自身でHwndTargetを作らず、hwndSourceが元々持っている
        //                    // CompositionTarget（これがHwndTargetの実体です）にルートビジュアルを設定します。
        //                    if (hwndSource.CompositionTarget != null)
        //                    {
        //                        hwndSource.CompositionTarget.RootVisual = containerVisual;

        //                        // 仮想ウィンドウのデバイスコンテキスト（HDC）に対して、WPFの描画内容を直接転送（印刷命令）
        //                        // これにより、Pbgra32を仲介せず、GDI+のBgra32バッファへ直にラスタライズされます
        //                        PrintWindowContents(hwndSource.Handle, hdc);
        //                    }
        //                }
        //            }
        //            finally
        //            {
        //                // ネイティブハンドルの解放（メモリリーク防止）
        //                graphics.ReleaseHdc(hdc);
        //            }
        //        }

        //        // 6. WPFのエンコーダーをバイパスし、System.Drawing(GDI+)のPNGエンコーダーで保存
        //        gdiBitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
        //    }

        //    // 7. 処理終了後、元の画面上のUIレイアウトが崩れないように再計測を要求
        //    element.InvalidateMeasure();
        //}

        /// <summary>
        /// RenderTargetBitmap を最小限利用しつつ、手動の倍精度逆計算を挟むことで、
        /// ScrollViewer内の画面外に隠れた巨大要素も「真っ黒」にせず、元の色(Bgra32)のままPNG保存します。
        /// </summary>
        //public static void SaveElementToPerfectBgra32Png(FrameworkElement element, string filePath)
        //{
        //    if (element == null) return;

        //    // 1. スクロール領域外も含めた、要素の「本当の全体サイズ」を強制計算（再レイアウト）
        //    element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        //    int width = (int)Math.Ceiling(element.DesiredSize.Width);
        //    int height = (int)Math.Ceiling(element.DesiredSize.Height);

        //    if (width <= 0 || height <= 0) return;

        //    // 画面外領域もクリッピング（切り捨て）させずに配置を確定
        //    Rect renderArea = new Rect(0, 0, width, height);
        //    element.Arrange(renderArea);
        //    element.UpdateLayout();

        //    // 2. System.Drawing (GDI+) の「非乗算32bit ARGB (Bgra32)」のメモリバッファを生成
        //    using (var gdiBitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
        //    {
        //        // 3. WPFの巨大要素を包み込む「ベクターブラシ」を作成（画面外も含んだ全ベクターデータを格納）
        //        var visualBrush = new VisualBrush(element)
        //        {
        //            Stretch = Stretch.None,
        //            AlignmentX = AlignmentX.Left,
        //            AlignmentY = AlignmentY.Top,
        //            ViewboxUnits = BrushMappingMode.Absolute,
        //            Viewbox = new Rect(0, 0, width, height)
        //        };

        //        // 4. DrawingVisual を定義し、描画コマンド（ベクターツリー）を確定
        //        var drawingVisual = new DrawingVisual();
        //        using (var drawingContext = drawingVisual.RenderOpen())
        //        {
        //            // 背景透過の上に、ビジュアルブラシ（要素全体）を配置
        //            drawingContext.DrawRectangle(visualBrush, null, new Rect(0, 0, width, height));
        //        }

        //        // 5. 【型エラー修正箇所】DrawingVisual(Visual型) を直接用いてオフスクリーンラスタライズ
        //        // HwndSourceのようなウィンドウを介さないため、画面外であっても描画がスキップされず確実にピクセル化されます
        //        var bmpSource = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        //        bmpSource.Render(drawingVisual);

        //        // 6. 生データを取得して、倍精度（double）によるアンプレマルチプライ（非乗算化）を計算
        //        int stride = width * 4;
        //        byte[] pixels = new byte[height * stride];
        //        bmpSource.CopyPixels(pixels, stride, 0);

        //        for (int i = 0; i < pixels.Length; i += 4)
        //        {
        //            byte b = pixels[i];
        //            byte g = pixels[i + 1];
        //            byte r = pixels[i + 2];
        //            byte a = pixels[i + 3];

        //            if (a == 0)
        //            {
        //                pixels[i] = 0;
        //                pixels[i + 1] = 0;
        //                pixels[i + 2] = 0;
        //            }
        //            else if (a < 255)
        //            {
        //                // double精度の逆計算を走り込ませ、WPFの標準エンコーダーによる「丸め誤差」を完全に消し去ります。
        //                double alphaFactor = 255.0 / a;

        //                // Math.Clamp と四捨五入（+0.5）で、元のRGB値を1ビットの狂いもなく復元します
        //                pixels[i] = (byte)Math.Clamp((int)(b * alphaFactor + 0.5), 0, 255); // B
        //                pixels[i + 1] = (byte)Math.Clamp((int)(g * alphaFactor + 0.5), 0, 255); // G
        //                pixels[i + 2] = (byte)Math.Clamp((int)(r * alphaFactor + 0.5), 0, 255); // R
        //            }
        //            // A=255 の場合は元のRGBが100%維持されているため計算不要
        //        }

        //        // 7. 補正をかけ終えた正確なBgra32ピクセルを、GDI+のBitmapへ直接ロック転送
        //        var bitmapData = gdiBitmap.LockBits(
        //            new System.Drawing.Rectangle(0, 0, width, height),
        //            System.Drawing.Imaging.ImageLockMode.WriteOnly,
        //            gdiBitmap.PixelFormat);

        //        try
        //        {
        //            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, bitmapData.Scan0, pixels.Length);
        //        }
        //        finally
        //        {
        //            gdiBitmap.UnlockBits(bitmapData);
        //        }

        //        // 8. WPFのバグだらけのエンコーダーを完全にバイパスし、GDI+ネイティブでPNG保存
        //        gdiBitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
        //    }

        //    // 9. 処理終了後、UIが崩れないように再レイアウトを要求
        //    element.InvalidateMeasure();
        //}









        private static BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap bitmap)
        {
            BitmapSource bmp = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            
            return bmp;
        }


        public static BitmapSource SaveAsAccuratePng(RenderTargetBitmap rtb)
        {
            int width = rtb.PixelWidth;
            int height = rtb.PixelHeight;
            int stride = width * 4; // BGRA32は1ピクセル4バイト
            byte[] pixels = new byte[height * stride];

            // 1. Pbgra32 の生データを取得
            rtb.CopyPixels(pixels, stride, 0);

            // 2. 高精度な非乗算化（アンプレマルチプライ）処理
            for (int i = 0; i < pixels.Length; i += 4)
            {
                // Pbgra32 の並び順は B, G, R, A
                byte b = pixels[i];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];
                byte a = pixels[i + 3];

                if (a == 0)
                {
                    // アルファが0（完全透明）の場合、RGBは0にする（割り算不可のため）
                    pixels[i] = 0;
                    pixels[i + 1] = 0;
                    pixels[i + 2] = 0;
                }
                else if (a < 255)
                {
                    // 半透明の場合、float精度で逆計算して丸め誤差を最小限に抑える
                    float alphaFactor = 255f / a;

                    // Math.Min と四捨五入（+0.5f）で 255 を超えないよう安全にキャスト
                    pixels[i] = (byte)Math.Min(255, (int)(b * alphaFactor + 0.5f)); // B
                    pixels[i + 1] = (byte)Math.Min(255, (int)(g * alphaFactor + 0.5f)); // G
                    pixels[i + 2] = (byte)Math.Min(255, (int)(r * alphaFactor + 0.5f)); // R
                }
                // a == 255（不透明）の場合は、元のRGB値がそのまま維持されているので計算不要
            }

            // 3. 補正後のデータから直接 Bgra32 の BitmapSource を作成
            BitmapSource accurateBitmap = BitmapSource.Create(
                width, height,
                rtb.DpiX, rtb.DpiY,
                PixelFormats.Bgra32, // ここで正確な Bgra32 を指定
                null, pixels, stride
            );
            return accurateBitmap;

            //// 4. PNGファイルとして保存
            //PngBitmapEncoder encoder = new PngBitmapEncoder();
            //encoder.Frames.Add(BitmapFrame.Create(accurateBitmap));

            //using (FileStream stream = new FileStream(filePath, FileMode.Create))
            //{
            //    encoder.Save(stream);
            //}
        }


        // doubleを切り上げてintに変換
        public static int MyCeiling(double value)
        {
            return (int)Math.Ceiling(value);
        }






        /// <summary>
        /// 要素からDPIを計算
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static double GetDpiFromElement(FrameworkElement element)
        {
            //PresentationSource pre = PresentationSource.FromVisual(element);
            //Matrix matrix = pre.CompositionTarget.TransformFromDevice;
            //double dpi = 96.0 * matrix.M11;
            return 96.0 * PresentationSource.FromVisual(element).CompositionTarget.TransformFromDevice.M11;
        }
    }
}
