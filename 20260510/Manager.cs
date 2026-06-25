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
            // Bgra32は非対応。PixelFormats.Defaultの中身はPbgra32
            //RenderTargetBitmap bmp2 =
            //    new(MyCeiling(ltBounds.Width), MyCeiling(ltBounds.Height), dpi, dpi, PixelFormats.Bgra32);


            bmp.Render(dv);

            var bmp2 = new FormatConvertedBitmap(bmp, PixelFormats.Gray8, null, 0);
            BitmapPalette palette = new(bmp, 4);
            var bmp3 = new FormatConvertedBitmap(bmp, PixelFormats.Indexed2, palette, 0);
            var bmp4 = new FormatConvertedBitmap(bmp, PixelFormats.Bgr24, null, 0);
            var bmp5 = new FormatConvertedBitmap(bmp, PixelFormats.BlackWhite, null, 0);

            return bmp;
        }


        //private static BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap bitmap)
        //{
        //    BitmapSource bmp = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            
        //    return bmp;
        //}


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
