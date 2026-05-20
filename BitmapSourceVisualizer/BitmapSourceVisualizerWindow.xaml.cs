using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

// Visual Studio用、BitmapSourceVisualizerに「ファイルに保存」と「コピー」を追加した - 午後わてんのブログ
// https://gogowaten.hatenablog.com/entry/2026/05/20/233726

namespace BitmapSourceVisualizer
{
    /// <summary>
    /// BitmapSourceVisualizerWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class BitmapSourceVisualizerWindow : Window
    {
        public BitmapSource MyBitmapSource;
        public BitmapSourceVisualizerWindow()
        {
            InitializeComponent();
            ContextMenu = CreateContextMenu();
        }

        public void SetImage(BitmapSource bitmap)
        {
            MyBitmapSource = bitmap;
            ImageControl.Source = bitmap;
            Title = $"BitmapSource Visualizer - {bitmap.PixelWidth} x {bitmap.PixelHeight}";
        }

        /// <summary>
        /// 右クリックメニュー作成
        /// </summary>
        /// <returns></returns>
        private ContextMenu CreateContextMenu()
        {
            ContextMenu menu = new();
            MenuItem item = new() { Header = "コピー" };
            menu.Items.Add(item);
            item.Click += (s, e) =>
            {
                if (MyBitmapSource is not null)
                {
                    BitmapToPngImageToClipboard(MyBitmapSource);
                }
            };
            item = new() { Header = "保存(png)" };
            menu.Items.Add(item);
            item.Click += (s, e) =>
            {
                if (MyBitmapSource is not null)
                {
                    SaveBitmapSource(MyBitmapSource);
                }
            };

            return menu;
        }

        //アルファ値を失わずに画像のコピペできた、.NET WPFのClipboard - 午後わてんのブログ
        //        https://gogowaten.hatenablog.com/entry/2021/02/10/134406
        //より
        private static void BitmapToPngImageToClipboard(BitmapSource source)
        {
            //画像をPNGにエンコード
            PngBitmapEncoder pngEnc = new();
            pngEnc.Frames.Add(BitmapFrame.Create(source));
            //エンコードした画像をMemoryStreamにSava
            using var ms = new System.IO.MemoryStream();
            pngEnc.Save(ms);
            //MemoryStreamをクリップボードにコピー
            Clipboard.SetData("PNG", ms);
        }

        private void ButtonCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource is not null)
            {
                BitmapToPngImageToClipboard(MyBitmapSource);
            }
        }

        private void ButtonChangeStretchNone_Click(object sender, RoutedEventArgs e)
        {
            ImageControl.Stretch = Stretch.None;
        }
        private void ButtonChangeStretchUniform_Click(object sender, RoutedEventArgs e)
        {
            ImageControl.Stretch = Stretch.Uniform;
        }
        private void ButtonChangeStretchFill_Click(object sender, RoutedEventArgs e)
        {
            ImageControl.Stretch = Stretch.Fill;
        }
        private void ButtonChangeStretchUniformToFill_Click(object sender, RoutedEventArgs e)
        {
            ImageControl.Stretch = Stretch.UniformToFill;
        }


        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveBitmapSource(MyBitmapSource);
        }

        private void SaveBitmapSource(BitmapSource bitmap)
        {
            Microsoft.Win32.SaveFileDialog dialog = new()
            {
                AddExtension = true,
                DefaultExt = "png",
            };

            if (dialog.ShowDialog() == true)
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using FileStream stream = new(dialog.FileName, FileMode.Create, FileAccess.Write);
                encoder.Save(stream);
            }
        }

        //private void ButtonEdge_Click(object sender, RoutedEventArgs e)
        //{
        //    if (RenderOptions.GetEdgeMode(ImageControl) == EdgeMode.Aliased)
        //    {
        //        RenderOptions.SetEdgeMode(ImageControl, EdgeMode.Unspecified);
        //    }
        //    else
        //    {
        //        RenderOptions.SetEdgeMode(ImageControl, EdgeMode.Aliased);
        //    }
        //}

        private void ButtonScalingModeNearestNeighbor_Click(object sender, RoutedEventArgs e)
        {
            RenderOptions.SetBitmapScalingMode(ImageControl, BitmapScalingMode.NearestNeighbor);
        }
        private void ButtonScalingModeLinear_Click(object sender, RoutedEventArgs e)
        {
            RenderOptions.SetBitmapScalingMode(ImageControl, BitmapScalingMode.Linear);
        }

        private void ButtonScalingModeFant_Click(object sender, RoutedEventArgs e)
        {
            RenderOptions.SetBitmapScalingMode(ImageControl, BitmapScalingMode.Fant);
        }

    }
}
