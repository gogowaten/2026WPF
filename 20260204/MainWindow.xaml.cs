using Microsoft.Win32;
using System.Globalization;
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

namespace _20260204
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapImage? _originalBitmap; // 加工しない「マスター」
        private WriteableBitmap? _targetBitmap; // 実際に加工して表示する用
        private byte[]? _basePixels; // BGRA32形式の生データを保持しておく

        public MainWindow()
        {
            InitializeComponent();
        }


        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. ファイル選択ダイアログの設定
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "画像ファイル|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // BitmapImageを作成して画像を読み込む
                    _originalBitmap = new BitmapImage();
                    _originalBitmap.BeginInit();
                    // メモリリーク防止：ファイルをロックしないように設定
                    _originalBitmap.CacheOption = BitmapCacheOption.OnLoad;
                    _originalBitmap.UriSource = new Uri(openFileDialog.FileName);
                    _originalBitmap.EndInit();

                    // ★重要: GIF等の特殊な形式を、標準的なBGRA32形式に変換する
                    var converted = new FormatConvertedBitmap(_originalBitmap, PixelFormats.Bgra32, null, 0);
                    
                    // 加工用の WriteableBitmap を作成
                    _targetBitmap = new WriteableBitmap(converted);

                    // ★重要: オリジナルの「標準形式ピクセルデータ」を配列に保存しておく
                    int stride = _targetBitmap.BackBufferStride;
                    _basePixels = new byte[_targetBitmap.PixelHeight * stride];
                    converted.CopyPixels(_basePixels, stride, 0);


                    // 画面に表示
                    MainImage.Source = _targetBitmap;

                }
                catch (Exception ex)
                {
                    MessageBox.Show("画像の読み込みに失敗しました: " + ex.Message);
                }
            }
        }

        private void ApplyGrayscale()
        {
            if (_targetBitmap == null)
            {
                return;
            }

            // 編集を開始
            _targetBitmap.Lock();

            int width = _targetBitmap.PixelWidth;
            int height = _targetBitmap.PixelHeight;
            int stride = _targetBitmap.BackBufferStride; // 1行あたりのバイト数
            IntPtr pBuffer = _targetBitmap.BackBuffer;   // メモリの先頭ポインタ

            unsafe
            {
                byte* pScanline = (byte*)pBuffer.ToPointer();

                for (int y = 0; y < height; y++)
                {
                    byte* pPixel = pScanline;
                    for (int x = 0; x < width; x++)
                    {
                        // 標準的なBGRA32形式（Blue, Green, Red, Alpha）
                        byte b = pPixel[0];
                        byte g = pPixel[1];
                        byte r = pPixel[2];

                        // 輝度計算（簡易版）
                        byte gray = (byte)(0.299 * r + 0.587 * g + 0.114 * b);

                        // RGBすべてに同じ値を書き込む
                        pPixel[0] = gray; // B
                        pPixel[1] = gray; // G
                        pPixel[2] = gray; // R

                        pPixel += 4; // 次のピクセルへ（4バイト移動）
                    }
                    pScanline += stride; // 次の行へ
                }
            }

            // 変更を通知してロック解除
            _targetBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            _targetBitmap.Unlock();
        }

        private void GrayScaleButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyGrayscale();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (_originalBitmap == null) { return; }

            // オリジナルから再度 WriteableBitmap を生成して上書き
            _targetBitmap = new WriteableBitmap(_originalBitmap);

            // UI側のソースも更新（念のため）
            MainImage.Source = _targetBitmap;
        }

        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // 画像が読み込まれていない場合は何もしない
            if (_originalBitmap == null || _targetBitmap == null) return;

            int offset = (int)e.NewValue; // スライダーの値（-255 ～ 255）
            ApplyBrightness(offset);
        }

        private void ApplyBrightness(int offset)
        {
            if (_targetBitmap == null || _basePixels == null) return;

            _targetBitmap.Lock();
            IntPtr pTarget = _targetBitmap.BackBuffer;

            unsafe
            {
                byte* pDest = (byte*)pTarget.ToPointer();

                // すでにBGRA32形式で統一されているので、単純にループを回すだけ
                for (int i = 0; i < _basePixels.Length; i += 4)
                {
                    pDest[i + 0] = (byte)Math.Clamp(_basePixels[i + 0] + offset, 0, 255); // B
                    pDest[i + 1] = (byte)Math.Clamp(_basePixels[i + 1] + offset, 0, 255); // G
                    pDest[i + 2] = (byte)Math.Clamp(_basePixels[i + 2] + offset, 0, 255); // R
                    pDest[i + 3] = _basePixels[i + 3]; // Alphaは元のまま
                }
            }

            _targetBitmap.AddDirtyRect(new Int32Rect(0, 0, _targetBitmap.PixelWidth, _targetBitmap.PixelHeight));
            _targetBitmap.Unlock();
        }


    }
}