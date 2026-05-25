using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace _20260510
{


    public static class Manager
    {

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
            bmp.Render(dv);
            return bmp;
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
