using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

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
    Description = "WPF BitmapSource Visualizer")]

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





//using Microsoft.VisualStudio.DebuggerVisualizers;
//using System;
//using System.Diagnostics;
//using System.Windows.Media.Imaging;

//[assembly: DebuggerVisualizer(
//    typeof(BitmapSourceVisualizer.BitmapSourceDebuggerVisualizer),
//    typeof(VisualizerObjectSource),
//    Target = typeof(BitmapSource),
//    Description = "WPF BitmapSource Visualizer")]

//namespace BitmapSourceVisualizer
//{
//    public class BitmapSourceDebuggerVisualizer : DialogDebuggerVisualizer
//    {
//        protected override void Show(IDialogVisualizerService windowService,
//                                     IVisualizerObjectProvider objectProvider)
//        {
//            if (windowService == null)
//                throw new ArgumentNullException(nameof(windowService));

//            if (objectProvider == null)
//                throw new ArgumentNullException(nameof(objectProvider));

//            var bmp = objectProvider.GetObject() as BitmapSource;
//            if (bmp == null)
//            {
//                System.Windows.MessageBox.Show(
//                    "BitmapSource ではありません。",
//                    "BitmapSource Visualizer");
//                return;
//            }

//            var win = new BitmapSourceVisualizerWindow();
//            win.SetImage(bmp);

//            //windowService.ShowDialog(win);
//            win.ShowDialog();
//        }
//    }
//}
