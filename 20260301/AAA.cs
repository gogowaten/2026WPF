using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace _20260301
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Data> _items = new();

        [RelayCommand]
        private void AddRectangle()
        {
            Items.Add(new RectangleData(50, 50, 100, 60) { Name = "新四角形" });
        }

        [RelayCommand]
        private void AddText()
        {
            Items.Add(new TextBlockData(150, 150, "Hello WPF!") { Name = "新テキスト" });
        }

        [RelayCommand]
        private void AddGroup()
        {
            // グループを作成
            var group = new GroupData { Name = "新しいグループ", X = 100, Y = 100 };

            // その中に子要素を追加
            group.Children.Add(new RectangleData(0, 0, 50, 50) { Name = "子:矩形" });
            group.Children.Add(new TextBlockData(10, 60, "子:テキスト") { Name = "子:文字" });

            Items.Add(group);
        }

        public MainViewModel()
        {
            // 初期データ
            Items.Add(new RectangleData(10, 10, 80, 40) { Name = "初期矩形" });
        }
    }



}
