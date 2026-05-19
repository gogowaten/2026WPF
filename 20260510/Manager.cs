using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace _20260510
{
    public static class Manager
    {

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

        // MyContentからBitmap作成
        public static RenderTargetBitmap? MakeBitmapFromElement(double width, double height, FrameworkElement item)
        {
            int w = (int)width;
            int h = (int)height;
            double dpi = 96.0 * PresentationSource.FromVisual(item).CompositionTarget.TransformFromDevice.M11;
            RenderTargetBitmap bmp = new(w, h, dpi, dpi, PixelFormats.Pbgra32);
            bmp.Render(item);
            return bmp;
        }

        //// MyContentからBitmap作成
        //public static RenderTargetBitmap? MakeMyContentRenderBitmap(CustomThumb item)
        //{
        //    if (item?.MyData is Data MyData)
        //    {
        //        int width = (int)MyData.Width;
        //        int height = (int)MyData.Height;
        //        double dpi = MyData.RootData is null ? 96.0 : MyData.RootData.MyDPI;
        //        RenderTargetBitmap bmp = new(width, height, dpi, dpi, PixelFormats.Pbgra32);
        //        bmp.Render(item.MyContent);
        //        return bmp;
        //    }
        //    return null;
        //}


    }
}
