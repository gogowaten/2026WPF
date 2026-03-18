using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace _20260318
{
    public partial class VM : ObservableObject
    {
        public ObservableCollection<Data> datas { get; set; } = [];

        // Current
        // 値変更時に有効無効を判定
        // NotifyCanExecuteChangedForで登録したCommandは
        // 値変更時にCommandの有効無効の判定処理が実行される
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RemoveCurrentDataCommand))]
        private Data? _currentData;

        public VM()
        {
            for (int i = 0; i < 10; i++)
            {
                datas.Add(new Data() { Id = i, Name = "Item" + i });
            }

            CurrentData = datas[0];
        }


        // Currentの削除処理
        // ボタンに割り当てたときに自動で有効無効を切り替える
        // Currentが無いときはボタンを無効にする
        // CanRemoveCurrentDataを参照して切り替える
        [RelayCommand(CanExecute = nameof(CanRemoveCurrentData))]
        private void RemoveCurrentData()
        {
            if (CurrentData is not null)
            {
                datas.Remove(CurrentData);
                CurrentData = null;
                //RemoveCurrentDataCommand.NotifyCanExecuteChanged(); // 任意の場所で判定処理することもできる
            }
        }

        // Currentが在ればtrueを返す
        private bool CanRemoveCurrentData()
        {
            return CurrentData is not null;
        }

    }

    public partial class Data : ObservableObject
    {
        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private int _id = 0;
    }


    internal class Class1
    {
    }
}
