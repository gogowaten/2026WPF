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

        private double _currentAngle = 0;   // 現在の回転角度
        private int _currentBrightness = 0; // 現在の明るさオフセット

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

        private void Rotate90Degrees()
        {
            if (_targetBitmap == null || _basePixels == null) return;

            // 現在のサイズを取得
            int oldWidth = _targetBitmap.PixelWidth;
            int oldHeight = _targetBitmap.PixelHeight;
            int oldStride = _targetBitmap.BackBufferStride;

            // 回転後のサイズ（幅と高さが逆転する）
            int newWidth = oldHeight;
            int newHeight = oldWidth;
            int newStride = newWidth * 4; // BGRA32想定

            byte[] newPixels = new byte[newHeight * newStride];

            // 回転アルゴリズム：(x, y) -> (newWidth - 1 - y, x)
            // ここでは unsafe を使わず、ロジックの分かりやすさと安全性のため配列操作で行います
            for (int y = 0; y < oldHeight; y++)
            {
                for (int x = 0; x < oldWidth; x++)
                {
                    int oldIdx = y * oldStride + x * 4;

                    // 右に90度回転後の座標
                    int newX = (newWidth - 1) - y;
                    int newY = x;
                    int newIdx = newY * newStride + newX * 4;

                    newPixels[newIdx + 0] = _basePixels[oldIdx + 0]; // B
                    newPixels[newIdx + 1] = _basePixels[oldIdx + 1]; // G
                    newPixels[newIdx + 2] = _basePixels[oldIdx + 2]; // R
                    newPixels[newIdx + 3] = _basePixels[oldIdx + 3]; // A
                }
            }

            // 1. 新しいサイズの WriteableBitmap を作成
            _targetBitmap = new WriteableBitmap(newWidth, newHeight, 96, 96, PixelFormats.Bgra32, null);

            // 2. 加工済みのピクセルデータを流し込む
            _targetBitmap.WritePixels(new Int32Rect(0, 0, newWidth, newHeight), newPixels, newStride, 0);

            // 3. ★重要：ベースピクセルを回転後のものに更新する
            // これにより、回転した状態でスライダー（明るさ）を動かせるようになります
            _basePixels = newPixels;

            // 4. 画面更新
            MainImage.Source = _targetBitmap;

            // スライダーの値をリセット（回転後の状態を0とする場合）
            BrightnessSlider.Value = 0;
        }

        private void ApplyFreeRotation(double angle)
        {
            if (_basePixels == null || _originalBitmap == null) return;

            // 度をラジアンに変換
            double radians = angle * Math.PI / 180.0;
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);

            int oldWidth = _originalBitmap.PixelWidth; // 元の幅
            int oldHeight = _originalBitmap.PixelHeight; // 元の高さ

            // 回転後のサイズを計算（対角線の長さでカバーする簡易版）
            int newWidth = (int)(Math.Abs(oldWidth * cos) + Math.Abs(oldHeight * sin));
            int newHeight = (int)(Math.Abs(oldWidth * sin) + Math.Abs(oldHeight * cos));
            int newStride = newWidth * 4;

            byte[] newPixels = new byte[newHeight * newStride];

            // 中心点
            double oldCenterX = oldWidth / 2.0;
            double oldCenterY = oldHeight / 2.0;
            double newCenterX = newWidth / 2.0;
            double newCenterY = newHeight / 2.0;

            // 出力画像の全ピクセルを走査
            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    // 出力座標を中心からの相対座標に変換
                    double dx = x - newCenterX;
                    double dy = y - newCenterY;

                    // 回転行列の逆行列を使って、元画像の座標を算出
                    double srcX = dx * cos + dy * sin + oldCenterX;
                    double srcY = -dx * sin + dy * cos + oldCenterY;

                    // 元画像の範囲内であればピクセルをコピー
                    if (srcX >= 0 && srcX < oldWidth - 1 && srcY >= 0 && srcY < oldHeight - 1)
                    {
                        // 簡易的な近傍補間（Nearest Neighbor）
                        int sx = (int)Math.Round(srcX);
                        int sy = (int)Math.Round(srcY);

                        int srcIdx = (sy * oldWidth + sx) * 4;
                        int destIdx = (y * newStride) + (x * 4);

                        if (srcIdx >= 0 && srcIdx < _basePixels.Length - 4)
                        {
                            Array.Copy(_basePixels, srcIdx, newPixels, destIdx, 4);
                        }
                    }
                }
            }

            // 描画
            _targetBitmap = new WriteableBitmap(newWidth, newHeight, 96, 96, PixelFormats.Bgra32, null);
            _targetBitmap.WritePixels(new Int32Rect(0, 0, newWidth, newHeight), newPixels, newStride, 0);
            MainImage.Source = _targetBitmap;

            // 明るさ調整用に現在の状態を保存
            // 注意：回転を繰り返すと画質が劣化するため、本来は original から計算するのが理想です
            _basePixels = newPixels;
        }

        private void ApplyTransformations()
        {
            if (_originalBitmap == null || _basePixels == null) return;

            // 1. オリジナルのピクセルデータ(basePixels)を元に回転処理を行う
            // 回転後のピクセルデータと、その時のサイズ情報を取得する
            var (rotatedPixels, newWidth, newHeight) = GetRotatedPixels(_basePixels, _originalBitmap.PixelWidth, _originalBitmap.PixelHeight, _currentAngle);

            // 2. 回転したデータに対して明るさを適用する
            int stride = newWidth * 4;
            for (int i = 0; i < rotatedPixels.Length; i += 4)
            {
                rotatedPixels[i + 0] = (byte)Math.Clamp(rotatedPixels[i + 0] + _currentBrightness, 0, 255); // B
                rotatedPixels[i + 1] = (byte)Math.Clamp(rotatedPixels[i + 1] + _currentBrightness, 0, 255); // G
                rotatedPixels[i + 2] = (byte)Math.Clamp(rotatedPixels[i + 2] + _currentBrightness, 0, 255); // R
                                                                                                            // Alphaはそのまま
            }

            // 3. WriteableBitmapを生成して表示
            _targetBitmap = new WriteableBitmap(newWidth, newHeight, 96, 96, PixelFormats.Bgra32, null);
            _targetBitmap.WritePixels(new Int32Rect(0, 0, newWidth, newHeight), rotatedPixels, stride, 0);
            MainImage.Source = _targetBitmap;
        }

        // 回転計算のみを行うロジックを分離
        private (byte[] pixels, int width, int height) GetRotatedPixels(byte[] srcPixels, int oldWidth, int oldHeight, double angle)
        {
            double radians = angle * Math.PI / 180.0;
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);

            int newWidth = (int)(Math.Abs(oldWidth * cos) + Math.Abs(oldHeight * sin));
            int newHeight = (int)(Math.Abs(oldWidth * sin) + Math.Abs(oldHeight * cos));
            if (newWidth < 1) newWidth = 1;
            if (newHeight < 1) newHeight = 1;

            byte[] newPixels = new byte[newHeight * newWidth * 4];

            double oldCenterX = oldWidth / 2.0;
            double oldCenterY = oldHeight / 2.0;
            double newCenterX = newWidth / 2.0;
            double newCenterY = newHeight / 2.0;

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    double dx = x - newCenterX;
                    double dy = y - newCenterY;

                    double srcX = dx * cos + dy * sin + oldCenterX;
                    double srcY = -dx * sin + dy * cos + oldCenterY;

                    if (srcX >= 0 && srcX < oldWidth - 1 && srcY >= 0 && srcY < oldHeight - 1)
                    {
                        int sx = (int)Math.Round(srcX);
                        int sy = (int)Math.Round(srcY);
                        int srcIdx = (sy * oldWidth + sx) * 4;
                        int destIdx = (y * newWidth + x) * 4;

                        if (srcIdx >= 0 && srcIdx <= srcPixels.Length - 4)
                        {
                            Array.Copy(srcPixels, srcIdx, newPixels, destIdx, 4);
                        }
                    }
                }
            }
            return (newPixels, newWidth, newHeight);
        }




        private void GrayScaleButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyGrayscale();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (_originalBitmap == null) { return; }

            //// オリジナルから再度 WriteableBitmap を生成して上書き
            //_targetBitmap = new WriteableBitmap(_originalBitmap);

            //// UI側のソースも更新（念のため）
            //MainImage.Source = _targetBitmap;

            _currentAngle = 0;
            _currentBrightness = 0;
            RotationSlider.Value = 0;
            BrightnessSlider.Value = 0;
            ApplyTransformations();
        }

        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // 画像が読み込まれていない場合は何もしない
            if (_originalBitmap == null || _targetBitmap == null) return;

            _currentBrightness = (int)e.NewValue;
            ApplyTransformations();
        }

        private void RotateButton_Click(object sender, RoutedEventArgs e)
        {
            _currentAngle = RotationSlider.Value;
            ApplyTransformations();
        }

        private void RotateFreeButton_Click(object sender, RoutedEventArgs e)
        {
            double angle = RotationSlider.Value;
            ApplyFreeRotation(angle);
        }
    }
}