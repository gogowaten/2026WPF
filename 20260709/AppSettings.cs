using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace _20260709
{
    public class AppSettings
    {
        // --- ウィンドウ位置・サイズ ---
        public double WindowLeft { get; set; } = 100;
        public double WindowTop { get; set; } = 100;
        public double WindowWidth { get; set; } = 800;
        public double WindowHeight { get; set; } = 450;
        public WindowState WindowState { get; set; } = WindowState.Normal;

        // --- その他のアプリ固有の設定 ---
        public string LastOpenedDirectory { get; set; } = string.Empty; // string型
        public bool IsDarkTheme { get; set; } = false;                  // bool型
        public int AutoSaveIntervalMinutes { get; set; } = 30;          // int型
    }

    public class SettingsService
    {
        // 保存先: C:\Users\<ユーザー名>\AppData\Local\YourApp\settings.json
        private static readonly string FolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "YourApp"
        );
        private static readonly string FilePath = Path.Combine(FolderPath, "settings.json");

        public AppSettings Current { get; private set; } = new();

        // 起動時に呼び出す
        public void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    // .NET 10の高速なSystem.Text.Jsonでデシリアライズ
                    Current = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                    return;
                }
            }
            catch
            {
                // ファイル破損などの場合はデフォルト値で上書き（ログ出力等を推奨）
            }
            Current = new AppSettings();
        }

        // 終了時に呼び出す
        public void Save()
        {
            try
            {
                if (!Directory.Exists(FolderPath))
                {
                    Directory.CreateDirectory(FolderPath);
                }

                var options = new JsonSerializerOptions { WriteIndented = true }; // 見やすく整形
                string json = JsonSerializer.Serialize(Current, options);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"設定の保存に失敗: {ex.Message}");
            }
        }
    }

}
