
using Microsoft.VisualStudio.DebuggerVisualizers;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WpfBitmapVisualizer;

namespace WpfBitmapVisualizer
{
    public class BitmapSourceObjectSource : VisualizerObjectSource
    {
        public override void GetData(object target, Stream outgoingData)
        {

            if (target is BitmapSource bitmapSource)
            {
                using var ms = new MemoryStream();
                // BitmapSourceをPNGにエンコード
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(ms);

                byte[] bytes = ms.ToArray();

                // デバッガー側にサイズとバイト配列を直接書き込む
                using var writer = new BinaryWriter(outgoingData);
                writer.Write(bytes.Length);
                writer.Write(bytes);
            }
        }
    }

}