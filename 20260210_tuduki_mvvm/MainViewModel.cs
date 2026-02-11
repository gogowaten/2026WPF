using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Windows.Input;


namespace _20260210_tuduki_mvvm
{

    // 図形のリスト管理、一括移動、保存・読み込みのロジックを集約します。
    public partial class MainViewModel
    {
        // 図形のリスト（追加・削除がViewに自動反映される）
        public ObservableCollection<RectModel> Rects { get; } = [];

        public MainViewModel()
        {
            // 初期データ（例）
            //Rects.Add(new RectModel { X = 50, Y = 50, Width = 100, Height = 100 });
        }

        [RelayCommand]
        private void AddRect()
        {
            var newRect = new RectModel { X = 100, Y = 100, Width = 100, Height = 100 };
            // 図形が追加されるときに「移動要求」への返答をセットする
            newRect.MoveRequested = (dx, dy) =>
            {
                // 自分が選択中なら、他の選択中の図形もすべて動かす
                if (newRect.IsSelected)
                {
                    foreach (var item in Rects.Where(x => x.IsSelected))
                    {
                        item.X += dx;
                        item.Y += dy;
                    }
                }
            };
            Rects.Add(newRect);
        }

        // 選択状態の変更
        [RelayCommand]
        public void SelectRect(RectModel target)
        {
            // Ctrlキーが押されているかどうかを判定（後述のコマンド引数で渡す）
            bool isCtrlPressed = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

            if(!isCtrlPressed && !target.IsSelected)
            {
                // Ctrlなし、かつ未選択のものをクリックした場合：他をすべて解除
                foreach (var r in Rects)
                {
                    r.IsSelected = false;
                }
            }

            // クリックしたものを選択状態にする
            target.IsSelected = true;
        }

        // 保存ロジック（JSON）
        [RelayCommand]
        public void Save()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "JSONファイル (*.json)|*.json";
            if (dialog.ShowDialog() == true)
            {

                var json = JsonSerializer.Serialize(Rects);
                File.WriteAllText(dialog.FileName, json);
            }
        }

        // 読み込みロジック
        [RelayCommand]
        public void Load()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "JSONファイル (*.json)|*.json";
            if (dialog.ShowDialog() == true)
            {
                List<RectModel>? data =
                    JsonSerializer.Deserialize<List<RectModel>>(File.ReadAllText(dialog.FileName));
                if (data != null)
                {
                    Rects.Clear();
                    foreach (var item in data)
                    {
                        // ロード時もイベントを再接続する
                        item.MoveRequested = (dx, dy) => { /* AddRectと同じロジック（共通化推奨） */ };
                        Rects.Add(item);
                    }
                }

            }

        }
    }
}
