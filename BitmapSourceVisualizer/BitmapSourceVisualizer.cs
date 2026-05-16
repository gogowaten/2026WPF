using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.Diagnostics;
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
    Description = "🐎WPF BitmapSource Visualizer")]

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

            // ここで PNG バイト列が入った Stream を受け取る
            using var stream = objectProvider.GetData();

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.StreamSource = stream;   // ← ここはそのまま Stream を渡す
            bmp.EndInit();
            bmp.Freeze();

            var win = new BitmapSourceVisualizerWindow();
            win.SetImage(bmp);
            win.ShowDialog();
        }
    }
}


