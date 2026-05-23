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

        // 要素からBitmap作成
        public static RenderTargetBitmap? MakeBitmapFromElement2(FrameworkElement item, FrameworkElement parent)
        {
            var anc = item.TransformToAncestor(parent);
            var des = parent.TransformToDescendant(item);
            var lay = item.LayoutTransform;
            var ren = item.RenderTransform;

            Rect orir = new(0, 0, item.ActualWidth, item.ActualHeight);
            List<Rect> rects = [];
            if (lay is TransformGroup group)
            {
                TransformCollection chil = group.Children;
                foreach (var tfc in group.Children)
                {
                    orir = tfc.TransformBounds(orir);
                    rects.Add(tfc.TransformBounds(new(0, 0, item.ActualWidth, item.ActualHeight)));
                }
            }


            double dpi = 96.0 * PresentationSource.FromVisual(item).CompositionTarget.TransformFromDevice.M11;
            GeneralTransform TF = item.TransformToVisual(parent);
            Rect bounds = TF.TransformBounds(new Rect(0, 0, item.ActualWidth, item.ActualHeight));
            Rect rect = new(new Point(), bounds.Size);
            DrawingVisual dv = new();
            using (var context = dv.RenderOpen())
            {
                VisualBrush brush = new(item);
                context.DrawRectangle(brush, null, rect);
            }

            RenderTargetBitmap bmp = new((int)bounds.Width, (int)bounds.Height, dpi, dpi, PixelFormats.Pbgra32);
            bmp.Render(dv);
            return bmp;
        }


    }
}
