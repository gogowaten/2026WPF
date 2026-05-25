using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20260524_01_ElementToBitmap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public BitmapSource MyBitmapSource;
        private readonly double ImageScaleMin = 0.01;
        private readonly double ImageScaleMax = 50.0;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            BitmapImage bi = new(new Uri("C:\\Users\\waten\\Documents\\20260518_221026.png"));
            
            MyBitmapSource = bi;
            ImageControl.Source = MyBitmapSource;
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

        // 試験版：要素からBitmap作成、LayoutTransformによる回転拡大対応
        public static RenderTargetBitmap MakeBitmapFromElement2(FrameworkElement item, FrameworkElement parent)
        {
            double dpi = 96.0 * PresentationSource.FromVisual(item).CompositionTarget.TransformFromDevice.M11;

            Rect bounds = new(0, 0, item.ActualWidth, item.ActualHeight);
            Rect itemBounds = VisualTreeHelper.GetContentBounds(item); // ActualSizeと同じ
            GeneralTransform TFV = item.TransformToVisual(parent);
            GeneralTransform TFA = item.TransformToAncestor(parent); // TFVと同じ、Parentのみ対応
            GeneralTransform TFD = parent.TransformToDescendant(item); // これは違う
            Transform LT = item.LayoutTransform; // これで十分
            var tfvBounds = TFV.TransformBounds(bounds);
            var tfaBounds = TFA.TransformBounds(bounds);
            var tfdBounds = TFD.TransformBounds(bounds);
            var ltBounds = item.LayoutTransform.TransformBounds(bounds);

            // オフセットする
            DrawingVisual dv = new();
            dv.Offset = new Vector(-ltBounds.X, -ltBounds.Y);

            using (DrawingContext context = dv.RenderOpen())
            {

                //VisualBrush brush = new(item);
                //context.DrawRectangle(brush, null, tfvBounds);
                //context.DrawRectangle(new VisualBrush(item), null, tfvBounds);

                // これがいい
                context.DrawRectangle(new VisualBrush(item), null, ltBounds);

                // BitmapCacheBrush、これだとぼやけるし、変なところで上下がループした画像になる
                //context.DrawRectangle(new BitmapCacheBrush(item), null, ltBounds);
            }
            //RenderTargetBitmap bmp = new((int)tfvBounds.Width, (int)tfvBounds.Height, dpi, dpi, PixelFormats.Pbgra32);
            RenderTargetBitmap bmp =
                new(MyCeiling(ltBounds.Width), MyCeiling(ltBounds.Height), dpi, dpi, PixelFormats.Pbgra32);
            bmp.Render(dv);
            return bmp;
        }

        // 完成版：要素からBitmap作成、LayoutTransformによる回転拡大対応
        public static RenderTargetBitmap MakeBitmapFromElement3(FrameworkElement item)
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
                //context.DrawRectangle(new VisualBrush(item), null, ltBounds);
            }
            RenderTargetBitmap bmp =
                new(MyCeiling(ltBounds.Width), MyCeiling(ltBounds.Height), dpi, dpi, PixelFormats.Pbgra32);
            bmp.Render(dv);
            return bmp;
        }

        public static int MyCeiling(double value)
        {
            return (int)Math.Ceiling(value);
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //var bmp = MakeBitmapFromElement2(ImageControl, this);
            var bmp = MakeBitmapFromElement3(ImageControl);
            BitmapToPngImageToClipboard(bmp);
        }
    }
}