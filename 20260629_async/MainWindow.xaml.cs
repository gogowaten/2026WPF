using System.IO;
using System.Reflection;
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

// クリップボードはUIスレッド（STAスレッド）からしか触れない

//DispatcherPriority 列挙型 (System.Windows.Threading) | Microsoft Learn
//https://learn.microsoft.com/ja-jp/dotnet/api/system.windows.threading.dispatcherpriority?view=windowsdesktop-10.0            
//Background  4 //列挙値は 4 です。 操作は、他のすべての非アイドル操作が完了した後に処理されます。
//Render  7     //列挙値は 7 です。 レンダリングと同じ優先順位で処理された操作。

// Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background) は、「Backgroundと同じ優先順位で、空っぽの処理（() => { }）をキューに入拘する」という意味になります。

// await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Invalid);// error
// await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Inactive);// ng, 終わってもそのままで、更新されない
// await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.SystemIdle);// ok
// await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);// ok
// await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle);// ok
// 優先度4のBackgroundまで非同期で待機、これならステータスバーの表示処理後に実行される
// await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);// ok

// await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Input);// ok
// await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Loaded);// ng
// 優先度7の描画更新が終わるまで非同期で待機、これだと素通りされる
// await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);// ng
// await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.DataBind);// ng
// await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Send);// ng

namespace _20260629_async
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
            //MyBitmapSource = MakeTestBitmap(10000);
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


        // ファイルに保存
        private async void SaveFileTest()
        {
            if (MyBitmapSource is null) { return; }

            // 1. UIを「処理中状態」にする
            MainContainer.IsEnabled = false;
            MyStatusBar.Visibility = Visibility.Visible;
            StatusTextBlock.Text = "クリップボードにコピー中...";
            Mouse.OverrideCursor = Cursors.Wait; // マウスカーソル変更

            // 重要：UIスレッドで作ったBitmapSourceを別スレッドに渡せるように凍結する
            if (MyBitmapSource.CanFreeze) { MyBitmapSource.Freeze(); }

            try
            {
                await Task.Run(() =>
                {
                    // マイドキュメントフォルダに保存
                    string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "huge_output.png");
                    using (FileStream stream = new(filePath, FileMode.Create))
                    {
                        PngBitmapEncoder encoder = new();
                        encoder.Frames.Add(BitmapFrame.Create(MyBitmapSource));
                        encoder.Save(stream);
                    }
                });

                StatusTextBlock.Text = "処理完了";
                _ = MessageBox.Show(this, "保存が完了しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"エラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);

            }
            finally
            {
                // 状態を元に戻す
                MainContainer.IsEnabled = true;
                MyStatusBar.Visibility = Visibility.Collapsed;
                StatusTextBlock.Text = "";
                Mouse.OverrideCursor = null; // マウスカーソルをも度に戻す
            }

        }

        // クリップボードへのコピー、実行直後にUIを更新        
        private async void CopyTest()
        {
            if (MyBitmapSource is null) { return; }

            // 1. UIを「処理中状態」にする（この時点ではまだ画面に反映されない）
            MainContainer.IsEnabled = false;
            MyStatusBar.Visibility = Visibility.Visible;
            StatusTextBlock.Text = "クリップボードにコピー中...";
            Mouse.OverrideCursor = Cursors.Wait; // マウスカーソル変更

            // 2. 【ここが重要】UIスレッドの順番を一度システムに譲り、画面を描画させる
            // 優先度4（低め）のBackgroundで非同期で待機、これならステータスバーの表示処理後に実行される
            // 「Backgroundと同じ優先順位で、空っぽの処理（() => { }）をキューに入拘する」という意味になります。
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);// ok

            try
            {
                // 3. 画面が書き換わった後、クリップボードへのコピーを実行する
                // (10000px超えだと数秒かかりますが、すでにステータスバーは表示されています)
                Clipboard.SetImage(MyBitmapSource);

                StatusTextBlock.Text = "完了しました！";
                _ = MessageBox.Show(this, "クリップボードへのコピーが完了しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"エラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // 4. 元に戻す
                MainContainer.IsEnabled = true;
                MyStatusBar.Visibility = Visibility.Collapsed;
                StatusTextBlock.Text = "";
                Mouse.OverrideCursor = null; // マウスカーソルをも度に戻す
            }
        }


        // 非同期処理を使わない通常のコピー処理、画面更新されない
        private void CopyTestNotAsync()
        {
            if (MyBitmapSource is null) { return; }

            MainContainer.IsEnabled = false;
            MyStatusBar.Visibility = Visibility.Visible;
            StatusTextBlock.Text = "クリップボードにコピー中...";
            Mouse.OverrideCursor = Cursors.Wait; // マウスカーソル変更

            try
            {
                Clipboard.SetImage(MyBitmapSource);

                StatusTextBlock.Text = "完了しました！";
                _ = MessageBox.Show("クリップボードへのコピーが完了しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                MainContainer.IsEnabled = true;
                MyStatusBar.Visibility = Visibility.Collapsed;
                StatusTextBlock.Text = "";
                Mouse.OverrideCursor = null; // マウスカーソルをも度に戻す
            }
        }

        private void MyExe_Click(object sender, RoutedEventArgs e)
        {
            // 非同期
            CopyTest();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // 同期
            CopyTestNotAsync();
        }

        private void Button_ClickSaveTest(object sender, RoutedEventArgs e)
        {
            SaveFileTest();
        }
    }
}