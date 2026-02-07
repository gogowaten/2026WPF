using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace _20260207_json_toolkitmvvm_command_matome
{
    public static class SettingsManager
    {
        private static readonly string FilePath = "app_settings.json";

        public static void Save(object data)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }

        public static TextBoxSettings? Load()
        {
            if (!File.Exists(FilePath)) { return null; }
            return JsonSerializer.Deserialize<TextBoxSettings>(File.ReadAllText(FilePath));
        }
    }
}
