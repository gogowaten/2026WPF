using _20260709.Models;
using _20260709.ViewModels;
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

namespace _20260709;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            // 【画面外にはみ出る問題の対策】
            // ViewModelからロードされた初期位置が、現在のデスクトップ領域内にあるか検証
            bool isInsideScreen =
                vm.WindowLeft >= SystemParameters.VirtualScreenLeft &&
                vm.WindowLeft + vm.WindowWidth <= SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth &&
                vm.WindowTop >= SystemParameters.VirtualScreenTop &&
                vm.WindowTop + vm.WindowHeight <= SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight;

            if (!isInsideScreen)
            {
                // 画面外ならOSに任せて中央に配置（バインディング経由でViewModel側も追従します）
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        if (DataContext is MainViewModel vm)
        {
            // 【最小化・最大化の罠対策】
            // 閉じる直前の状態をチェックし、通常サイズ(Normal)以外なら RestoreBounds の値をViewModelに強制同期
            if (this.WindowState == WindowState.Normal)
            {
                vm.WindowLeft = this.Left;
                vm.WindowTop = this.Top;
                vm.WindowWidth = this.Width;
                vm.WindowHeight = this.Height;
            }
            else
            {
                vm.WindowLeft = this.RestoreBounds.Left;
                vm.WindowTop = this.RestoreBounds.Top;
                vm.WindowWidth = this.RestoreBounds.Width;
                vm.WindowHeight = this.RestoreBounds.Height;
            }
            vm.WindowState = this.WindowState;

            // ViewModelのSaveCommand（[RelayCommand]によって自動生成されたもの）を実行
            if (vm.SaveCommand.CanExecute(null))
            {
                vm.SaveCommand.Execute(null);
            }
        }
    }
}