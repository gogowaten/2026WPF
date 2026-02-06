using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.IO;

namespace _20260206_fairu_ni_hozon
{
    public class TextBoxSettings
    {
        public string Text { get; set; }
        public double FontSize { get; set; }
        public string FontFamily { get; set; }
        public string Foreground { get; set; } // Color名やHexコードで保存
        public double MarginLeft { get; set; }
        public double MarginTop { get; set; }
        public double MarginRight { get; set; }
        public double MarginBottom { get; set; }
        public string FontWeight { get; set; } = "Normal";

    }



    public class SettingsManager
    {
        private static string filePath = "textbox_settings.json";

        // 保存処理
        public static void Save(System.Windows.Controls.TextBox textBox)
        {
            var settings = new TextBoxSettings
            {
                Text = textBox.Text,
                FontSize = textBox.FontSize,
                FontFamily = textBox.FontFamily.ToString(),
                Foreground = textBox.Foreground.ToString(),
                MarginLeft = textBox.Margin.Left,
                MarginTop = textBox.Margin.Top,
                MarginRight = textBox.Margin.Right,
                MarginBottom = textBox.Margin.Bottom,
                FontWeight = textBox.FontWeight.ToString(),
            };

            string jsonString = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, jsonString);
        }

        // 読み込み処理
        public static void Load(System.Windows.Controls.TextBox textBox)
        {
            if (!File.Exists(filePath)) return;

            string jsonString = File.ReadAllText(filePath);
            var settings = JsonSerializer.Deserialize<TextBoxSettings>(jsonString);

            if (settings != null)
            {
                textBox.Text = settings.Text;
                textBox.FontSize = settings.FontSize;
                textBox.FontFamily = new FontFamily(settings.FontFamily);
                textBox.Foreground = (Brush)new BrushConverter().ConvertFromString(settings.Foreground);
                textBox.Margin = new Thickness(settings.MarginLeft, settings.MarginTop, settings.MarginRight, settings.MarginBottom);
                // 行末の！はnull免除演算子で、nullじゃない確信があるときにだけ使う
                textBox.FontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(settings.FontWeight)!;
            }
        }
    }
}
