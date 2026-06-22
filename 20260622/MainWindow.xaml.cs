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

namespace _20260622
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string MyPicturePath = "D:\\ブログ用\\テスト用画像\\collection3.png";
        //private string MyPicturePath = "D:\\ブログ用\\テスト用画像\\NEC_9011_2015_07_23_スイートバジル収穫.jpg";
        public MainWindow()
        {
            InitializeComponent();

            BitmapImage img = new(new Uri(MyPicturePath));
            MyImage.Source = img;

            GridOverlay.Source = CreatePixelGrid((int)MyScale, img.PixelWidth, img.PixelHeight);
        }

        // グリッド線画像作成
        /// <summary>
        /// 指定されたセルサイズとセル数から、グリッド線を描画した <see cref="WriteableBitmap"/> を生成します。
        /// </summary>
        /// <param name="cellSize">1セルあたりのピクセル数。正の整数を想定します。</param>
        /// <param name="width">横方向のセル数。</param>
        /// <param name="height">縦方向のセル数。</param>
        /// <returns>
        /// 横幅が <c>width * cellSize</c>、高さが <c>height * cellSize</c> の
        /// <see cref="WriteableBitmap"/> を返します。ピクセル形式は BGRA32、DPI は 96x96 です。
        /// グリッド線はセルの境界（セルサイズの倍数の位置）に描画されます。
        /// </returns>
        /// <remarks>
        /// - 線の色は半透明の薄い灰色（A=100, R=200, G=200, B=200）で固定されています。
        /// - 内部では (bmpWidth * bmpHeight * 4) バイトのピクセルバッファを確保し、<see cref="WriteableBitmap.WritePixels"/> で書き込んでいます。
        /// - パラメーターの妥当性検査（0以下の値など）は行っていないため、呼び出し側で正の整数を渡してください。
        /// - 大きなサイズを指定するとメモリ使用量が増加します（bmpWidth * bmpHeight * 4 バイト）。
        /// </remarks>
        private WriteableBitmap CreatePixelGrid(int cellSize, int width, int height)
        {
            int bmpWidth = width * cellSize;
            int bmpHeight = height * cellSize;

            WriteableBitmap wbitmap = new(bmpWidth, bmpHeight, 96, 96, PixelFormats.Bgra32, null);
            int stride = bmpWidth * 4;
            byte[] pixels = new byte[stride * bmpHeight];

            // グリッド線の色
            byte r = 200, g = 200, b = 200, a = 100;

            // セルの境界（右端または下端）に線を引く
            // 縦線描画
            for (int x = cellSize; x < bmpWidth; x += cellSize)
            {
                for (int y = 0; y < bmpHeight; y++)
                {
                    int index = (y * stride) + (x * 4);
                    pixels[index] = b;     // Blue
                    pixels[index + 1] = g; // Green
                    pixels[index + 2] = r; // Red
                    pixels[index + 3] = a; // Alpha
                }
            }

            // 横線
            for (int y = cellSize; y < bmpHeight; y += cellSize)
            {
                for (int x = 0; x < bmpWidth; x++)
                {
                    int index = (y * stride) + (x * 4);
                    pixels[index] = b;     // Blue
                    pixels[index + 1] = g; // Green
                    pixels[index + 2] = r; // Red
                    pixels[index + 3] = a; // Alpha
                }
            }

            wbitmap.WritePixels(new Int32Rect(0, 0, bmpWidth, bmpHeight), pixels, stride, 0);
            return wbitmap;
        }

        // 縦横まとめて描画するときはこれ
        //private WriteableBitmap CreatePixelGrid(int width , int height)
        //{
        //    // 1ピクセルを少し大きめのセル（例: 10x10ピクセル）として扱うグリッドを作ると綺麗に見えます
        //    // もしくは、1x1マスの右と下に線を引いた1ピクセル単位のBitmapを作成します
        //    int cellSize = 16; // 1セルのサイズ
        //    int bmpWidth = width * cellSize;
        //    int bmpHeight = height * cellSize;

        //    WriteableBitmap wbitmap = new(bmpWidth, bmpHeight, 96, 96, PixelFormats.Bgra32, null);
        //    int stride = bmpWidth * 4;
        //    byte[] pixels = new byte[stride * bmpHeight];

        //    // グリッド線の色（例：薄いグレー #33FFFFFF）
        //    byte r = 200, g = 200, b = 200, a = 100;

        //    for (int y = 0; y < bmpHeight; y++)
        //    {
        //        for (int x = 0; x < bmpWidth; x++)
        //        {
        //            // セルの境界（右端または下端）に線を引く
        //            if (x % cellSize == 0 || y % cellSize == 0)
        //            {
        //                int index = (y * stride) + (x * 4);
        //                pixels[index] = b;     // Blue
        //                pixels[index + 1] = g; // Green
        //                pixels[index + 2] = r; // Red
        //                pixels[index + 3] = a; // Alpha
        //            }
        //        }
        //    }

        //    wbitmap.WritePixels(new Int32Rect(0, 0, bmpWidth, bmpHeight), pixels, stride, 0);
        //    return wbitmap;
        //}


        public double MyScale
        {
            get { return (double)GetValue(MyScaleProperty); }
            set { SetValue(MyScaleProperty, value); }
        }
        public static readonly DependencyProperty MyScaleProperty =
            DependencyProperty.Register(nameof(MyScale), typeof(double), typeof(MainWindow), new PropertyMetadata(50.0));

    }
}