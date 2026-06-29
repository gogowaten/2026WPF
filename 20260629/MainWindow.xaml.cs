using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20260629
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CancellationTokenSource? cancellationTokenSource;
        BitmapSource? MyBitmapSource;
        public MainWindow()
        {
            InitializeComponent();
            MyBitmapSource = MakeBitmap();
            MyExe.Click += MyButtonキャンセル_Click;
        }

        private BitmapSource MakeBitmap()
        {
            int px = 10000;
            int py = 10000;
            int stride = px * 4;
            int length = py * stride;
            byte[] pixels = new byte[length];
            for (int i = 0; i < px * py; i++)
            {
                pixels[i] = 200;
            }
            return BitmapSource.Create(px, py, 96.0, 96.0, PixelFormats.Bgra32, null, pixels, stride);
        }
        // 表示更新用
        private void ShowProgress(int percent)
        {
            MyProgressBar.Value = percent;
            MyTextBlock.Text = $"{percent} ％完了";
        }

        private bool AAA(IProgress<int> p, CancellationToken token)
        {
            for (int i = 1; i <= 50; i++)
            {
                // キャンセル判定
                if (token.IsCancellationRequested) { return false; }

                Thread.Sleep(100);
                int percent = i * 100 / 50;
                p.Report(percent);
            }
            return true;
        }
        private async void MyButton実行_Click(object sender, RoutedEventArgs e)
        {
            MyButton実行.IsEnabled = false;
            MyStatasText.Text = "処理中";
            MyProgressBar.Value = 0;

            cancellationTokenSource = new();
            CancellationToken token = cancellationTokenSource.Token;

            Progress<int> prog = new(ShowProgress);

            bool result = await Task.Run(() => AAA(prog, token));

            if (result == false) { MyStatasText.Text = "キャンセルされた"; }
            else { MyStatasText.Text = "処理完了"; }

            MyButton実行.IsEnabled = true;
        }

        private void MyButtonキャンセル_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource?.Cancel();
        }


        #region 依存関係プロパティ


        //public bool MyIsNotLoading
        //{
        //    get { return (bool)GetValue(MyIsNotLoadingProperty); }
        //    set { SetValue(MyIsNotLoadingProperty, value); }
        //}
        //public static readonly DependencyProperty MyIsNotLoadingProperty =
        //    DependencyProperty.Register(nameof(MyIsNotLoading), typeof(bool), typeof(MainWindow), new PropertyMetadata(false));
        public bool MyIsNotLoadint => !MyIsLoading;

        public bool MyIsLoading
        {
            get { return (bool)GetValue(MyIsLoadingProperty); }
            set { SetValue(MyIsLoadingProperty, value); }
        }
        public static readonly DependencyProperty MyIsLoadingProperty =
            DependencyProperty.Register(nameof(MyIsLoading), typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public string MyStatusText
        {
            get { return (string)GetValue(MyStatusTextProperty); }
            set { SetValue(MyStatusTextProperty, value); }
        }
        public static readonly DependencyProperty MyStatusTextProperty =
            DependencyProperty.Register(nameof(MyStatusText), typeof(string), typeof(MainWindow), new PropertyMetadata(""));
        #endregion 依存関係プロパティ

        private async Task ExportImageAsync()
        {
            if (MyBitmapSource is null) { return; }
            // 状態を「処理中」にする
            MyIsLoading = true;
            MyStatusText = "処理中";

            // UIスレッドで作ったBitmapSourceを別スレッドに渡せるように凍結する
            if (MyBitmapSource.CanFreeze) { MyBitmapSource.Freeze(); }

            try
            {
                await Task.Run(() =>
                {
                    // ファイルに保存
                    string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "huge_output.png");
                    using (FileStream stream = new(filePath, FileMode.Create))
                    {
                        PngBitmapEncoder encoder = new();
                        encoder.Frames.Add(BitmapFrame.Create(MyBitmapSource));
                        encoder.Save(stream);
                    }

                    //// クリップボードへのコピー
                    //// ※クリップボード操作はSTAスレッド（UIスレッド）で行う必要があるため、Application.Current.Dispatcher を使う
                    //Application.Current.Dispatcher.Invoke(() =>
                    //{
                    //    // Clipboard.SetImage は中で色々処理が走るため、10,000px超えだとここでも少し引っかかり感が出る可能性はあります
                    //    Clipboard.SetImage(MyBitmapSource);
                    //});
                });

                MyStatusText = "処理完了";
                MessageBox.Show("保存とコピーが完了しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"エラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);

            }
            finally
            {
                // 状態を元に戻す
                MyIsLoading = false; MyStatusText = "";
            }
        }



        private async void MyExe_Click(object sender, RoutedEventArgs e)
        {
            UiUpdateAfterCopy();

            //// 1. 処理開始：UIを「処理中状態」にする
            //MainContainer.IsEnabled = false;            // 画面全体の操作を無効化
            //MyStatusBar.Visibility = Visibility.Visible; // ステータスバーを表示
            //StatusTextBlock.Text = "画像を処理中（保存・コピー中）...";
            //Mouse.OverrideCursor = Cursors.Wait;

            //// 2. 【超重要】UIスレッドで作ったBitmapSourceを別スレッドに渡せるように凍結
            //if (MyBitmapSource != null && MyBitmapSource.CanFreeze)
            //{
            //    MyBitmapSource.Freeze();
            //}

            //try
            //{
            //    // 3. 重い処理を Task.Run で別スレッド（バックグラウンド）で実行
            //    await Task.Run(async () =>
            //    {
            //        if (MyBitmapSource == null) return;

            //        //// A. ファイルに保存処理
            //        //string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "huge_output.png");
            //        //using (var fileStream = new FileStream(filePath, FileMode.Create))
            //        //{
            //        //    var encoder = new PngBitmapEncoder();
            //        //    encoder.Frames.Add(BitmapFrame.Create(MyBitmapSource));
            //        //    encoder.Save(fileStream);
            //        //}

            //        // B. クリップボードへのコピー
            //        // クリップボードはUIスレッド（STAスレッド）からしか触れないため Dispatcher を使う
            //       await Dispatcher.InvokeAsync(() =>
            //          {
            //              Clipboard.SetImage(MyBitmapSource);
            //          });
            //    });


            //    // 成功時のテキスト変更
            //    StatusTextBlock.Text = "完了しました！";
            //    MessageBox.Show("保存とコピーが完了しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"エラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
            //finally
            //{
            //    // 4. 処理終了：UIを元の状態に戻す
            //    MainContainer.IsEnabled = true;             // 画面全体の操作を有効化
            //    MyStatusBar.Visibility = Visibility.Collapsed; // ステータスバーを非表示
            //    StatusTextBlock.Text = "";
            //    Mouse.OverrideCursor = null;
            //}
        }

        // クリップボードへのコピー、実行直後にUIを更新
        private async void UiUpdateAfterCopy()
        {
            if (MyBitmapSource is null) { return; }

            // 1. UIを「処理中状態」にする（この時点ではまだ画面に反映されない）
            MainContainer.IsEnabled = false;
            MyStatusBar.Visibility = Visibility.Visible;
            StatusTextBlock.Text = "クリップボードにコピー中...";

            //// 2. 【ここが重要】UIスレッドの順番を一度システムに譲り、画面を描画させる
            //// 1ミリ秒だけ待つことで、WPFが「ステータスバーを表示する」という描画処理を完了できます
            //await Task.Delay(100);

            //DispatcherPriority 列挙型 (System.Windows.Threading) | Microsoft Learn
        //https://learn.microsoft.com/ja-jp/dotnet/api/system.windows.threading.dispatcherpriority?view=windowsdesktop-10.0

            //await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);// ok
            // 優先度4のBackgroundまで非同期で待機、これならステータスバーの表示処理後に実行される
            await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.Background);// ok
            //await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.ContextIdle);// ok
            //await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.DataBind);// ng
            //await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.Inactive);// ng, 終わってもそのままで、更新されない
            //await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.Input);// ok
            //await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.Invalid);// error
            //await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.Loaded);// ng
            // 優先度7の描画更新（ステータスバー更新）が終わるまで非同期で待機、これだと素通りされる
            //await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.Render);// ng
            //await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.Send);// ng
            //await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.SystemIdle);// ok

            try
            {
                // 3. 画面が書き換わった後、クリップボードへのコピーを実行する
                // (10000px超えだと数秒かかりますが、すでにステータスバーは表示されています)
                Clipboard.SetImage(MyBitmapSource);

                StatusTextBlock.Text = "完了しました！";
                MessageBox.Show("クリップボードへのコピーが完了しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
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
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MyIsLoading = true;
        }
    }
}