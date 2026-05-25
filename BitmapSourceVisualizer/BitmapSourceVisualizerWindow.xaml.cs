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
        private readonly double ImageScaleMin = 0.01; // 拡大率下限
        private readonly double ImageScaleMax = 50.0; // 拡大率上限

        public BitmapSourceVisualizerWindow()
        {
            InitializeComponent();
            ContextMenu = CreateContextMenu();
            DataContext = this;
        }


        public double MyImageScale
        {
            get { return (double)GetValue(MyImageScaleProperty); }
            set { SetValue(MyImageScaleProperty, value); }
        }
        public static readonly DependencyProperty MyImageScaleProperty =
            DependencyProperty.Register(nameof(MyImageScale), typeof(double), typeof(Window), new PropertyMetadata(1.0));


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
            MenuItem item = new() { Header = "コピー(等倍)" };
            menu.Items.Add(item);
            item.Click += (s, e) =>
            {
                if (MyBitmapSource is not null)
                {
                    BitmapToPngImageToClipboard(MyBitmapSource);
                }
            };

            item = new() { Header = "コピー(拡大後)" };
            menu.Items.Add(item);
            item.Click += (s, e) =>
            {
                if (MyBitmapSource is not null)
                {
                    CopyToClipboardExterior(ImageControl);
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

        // 要素からBitmap作成
        public static RenderTargetBitmap MakeBitmapFromElement(double width, double height, FrameworkElement item)
        {
            int w = (int)width;
            int h = (int)height;
            double dpi = 96.0 * PresentationSource.FromVisual(item).CompositionTarget.TransformFromDevice.M11;
            RenderTargetBitmap bmp = new(w, h, dpi, dpi, PixelFormats.Pbgra32);
            bmp.Render(item);
            return bmp;
        }


        private void ButtonCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource is not null)
            {
                BitmapToPngImageToClipboard(MyBitmapSource);
            }
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



        private void ButtonSetScale_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag.ToString(), out int scale))
            {
                MyImageScale = scale;
            }
        }

        private void ButtonSetMathScale_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && double.TryParse(button.Tag.ToString(), out double scale))
            {
                MyImageScale = Clamp(MyImageScale * scale, ImageScaleMin, ImageScaleMax);
            }
        }

        // クランプ。値を上限下限内に収めて返す
        private static double Clamp(double value, double min, double max)
        {
            if (min > max)
            {
                (max, min) = (min, max);
            }

            double result = value;
            if (value < min) { result = min; }
            else if (value > max) { result = max; }
            return result;
        }

        // 完成版：要素からBitmap作成、LayoutTransformによる回転拡大対応
        public static RenderTargetBitmap MakeBitmapFromLayoutTransformElement(FrameworkElement item)
        {
            double dpi = 96.0 * PresentationSource.FromVisual(item).CompositionTarget.TransformFromDevice.M11;
            Rect bounds = new(0, 0, item.ActualWidth, item.ActualHeight); // 元のBounds
            var ltBounds = item.LayoutTransform.TransformBounds(bounds); // 変形後のBounds
            DrawingVisual dv = new();
            dv.Offset = new Vector(-ltBounds.X, -ltBounds.Y);

            using (DrawingContext context = dv.RenderOpen())
            {
                VisualBrush brush = new(item) { Stretch = Stretch.None };
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

        // LayoutTransformによる変形後の要素からBitmap作成して、クリップボードにコピー
        private void ButtonCopyToClipboardExterior_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboardExterior(ImageControl);
        }

        // LayoutTransformによる変形後の要素からBitmap作成して、クリップボードにコピー
        public static void CopyToClipboardExterior(FrameworkElement element)
        {
            BitmapToPngImageToClipboard(MakeBitmapFromLayoutTransformElement(element));
        }
    }
}
