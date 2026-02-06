using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Media;

namespace _20260206_fairu_ni_hozon
{

    public class TextBoxViewModel : INotifyPropertyChanged
    {
        #region プロパティ

        private string _text = "";
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged();
                    SaveSettings(); // 変更された瞬間に保存！
                }
            }
        }
        // 他のプロパティ（FontSizeなど）も同様に作成

        public string Foreground
        {
            get => foreground;
            set
            {
                if (foreground != value)
                {
                    foreground = value;
                    OnPropertyChanged();
                    SaveSettings();
                }
            }
        }
        private string foreground = Colors.Black.ToString();

        public string Background
        {
            get => background;
            set
            {
                if (background != value)
                {
                    background = value;
                    OnPropertyChanged();
                    SaveSettings();
                }
            }
        }

        private string background = Colors.Transparent.ToString();
        #endregion プロパティ

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        private void SaveSettings()
        {
            // 前述のJSON保存ロジックをここで呼び出す
            SettingsManager.Save(this);
            Console.WriteLine("データを保存しました。");
        }


        public TextBoxViewModel()
        {
            // 起動時にデータを読み込む
            var savedData = SettingsManager.Load();

            if (savedData != null)
            {
                // 初期値をセット（この時はまだSaveを走らせたくない場合は工夫が必要）
                // Textじゃなくて、そのバッキングフィールドの_textに直接代入する
                _text = savedData.Text;
                //_fontSize = savedData.FontSize;
                //_fontWeight = savedData.FontWeight;
                // ... 他の項目
                foreground = savedData.Foreground;
            }
        }

        //// --- 以下、既存のプロパティ群 ---
        //private string _text = "";
        //public string Text
        //{
        //    get => _text;
        //    set { /* 変更時に保存するロジック */ }
        //}
    }

    public class TextBoxSettings
    {
        public string Text { get; set; } = string.Empty;
        public double FontSize { get; set; } = Application.Current.MainWindow.FontSize;
        public string FontFamily { get; set; } = Application.Current.MainWindow.FontFamily.ToString();
        public string Foreground { get; set; } = Colors.Black.ToString(); // Color名やHexコードで保存
        public string Background { get; set; } = Colors.Transparent.ToString(); // Color名やHexコードで保存
        public double MarginLeft { get; set; }
        public double MarginTop { get; set; }
        public double MarginRight { get; set; }
        public double MarginBottom { get; set; }
        public string FontWeight { get; set; } = "Normal";

    }



    public class SettingsManager
    {
        private static string filePath = "textbox_settings.json";

        public static TextBoxSettings? Load()
        {
            //string filePath = "textbox_settings.json";
            if (!File.Exists(filePath)) return null;

            try
            {
                string jsonString = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<TextBoxSettings>(jsonString);
            }
            catch
            {
                // ファイルが壊れている場合などのエラーハンドリング
                return null;
            }
        }


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

        // 保存処理
        public static void Save(TextBoxViewModel textBox)
        {
            var settings = new TextBoxSettings
            {
                Text = textBox.Text,
                //FontSize = textBox.FontSize,
                //FontFamily = textBox.FontFamily.ToString(),
                Foreground = textBox.Foreground.ToString(),
                Background = textBox.Background.ToString(),
                //MarginLeft = textBox.Margin.Left,
                //MarginTop = textBox.Margin.Top,
                //MarginRight = textBox.Margin.Right,
                //MarginBottom = textBox.Margin.Bottom,
                //FontWeight = textBox.FontWeight.ToString(),
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
                textBox.Background = (Brush)new BrushConverter().ConvertFromString(settings.Background);
                textBox.Margin = new Thickness(settings.MarginLeft, settings.MarginTop, settings.MarginRight, settings.MarginBottom);
                // 行末の！はnull免除演算子で、nullじゃない確信があるときにだけ使う
                textBox.FontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(settings.FontWeight)!;
            }
        }

        // 読み込み処理
        public static void Load(TextBoxViewModel textBox)
        {
            if (!File.Exists(filePath)) return;

            string jsonString = File.ReadAllText(filePath);
            var settings = JsonSerializer.Deserialize<TextBoxSettings>(jsonString);

            if (settings != null)
            {
                textBox.Text = settings.Text;
                //textBox.FontSize = settings.FontSize;
                //textBox.FontFamily = new FontFamily(settings.FontFamily);
                //textBox.Foreground = (Brush)new BrushConverter().ConvertFromString(settings.Foreground);
                //textBox.Margin = new Thickness(settings.MarginLeft, settings.MarginTop, settings.MarginRight, settings.MarginBottom);
                //// 行末の！はnull免除演算子で、nullじゃない確信があるときにだけ使う
                //textBox.FontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(settings.FontWeight)!;
            }
        }





    }
}
