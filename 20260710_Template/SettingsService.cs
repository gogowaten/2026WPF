using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace _20260710_Template;

public class SettingsService
{
    private static readonly string FolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    private static readonly string FilePath = Path.Combine(FolderPath, "settings20260710.json");
    public Data MyData { get; private set; } = new();

    public void Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                MyData = JsonSerializer.Deserialize<Data>(json) ?? new Data();
                return;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"jsonファイル読み込みエラー:{ex.Message}");
        }
        MyData = new Data();
    }

    public void Save()
    {
        try
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
            JsonSerializerOptions options = new() { WriteIndented = true };
            string json = JsonSerializer.Serialize(MyData, options);
            File.WriteAllText(FilePath, json);

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"設定の保存に失敗:{ex.Message}");
            throw;
        }
    }

}
