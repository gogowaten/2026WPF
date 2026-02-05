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

namespace _20260205_gammaGradient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MyImage.Source = CreateGammaCorrectedGradient(Colors.Red, Colors.Cyan, (int)MyImage.Width, (int)MyImage.Height);
        }

        // sRGB → Linear ：値の2.2乗
        // Linear → sRGB：値の1 / 2.2乗
        // R = 60をLenearにするときは、一旦255で割って正規化して2.2乗
        // 60/255=0.23529412、これを2.2乗で0.23529412^2.2=0.041451893

        /// <summary>
        /// 2色間の水平グラデーションビットマップを作成し、ガンマ補正を適用して知覚的に正確な色のブレンドを実現します。
        /// </summary>
        /// <remarks>ガンマ補正は、RGB空間で線形にグラデーションが変化するのに対し、知覚される明るさでグラデーションが滑らかに変化するようにします。
        /// グラデーションは水平方向にレンダリングされ、各垂直列は color1 と color2 の間の補間色を表します。</remarks>
        /// <param name="color1">グラデーションの開始色。ビットマップの左端に表示されます。</param>
        /// <param name="color2">グラデーションの終了色。ビットマップの右端に表示されます。</param>
        /// <param name="width">結果のビットマップの幅（ピクセル単位）。0より大きい必要があります。</param>
        /// <param name="height">結果のビットマップの高さ（ピクセル単位）。 0より大きい必要があります。</param>
        /// <returns>color1からcolor2へのガンマ補正されたグラデーションを含むBitmapSource。ビットマップは32ビットBGRAピクセル形式を使用します。
        /// </returns>
        public BitmapSource CreateGammaCorrectedGradient(Color color1, Color color2, int width, int height)
        {
            var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            int[] pixels = new int[width * height];

            // 1. 各色のリニア値を計算 (2.2乗)
            double r1 = Math.Pow(color1.R / 255.0, 2.2);
            double g1 = Math.Pow(color1.G / 255.0, 2.2);
            double b1 = Math.Pow(color1.B / 255.0, 2.2);

            double r2 = Math.Pow(color2.R / 255.0, 2.2);
            double g2 = Math.Pow(color2.G / 255.0, 2.2);
            double b2 = Math.Pow(color2.B / 255.0, 2.2);

            for (int x = 0; x < width; x++)
            {
                double t = (double)x / (width - 1);

                // 2. リニア空間での線形補間
                double rL = r1 + ((r2 - r1) * t);
                double gL = g1 + ((g2 - g1) * t);
                double bL = b1 + ((b2 - b1) * t);

                // 3. sRGB空間に戻す (1/2.2乗)
                byte r = (byte)(Math.Pow(rL, 1.0 / 2.2) * 255);
                byte g = (byte)(Math.Pow(gL, 1.0 / 2.2) * 255);
                byte b = (byte)(Math.Pow(bL, 1.0 / 2.2) * 255);

                // ビットシフト、255 << 24はアルファ値
                // "|"は論理和
                // BGRAの4つのbyte型値を連結してint型に入れている
                int colorInt = (255 << 24) | (r << 16) | (g << 8) | b;

                for (int y = 0; y < height; y++)
                {
                    pixels[y * width + x] = colorInt;
                }
            }

            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);
            return bitmap;
        }

        private void Gradient_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}