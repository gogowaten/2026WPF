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
using System.Windows.Threading;

namespace _20260630
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapSource? MyBitmapSource;

        public MainWindow()
        {
            InitializeComponent();
            MyBitmapSource = MakeTestBitmap(10000);
        }

        // 指定したsizeの半透明の灰色の画像を作成する
        // 実際にできあがるバイトサイズは（size * size * 4）バイト
        private static BitmapSource MakeTestBitmap(int size)
        {
            int stride = size * 4;
            byte[] pixels = new byte[size * stride];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = 200;
            }
            return BitmapSource.Create(size, size, 96.0, 96.0, PixelFormats.Bgra32, null, pixels, stride);
        }


        //// クリップボードへのコピー、実行直後にUIを更新        
        //private async void CopyTest()
        //{
        //    if (MyBitmapSource is null) { return; }

        //    // 1. UIを「処理中状態」にする（この時点ではまだ画面に反映されない）
        //    MainContainer.IsEnabled = false;
        //    MyProgressBar.IsIndeterminate = true;

        //    if (MyBitmapSource.IsFrozen) { MyBitmapSource.Freeze(); }

        //    await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);

        //    Application.Current.Dispatcher.Invoke(() =>
        //        {
        //            try
        //            {
        //                // 3. 画面が書き換わった後、クリップボードへのコピーを実行する
        //                Clipboard.SetImage(MyBitmapSource);
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show($"エラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        //            }
        //            finally
        //            {                        
        //                MainContainer.IsEnabled = true;
        //                MyProgressBar.IsIndeterminate = false;
        //            }
        //        });
        //}

        private async void CopyTest()
        {
            if (MyBitmapSource is null) { return; }

            // 1. UIを「処理中状態」にする（IsIndeterminateをTrueに）
            MainContainer.IsEnabled = false;
            MyProgressBar.IsIndeterminate = true;
            MyStatusTextBlock.Text = "コピー処理中";

            // 画像は別スレッドに渡す前に必ず Freeze する必要があります
            if (!MyBitmapSource.IsFrozen && MyBitmapSource.CanFreeze)
            {
                MyBitmapSource.Freeze();
            }

            // 2. 新しいSTAスレッドを作成して、バックグラウンドでクリップボード処理を実行
            bool isSuccess = false;
            Exception? exception = null;

            await Task.Run(() =>
            {
                // STAスレッドを立てるための設定
                Thread thread = new(() =>
                {
                    try
                    {
                        // バックグラウンドのSTAスレッドでコピーを実行
                        Clipboard.SetImage(MyBitmapSource);
                        isSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }
                });

                thread.SetApartmentState(ApartmentState.STA); // 必須設定
                thread.Start();
                thread.Join(); // スレッドの完了を待つ
            });

            // 3. UIスレッドに戻ってきたので、結果の表示とUIの復元を行う
            if (!isSuccess && exception != null)
            {
                MessageBox.Show($"エラーが発生しました: {exception.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                MyStatusTextBlock.Text = "コピー処理中にエラー発生";
            }
            else
            {
                MyStatusTextBlock.Text = "コピー処理完了";
            }

            MainContainer.IsEnabled = true;
            MyProgressBar.IsIndeterminate = false;
        }


        private void MyExe_Click(object sender, RoutedEventArgs e)
        {
            CopyTest();
        }
    }
}