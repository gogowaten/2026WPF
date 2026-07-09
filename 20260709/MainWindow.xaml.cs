using System.ComponentModel;
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

namespace _20260709
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SettingsService _settingsService = new();

        public MainWindow()
        {
            InitializeComponent();

            // 設定のロード
            _settingsService.Load();
            var settings = _settingsService.Current;

            // 画面外にはみ出ていないかチェック（マルチディスプレイ切断対策）
            if (settings.WindowLeft >= SystemParameters.VirtualScreenLeft &&
                settings.WindowLeft + settings.WindowWidth <= SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth &&
                settings.WindowTop >= SystemParameters.VirtualScreenTop &&
                settings.WindowTop + settings.WindowHeight <= SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight)
            {
                // 画面内に収まるなら位置を復元
                this.Left = settings.WindowLeft;
                this.Top = settings.WindowTop;
                this.Width = settings.WindowWidth;
                this.Height = settings.WindowHeight;

                // 最小化で閉じられていた場合は通常サイズで開く
                this.WindowState = settings.WindowState == WindowState.Minimized
                    ? WindowState.Normal
                    : settings.WindowState;
            }
            else
            {
                // 画面外なら画面中央に配置
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            // --- ここで string や bool の設定をUI等に反映 ---
            // 例: ApplyTheme(settings.IsDarkTheme);
            // 例: txtPath.Text = settings.LastOpenedDirectory;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            var settings = _settingsService.Current;

            // 1. ウィンドウ位置・サイズの保存（最大化・最小化時はRestoreBoundsを使用）
            if (this.WindowState == WindowState.Normal)
            {
                settings.WindowLeft = this.Left;
                settings.WindowTop = this.Top;
                settings.WindowWidth = this.Width;
                settings.WindowHeight = this.Height;
            }
            else
            {
                settings.WindowLeft = this.RestoreBounds.Left;
                settings.WindowTop = this.RestoreBounds.Top;
                settings.WindowWidth = this.RestoreBounds.Width;
                settings.WindowHeight = this.RestoreBounds.Height;
            }
            settings.WindowState = this.WindowState;

            // 2. その他の設定値をUIや現在の状態から取得して保存
            // settings.IsDarkTheme = ...
            // settings.LastOpenedDirectory = ...

            // ファイルに書き込み
            _settingsService.Save();
        }
    }

}