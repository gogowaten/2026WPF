using _20260709.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;



namespace _20260709.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;

    // --- UIと双方向バインディングするプロパティ ---
    // [ObservableProperty] により、背後で変更通知付きのプロパティが自動生成されます
    [ObservableProperty] private double _windowLeft;
    [ObservableProperty] private double _windowTop;
    [ObservableProperty] private double _windowWidth;
    [ObservableProperty] private double _windowHeight;
    [ObservableProperty] private WindowState _windowState;

    // string型やbool型の設定項目
    [ObservableProperty] private string _lastOpenedDirectory = string.Empty;
    [ObservableProperty] private bool _isDarkTheme;

    public MainViewModel()
    {
        // 本来はDI（依存注入）推奨ですが、例示のため簡易的にnewしています
        _settingsService = new SettingsService();
        _settingsService.Load();

        // ロードした値をViewModelのプロパティに同期
        var settings = _settingsService.Current;
        WindowLeft = settings.WindowLeft;
        WindowTop = settings.WindowTop;
        WindowWidth = settings.WindowWidth;
        WindowHeight = settings.WindowHeight;

        // 最小化状態で閉じていた場合は、通常サイズ(Normal)で起動させるケア
        WindowState = settings.WindowState == WindowState.Minimized
            ? WindowState.Normal
            : settings.WindowState;

        LastOpenedDirectory = settings.LastOpenedDirectory;
        IsDarkTheme = settings.IsDarkTheme;
    }

    /// <summary>
    /// 保存処理を実行するコマンド
    /// [RelayCommand] により、外部からは「SaveCommand」としてアクセス可能になります
    /// </summary>
    [RelayCommand]
    private void Save()
    {
        var settings = _settingsService.Current;

        // ViewModelの現在の状態をModelに集約
        settings.WindowLeft = WindowLeft;
        settings.WindowTop = WindowTop;
        settings.WindowWidth = WindowWidth;
        settings.WindowHeight = WindowHeight;
        settings.WindowState = WindowState;

        settings.LastOpenedDirectory = LastOpenedDirectory;
        settings.IsDarkTheme = IsDarkTheme;

        // ファイルへ保存
        _settingsService.Save();
    }
}
