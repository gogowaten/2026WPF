using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace BitmapSourceVisualizer
{
    public class BitmapSourceObjectSource : VisualizerObjectSource
    {
        public override void GetData(object target, Stream outgoingData)
        {
            if (target is not BitmapSource bmp)
            {
                base.GetData(target, outgoingData);
                return;
            }

            // BitmapSource → PNG バイト列
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));

            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                var bytes = ms.ToArray();
                outgoingData.Write(bytes, 0, bytes.Length);
            }
        }
    }

    //public override void GetData(object target, Stream outgoingData)
    //    {
    //        if (target is not BitmapSource bmp)
    //        {
    //            base.GetData(target, outgoingData);
    //            return;
    //        }

    //        var encoder = new PngBitmapEncoder();
    //        encoder.Frames.Add(BitmapFrame.Create(bmp));

    //        // MemoryStream を使わずに直接書き込む
    //        encoder.Save(outgoingData);
    //    }

    }
