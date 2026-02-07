using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace _20260207_json_toolkitmvvm_command_matome
{
    //CommunityToolkit.Mvvm の [ObservableProperty] を使っている場合、この partial が絶対に必要です。

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
        // 変更時イベントのフック
        // CommunityToolkit.Mvvmだと
        // partial void OnTextChanged(string value) のように、特定の命名規則でメソッドを書くだけで、そのプロパティが変わった瞬間の処理（今回の場合は保存処理）を差し込めます。
        partial void OnTextChanged(string value) => Save();
        partial void OnFontSizeChanged(double value) => Save();
        partial void OnFontWeightChanged(string value) => Save();
    }

    /*        ってことは

partial void OnTextChanged(string value) => Save();

        このメソッド名も別の場所に自動生成されているの？



その通りです！まさにビンゴです。

partial void OnTextChanged は、**「ツールが裏側で用意した『予約席』に、あなたが中身を書き込んでいる」**という状態です。

仕組みの裏側
ライブラリが Text プロパティを自動生成する際、内部でこっそり以下のようなコードを書き出しています。

C#
// --- 自動生成ファイルの中身（イメージ） ---
public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    OnTextChanging(value); // 予約席1（変更前）
                    _text = value;
                    OnTextChanged(value);  // 予約席2（変更後） ← ここ！
                    OnPropertyChanged(nameof(Text));
                }
            }
        }

        // 宣言だけして、中身は空っぽの状態
        partial void OnTextChanged(string value); 
「宣言」と「実装」の分割
C#の partial method には面白いルールがあります。

宣言（自動生成側）: 「OnTextChanged というメソッドをここで呼び出すよ（でも中身は誰かが書くかもしれないし、書かないかもしれない）」と宣言します。

実装（あなた側）: partial void OnTextChanged...と書くことで、その中身を提供します。

もしあなたがメソッドを書かなかったら？
これが partial のすごいところで、もしあなたが OnTextChanged を書かなかった場合、コンパイラはその呼び出しコード自体を**「最初からなかったこと」**にして消し去ります。そのため、余計な処理が走ることもなく、パフォーマンスが一切低下しません。

まとめ
メソッド名の予約: 自動生成コード側ですでに「宣言」されている。

あなたの役割: その「予約された名前」を使って、実行したい処理（Save() など）を書き込む。

これで、なぜ「勝手に決まった名前（Onプロパティ名Changed）」を使わなければならないのか、その理由が繋がったのではないでしょうか。

次は、**「他にも自動生成されている便利なメソッド（変更前の値をチェックするメソッドなど）」**についても知りたいですか？
*/

}
