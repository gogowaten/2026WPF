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

namespace _20260628
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private string imagePath = "C:\\Users\\waten\\Documents\\0511_item.jpg";
        private string imagePath = "C:\\Users\\waten\\Documents\\0511_item.tiff";
        //private string imagePath = "D:\\ブログ用\\テスト用画像\\hueRect060.png";
        private BitmapSource MyBitmapSource;

        public MainWindow()
        {
            InitializeComponent();

            BitmapImage img = new();
            img.BeginInit();
            img.UriSource = new Uri(imagePath);
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.EndInit();
            MyBitmapSource = img;

            MemoryStream value = PackData(MyBitmapSource, MyBitmapSource.DpiX);
            var (bitmap, number) = UnpackData(value);
            MyImage.Source = bitmap;
            double dpi = number;

            PngBitmapEncoder encoder = new();
            //TiffBitmapEncoder encoder = new();
            using MemoryStream stream = new();
            encoder.Frames.Add(BitmapFrame.Create(MyBitmapSource));
            encoder.Save(stream);

            BitmapImage bmg = new();
            bmg.BeginInit();
            bmg.StreamSource = stream;
            bmg.CacheOption = BitmapCacheOption.OnLoad;
            bmg.EndInit();
            var pngDpi = bmg.DpiX;


        }

        public MemoryStream PackData(BitmapSource bitmapSource, double number)
        {
            var ms = new MemoryStream();

            // BinaryWriterを使ってストリームに書き込む
            using (var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                // 1. まずBitmapSourceを一度別のメモリにPNGとして保存し、サイズを確定させる
                byte[] imageBytes;
                using (var imageStream = new MemoryStream())
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    encoder.Save(imageStream);
                    imageBytes = imageStream.ToArray();
                }

                // 2. 画像のバイト数を書き込む（受け取り側が何バイト読めばいいか知るため）
                writer.Write(imageBytes.Length);

                // 3. 画像のデータ本体を書き込む
                writer.Write(imageBytes);

                // 4. 任意の型の値を書き込む (double型)
                writer.Write(number);
            }

            // ストリームの位置を先頭に戻して返す
            ms.Position = 0;
            return ms;
        }

        public (BitmapSource bitmap, double number) UnpackData(Stream stream)
        {
            BitmapSource bitmap;// = null;
            double number = 0;

            // BinaryReaderを使ってストリームから読み込む
            using (BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: true))
            {
                // 1. 最初に画像のバイト数を読み込む
                int imageLength = reader.ReadInt32(); // 4byte

                // 2. そのバイト数分だけ画像データを読み込む
                byte[] imageBytes = reader.ReadBytes(imageLength); // (1 * imageLength) byte

                // バイト配列からBitmapSourceを復元
                using (var imageStream = new MemoryStream(imageBytes))
                {
                    //var decoder = BitmapDecoder.Create(imageStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    //bitmap = decoder.Frames[0];
                    bitmap = BitmapFrame.Create(imageStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    
                }

                // 3. 次にあるdouble型の値を読み込む
                number = reader.ReadDouble(); // 8byte
            }

            return (bitmap, number);
        }
    }
}