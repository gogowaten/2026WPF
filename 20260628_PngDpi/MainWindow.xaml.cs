using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace _20260628_PngDpi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DpiTest();
        }

        private void DpiTest()
        {
            // 1x1ピクセルのBitmapSource作成
            byte[] pixels = [200, 1, 1, 1];
            //BitmapSource source = BitmapSource.Create(1, 1, 96.0, 96.0, PixelFormats.Pbgra32, null, pixels, 4);
            BitmapSource source = BitmapSource.Create(1, 1, 96.0, 96.0, PixelFormats.Bgra32, null, pixels, 4);
            //BitmapSource source = BitmapSource.Create(1, 1, 96.0, 96.0, PixelFormats.Rgb24, null, pixels, 3);
            //BitmapSource source = BitmapSource.Create(1, 1, 96.0, 96, PixelFormats.Gray8, null, pixels, 4);

            double sourceDpi = source.DpiX;
            var sourcePF = source.Format;
            var png = GetFormatAndDpi(source, new PngBitmapEncoder());
            var bmp = GetFormatAndDpi(source, new BmpBitmapEncoder());
            var tiff = GetFormatAndDpi(source, new TiffBitmapEncoder());
            var gif = GetFormatAndDpi(source, new GifBitmapEncoder());
            //var jpegDpi = GetDpi(source, new JpegBitmapEncoder()); // デコード時にヘッダーエラーとかになる
        }

        private (PixelFormat, double) GetFormatAndDpi(BitmapSource source, BitmapEncoder encoder)
        {
            // 指定エンコーダで変換した後、復元したBitmapSourceのピクセルフォーマットとdpiを返す
            // 変換
            using MemoryStream stream = new();
            encoder.Frames.Add(BitmapFrame.Create(source));
            encoder.Save(stream);
            
            // 復元
            BitmapFrame bmp = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            var format = bmp.Format;
            return (bmp.Format, bmp.DpiX);
        }






        //private double GetDpi(BitmapSource source, BitmapEncoder encoder)
        //{
        //    encoder.Frames.Add(BitmapFrame.Create(source));
        //    using MemoryStream stream = new();
        //    encoder.Save(stream);

        //    BitmapDecoder decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        //    return decoder.Frames[0].DpiX;
        //}

        //private double GetDpi(BitmapSource source, BitmapEncoder encoder)
        //{
        //    using MemoryStream stream = new();
        //    encoder.Frames.Add(BitmapFrame.Create(source));
        //    encoder.Save(stream);

        //    BitmapImage bitmapImage = new();
        //    bitmapImage.BeginInit();
        //    bitmapImage.StreamSource = stream;
        //    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        //    bitmapImage.EndInit();

        //    return bitmapImage.DpiX;
        //}

    }
}