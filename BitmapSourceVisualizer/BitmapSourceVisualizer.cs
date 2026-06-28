using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

// この属性指定でも虫眼鏡リストに表示される
//[assembly: DebuggerVisualizer(
//    typeof(BitmapSourceVisualizer.BitmapSourceDebuggerVisualizer),
//    typeof(BitmapSourceVisualizer.BitmapSourceObjectSource),
//    Target = typeof(BitmapSource),
//    Description = "WPF BitmapSource Visualizer")]

[assembly: DebuggerVisualizer(
    typeof(BitmapSourceVisualizer.BitmapSourceDebuggerVisualizer),
    typeof(BitmapSourceVisualizer.BitmapSourceObjectSource),
    TargetTypeName = "System.Windows.Media.Imaging.BitmapSource",
    Target = typeof(BitmapSource),
    Description = "🐎 WPF BitmapSource Visualizer 🐎")]

// この属性指定では虫眼鏡リストに表示されない
//[assembly: DebuggerVisualizer(
//    typeof(BitmapSourceVisualizer.BitmapSourceDebuggerVisualizer),
//    typeof(BitmapSourceVisualizer.BitmapSourceObjectSource),
//    TargetTypeName = "System.Windows.Media.Imaging.BitmapSource",
//    Description = "WPF BitmapSource Visualizer")]

namespace BitmapSourceVisualizer
{
    public class BitmapSourceDebuggerVisualizer : DialogDebuggerVisualizer
    {
        protected override void Show(IDialogVisualizerService windowService,
                                     IVisualizerObjectProvider objectProvider)
        {
            if (objectProvider == null)
                throw new ArgumentNullException(nameof(objectProvider));

            BitmapSource bmp;
            double dpiX;
            double dpiY;
            string format;


            // ここで PNG バイト列が入った Stream を受け取る
            using Stream stream = objectProvider.GetData();

            using (var reader = new BinaryReader(stream, System.Text.Encoding.UTF8))
            {
                // 順番にデータを読み出す
                dpiX = reader.ReadDouble();
                dpiY = reader.ReadDouble();
                format = reader.ReadString();

                // BitmapSourceのデコード、配列を読み出して、BitmapFrameを使って復元
                int imageLength = reader.ReadInt32();
                byte[] imageBytes = reader.ReadBytes(imageLength);
                using (var imageStream = new MemoryStream(imageBytes))
                {
                    bmp = BitmapFrame.Create(imageStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
            }

            BitmapSourceVisualizerWindow win = new();
            win.SetImage(bmp);
            // タイトルバーに色々表示
            win.Title = $" BitmapSource Visualizer {{ PixelSize {bmp.PixelWidth} x {bmp.PixelHeight} }} , {{ PixelFormat {format} }} , {{ DpiX, DpiY {dpiX}, {dpiY} }}";
            win.ShowDialog();
        }
    }


    //public class BitmapSourceDebuggerVisualizer : DialogDebuggerVisualizer
    //{
    //    protected override void Show(IDialogVisualizerService windowService,
    //                                 IVisualizerObjectProvider objectProvider)
    //    {
    //        if (objectProvider == null)
    //            throw new ArgumentNullException(nameof(objectProvider));

    //        // ここで PNG バイト列が入った Stream を受け取る
    //        using Stream stream = objectProvider.GetData();

    //        BitmapImage bmp = new();
    //        bmp.BeginInit();
    //        bmp.CacheOption = BitmapCacheOption.OnLoad;
    //        bmp.StreamSource = stream;   // ← ここはそのまま Stream を渡す
    //        bmp.EndInit();
    //        bmp.Freeze();

    //        BitmapSourceVisualizerWindow win = new();
    //        win.SetImage(bmp);
    //        win.ShowDialog();
    //    }
    //}
}


