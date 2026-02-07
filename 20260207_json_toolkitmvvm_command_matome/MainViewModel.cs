using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace _20260207_json_toolkitmvvm_command_matome
{





    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
        private string _text = "";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
        private double _fontSize = 12;

        [ObservableProperty]
        private string _fontWeight = "Normal";

        private bool CanReset()
        {
            return !string.IsNullOrWhiteSpace(Text) && FontSize > 10;
        }

        // 条件付きコマンド：文字があり、かつサイズが10より大きい時だけ押せる
        [RelayCommand(CanExecute = nameof(CanReset))]
        private void Reset()
        {
            Text = "";
            FontSize = 12;
        }



        public MainViewModel()
        {
            // 起動時に自動読み込み
            TextBoxSettings? saved = SettingsManager.Load();
            if (saved != null)
            {
                _text = saved.Text;
                _fontSize = saved.FontSize;
                _fontWeight = saved.FontWeight;
            }
        }

        private void Save()
        {
            SettingsManager.Save(new TextBoxSettings
            {
                Text = Text,
                FontSize = FontSize,
                FontWeight = FontWeight,
            });
        }


        // プロパティ変更時に自動保存
        partial void OnTextChanged(string value) => Save();
        partial void OnFontSizeChanged(double value) => Save();
        partial void OnFontWeightChanged(string value) => Save();
    }


}
