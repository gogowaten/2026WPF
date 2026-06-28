using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace BitmapSourceVisualizer
{
    public class BitmapSourceObjectSource : VisualizerObjectSource
    {
        // dpiとピクセルフォーマットも送信する方法
        public override void GetData(object target, Stream outgoingData)
        {
            if (target is not BitmapSource bmp) { base.GetData(target, outgoingData); return; }

            using (var writer = new BinaryWriter(outgoingData, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                // BitmapSourceをStreamにして、それをbyte型配列にする
                byte[] imageBytes;
                using (var imageStream = new MemoryStream())
                {
                    PngBitmapEncoder encoder = new();
                    encoder.Frames.Add(BitmapFrame.Create(bmp));
                    encoder.Save(imageStream);
                    imageBytes = imageStream.ToArray();
                }
                writer.Write(bmp.DpiX);// dpiを書き込む
                writer.Write(bmp.DpiY);
                writer.Write(bmp.Format.ToString());
                writer.Write(imageBytes.Length);// BitmapSourceのbyte型配列の長さも記録、受信時のデコードで必要
                writer.Write(imageBytes);
            }
        }

        //// BitmapSourceだけをStreamにして送信する方法、
        //// これだと送信時にdpiやピクセルフォーマットが変化してしまう
        //public override void GetData(object target, Stream outgoingData)
        //{
        //    if (target is not BitmapSource bmp)
        //    {
        //        base.GetData(target, outgoingData);
        //        return;
        //    }

        //    var encoder = new PngBitmapEncoder();
        //    encoder.Frames.Add(BitmapFrame.Create(bmp));

        //    // MemoryStream を使わずに直接書き込む
        //    encoder.Save(outgoingData);
        //}


        //public override void GetData(object target, Stream outgoingData)
        //{
        //    if (target is not BitmapSource bmp)
        //    {
        //        base.GetData(target, outgoingData);
        //        return;
        //    }

        //    // BitmapSource → PNG バイト列
        //    var encoder = new PngBitmapEncoder();
        //    encoder.Frames.Add(BitmapFrame.Create(bmp));

        //    using (var ms = new MemoryStream())
        //    {
        //        encoder.Save(ms);
        //        var bytes = ms.ToArray();
        //        outgoingData.Write(bytes, 0, bytes.Length);
        //    }
        //}


    }
}