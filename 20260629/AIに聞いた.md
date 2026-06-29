# WPFで時間がかかる処理の最中にそれを使用者に示すにはどんな方法がある？
<br><br><br>



WPFアプリケーションで時間がかかる処理（重い処理）を実行する際、ユーザーに「フリーズしていないこと」を伝え、適切に待機してもらうためのUIパターンはいくつかあります。

重要なのは、「画面が固まらないように非同期処理（`async/await`）にする」**ことと、**「処理中であることを視覚的に伝える」ことです。主な方法を4つご紹介します。

---

## 1. ProgressBar（プログレスバー）を表示する

最も王道な方法です。進捗が数値化できる場合と、できない場合で設定を切り替えます。

* **進捗が分からない場合（不確定：Indeterminate）**
「処理中であること」だけを伝えるために、バーがループアニメーションする状態にします。
```xml
<ProgressBar IsIndeterminate="True" Height="20" Width="200" />

```


* **進捗が分かる場合（確定）**
全体の何％まで進んだかを正確に表示します。
```xml
<ProgressBar Minimum="0" Maximum="100" Value="{Binding ProgressValue}" Height="20" Width="200" />

```



---

## 2. マウスポインタを「砂時計（ぐるぐる）」にする

一番手軽に実装できる方法です。アプリ全体のカーソルを一時的に待機状態（Wait）に変更します。

* **実装例（C#）**
```csharp
try
{
    Mouse.OverrideCursor = Cursors.Wait; // 砂時計にする
    await Task.Run(() => DoHeavyWork()); // 重い処理
}
finally
{
    Mouse.OverrideCursor = null; // 元に戻す
}

```



> **注意点:** カーソルが変わるだけなので、ユーザーがボタンを連打できてしまう可能性があります。後述の「コントロールの無効化」と組み合わせるのが安全です。

---

## 3. 画面全体を半透明のレイヤーで覆う（ローディングマスク）

処理中にユーザーが他のボタンをクリックできないように（誤操作防止）、画面全体、または特定のエリアを半透明の黒や白のパネルで覆い、その中央に「Loading...」などの文字やスピナー（ぐるぐる回るアイコン）を表示する方法です。

* **XAMLの構造イメージ**
`Grid` を使って、メインコンテンツの上に重ねるように配置し、処理中だけ `Visibility` を `Visible` に切り替えます。
```xml
<Grid>
    <StackPanel> ... </StackPanel>

    <Grid Background="#88000000" Visibility="{Binding IsLoading, Converter={StaticResource BooleanToVisibilityConverter}}">
        <StackPanel HorizontalAlignment="Center" VerticalAlignment="Center">
            <ProgressBar IsIndeterminate="True" Width="100" Margin="0,0,0,10"/>
            <TextBlock Text="処理中、しばらくお待ちください..." Foreground="White"/>
        </StackPanel>
    </Grid>
</Grid>

```



---

## 4. 処理中のパーツ（コントロール）を無効化する

ボタンなどの特定のコントロール、あるいは画面全体の `IsEnabled` プロパティを `False` にして、ユーザーが入力を受け付けない状態にします。

* **実装例**
```xml
<Window IsEnabled="{Binding IsNotLoading}">
    </Window>

```


これに加えて、ステータスバーなどに「データを読み込み中...」とテキストを表示しておくと、親切なUIになります。

---

## 💡 最も重要な技術的注意点：UIスレッドを止めないこと

どんなに綺麗なプログレスバーを作っても、重い処理をUIスレッド（メインスレッド）のまま実行してしまうと、**画面自体がフリーズしてアニメーションも動きません。**

必ず `async / await` や `Task.Run` を使って、バックグラウンドスレッドで処理を実行してください。

```csharp
// ✕ ダメな例（画面が固まる）
private void Button_Click(object sender, RoutedEventArgs e) {
    Thread.Sleep(5000); // 5秒間フリーズ
}

// ◯ 良い例（画面が固まらない）
private async void Button_Click(object sender, RoutedEventArgs e) {
    IsLoading = true; // プログレスバーなどを表示
    
    await Task.Run(() => {
        // ここに重い処理を書く（裏で実行される）
        DoHeavyWork();
    });
    
    IsLoading = false; // プログレスバーを非表示
}

```

今回のアプリでは、どのパターン（進捗率がわかるもの、ただ待たせるだけのものなど）が一番イメージに近そうでしょうか？

<br><br><br><br><br><br>




# 時間のかかる処理はクリップボードへのコピーと、ファイルに保存。その対象は10000x10000ピクセル以上の大きなBitmapSource。なので進捗率はわからない。ステータスバーにプログレスバーとテキスト表示、操作の無効化するのがよさそう


<br><br><br>


方向性がカチッと固まりましたね！
「10,000 × 10,000ピクセル以上のBitmapSource」の処理（保存・クリップボードコピー）となると、完全にI/Oやメモリのシリアライズ処理がボトルネックになるため、進捗率（％）を細かく計算して出すのは不可能です（やろうとすると余計に遅くなります）。

ですので、ご提案通り「ステータスバーでの不確定（Indeterminate）表示 ＋ 画面全体の無効化（IsEnabled = False）」が、最もシンプルかつ実装コストが低く、ユーザーにとっても分かりやすいベストなアプローチです。

この仕様を満たす、MVVMパターンを意識した具体的な実装コードの例を用意しました。

---

## 1. XAML（画面側）の実装

ステータスバーを画面下部に配置し、画面全体の `IsEnabled` とステータスバーの `Visibility` を連動させます。

```xml
<Window x:Class="WpfApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2000/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2000/xaml"
        Title="画像処理アプリ" Height="450" Width="800">
    
    <DockPanel IsEnabled="{Binding IsNotLoading}">
        
        <StatusBar DockPanel.Dock="Bottom" Height="30">
            <StatusBar.Style>
                <Style TargetType="StatusBar">
                    <Setter Property="Visibility" Value="Collapsed"/>
                    <Style.Triggers>
                        <DataTrigger Binding="{Binding IsLoading}" Value="True">
                            <Setter Property="Visibility" Value="Visible"/>
                        </DataTrigger>
                    </Style.Triggers>
                </Style>
            </StatusBar.Style>
            
            <StatusBarItem>
                <ProgressBar IsIndeterminate="True" Width="100" Height="15"/>
            </StatusBarItem>
            <StatusBarItem>
                <TextBlock Text="{Binding StatusText}" Margin="5,0,0,0"/>
            </StatusBarItem>
        </StatusBar>

        <Grid Background="White">
            <Button Content="巨大画像を保存＆コピー" 
                    HorizontalAlignment="Center" VerticalAlignment="Center" 
                    Width="200" Height="50"
                    Command="{Binding ExportImageCommand}"/>
        </Grid>
        
    </DockPanel>
</Window>

```

---

## 2. ViewModel（ロジック側）の実装

巨大画像（BitmapSource）の処理を `Task.Run` で完全に別スレッドに逃がします。

> ⚠️ **超重要注意点（WPFの仕様）**
> `BitmapSource` は通常、**作成されたスレッド（UIスレッド）でしか操作できません**。
> そのため、そのまま `Task.Run` の中で保存しようとすると「別スレッドからアクセスされました」というエラー（InvalidOperationException）を吐きます。
> これを防ぐために、バックグラウンドに渡す前に **`.Freeze()`** を呼んでオブジェクトを凍結（読み取り専用化）させる必要があります。

```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel; // ※MVVMライブラリ（例）
using CommunityToolkit.Mvvm.Input;

public partial class MainViewModel : ObservableObject
{
    // 状態管理用のプロパティ
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotLoading))]
    private bool _isLoading;

    // IsLoading の逆を返すプロパティ（画面のIsEnabled用）
    public bool IsNotLoading => !IsLoading;

    [ObservableProperty]
    private string _statusText = "";

    // 巨大な画像データ（どこかで生成済みとする）
    private BitmapSource _hugeBitmap;

    [RelayCommand]
    private async Task ExportImageAsync()
    {
        // 1. 状態を「処理中」にする
        IsLoading = true;
        StatusText = "画像を処理中（保存・コピー中）...";

        // 2. 【超重要】UIスレッドで作ったBitmapSourceを別スレッドに渡せるように凍結する
        if (_hugeBitmap.CanFreeze)
        {
            _hugeBitmap.Freeze();
        }

        try
        {
            // 3. 重い処理を別スレッドで実行（画面は固まらない）
            await Task.Run(() =>
            {
                // A. ファイルに保存処理（例：PNGとして保存）
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "huge_output.png");
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(_hugeBitmap));
                    encoder.Save(fileStream);
                }

                // B. クリップボードへのコピー
                // ※クリップボード操作はSTAスレッド（UIスレッド）で行う必要があるため、Application.Current.Dispatcher を使う
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Clipboard.SetImage は中で色々処理が走るため、10,000px超えだとここでも少し引っかかり感が出る可能性はあります
                    Clipboard.SetImage(_hugeBitmap);
                });
            });

            StatusText = "完了しました！";
            MessageBox.Show("保存とコピーが完了しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"エラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            // 4. 状態を元に戻す
            IsLoading = false;
            StatusText = "";
        }
    }
}

```

---

## この実装のメリット

* **誤操作防止:** `DockPanel` 全体の `IsEnabled` を `False` にしているため、ユーザーは処理中にボタンを連打したり、他のタブに切り替えたりできなくなります（アプリがサスペンド状態のようになる）。
* **フリーズ感の解消:** ステータスバーのプログレスバーが「右から左へサラサラとアニメーション」し続けるため、Windowsが頑張って処理していることが一目で分かります。
* **メモリバグ防止:** `Freeze()` を行うことで、巨大な画像データを別スレッドで安全かつ高速にエンコードできます。10000x10000ピクセル（約4億バイト＝400MB以上の生データ）を扱う際は必須の処理です。



<br><br><br><br><br><br>





# mvvmを使わない場合も書いて
<br><br><br>


MVVMを使わない場合（コードビハインドに直接処理を書く場合）の実装例ですね！
ボタンのクリックイベント（Event Handler）の中で直接コントロール（コントロールの `Name`）を操作する形になります。

基本的なロジックの流れ（`async/await`の利用や、巨大画像に対する `.Freeze()` の重要性）はMVVM時と同じですが、コードはより直感的になります。

---

## 1. XAML（画面側）の実装

各コントロールに `Name` を割り当て、初期状態としてステータスバーは非表示（`Visibility="Collapsed"`）にしておきます。

```xml
<Window x:Class="WpfApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2000/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2000/xaml"
        Title="画像処理アプリ (コードビハインド版)" Height="450" Width="800">
    
    <DockPanel Name="MainContainer">
        
        <StatusBar Name="MyStatusBar" DockPanel.Dock="Bottom" Height="30" Visibility="Collapsed">
            <StatusBarItem>
                <ProgressBar IsIndeterminate="True" Width="100" Height="15"/>
            </StatusBarItem>
            <StatusBarItem>
                <TextBlock Name="StatusTextBlock" Text="" Margin="5,0,0,0"/>
            </StatusBarItem>
        </StatusBar>

        <Grid Background="White">
            <Button Content="巨大画像を保存＆コピー" 
                    HorizontalAlignment="Center" VerticalAlignment="Center" 
                    Width="200" Height="50"
                    Click="ExportButton_Click"/>
        </Grid>
        
    </DockPanel>
</Window>

```

---

## 2. コードビハインド（MainWindow.xaml.cs）の実装

イベントハンドラの頭に `async` を付け、処理の開始時と終了時にUIのパーツを直接コントロールします。

```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        // 巨大な画像データ（あらかじめどこかで生成されていると仮定）
        private BitmapSource _hugeBitmap;

        public MainWindow()
        {
            InitializeComponent();
            
            // テスト用：ダミーの10000x10000ピクセル画像を作成しておく
            // _hugeBitmap = BitmapSource.Create(10000, 10000, 96, 96, System.Windows.Media.PixelFormats.Pbgra32, null, new byte[10000 * 10000 * 4], 10000 * 4);
        }

        // async を付与して非同期イベントハンドラにする
        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. 処理開始：UIを「処理中状態」にする
            MainContainer.IsEnabled = false;            // 画面全体の操作を無効化
            MyStatusBar.Visibility = Visibility.Visible; // ステータスバーを表示
            StatusTextBlock.Text = "画像を処理中（保存・コピー中）...";

            // 2. 【超重要】UIスレッドで作ったBitmapSourceを別スレッドに渡せるように凍結
            if (_hugeBitmap != null && _hugeBitmap.CanFreeze)
            {
                _hugeBitmap.Freeze();
            }

            try
            {
                // 3. 重い処理を Task.Run で別スレッド（バックグラウンド）で実行
                await Task.Run(() =>
                {
                    if (_hugeBitmap == null) return;

                    // A. ファイルに保存処理
                    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "huge_output.png");
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(_hugeBitmap));
                        encoder.Save(fileStream);
                    }

                    // B. クリップボードへのコピー
                    // クリップボードはUIスレッド（STAスレッド）からしか触れないため Dispatcher を使う
                    Dispatcher.Invoke(() =>
                    {
                        Clipboard.SetImage(_hugeBitmap);
                    });
                });

                // 成功時のテキスト変更
                StatusTextBlock.Text = "完了しました！";
                MessageBox.Show("保存とコピーが完了しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"エラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // 4. 処理終了：UIを元の状態に戻す
                MainContainer.IsEnabled = true;             // 画面全体の操作を有効化
                MyStatusBar.Visibility = Visibility.Collapsed; // ステータスバーを非表示
                StatusTextBlock.Text = "";
            }
        }
    }
}

```

---

## コードビハインドで書く場合のポイント

* **`try-finally` の徹底:** 万が一ファイル保存などでエラーが起きても、必ず `finally` ブロックが通るようにします。そうしないと、**画面が一生無効化（フリーズ状態）されたまま**になってしまいます。
* **直感的な制御:** `MainContainer.IsEnabled = false;` のように、隠したいもの・止めたいものをコードから直接指定できるため、小規模なツールであればこちらの方が素早く実装できます。

<br><br><br><br><br><br>





# クリップボードへのコピー処理では、ステータスバーの表示が処理開始ではなく、終了時に表示されてしまう。開始時に表示するには？

<br><br><br>


`Dispatcher.Invoke` の使い方による罠にかかってしまっていますね！

結論から言うと、原因は **`Dispatcher.Invoke` が「UIスレッドが空くまで処理を止めて待つ（同期処理）」という動きをするため**です。

### なぜ遅れて表示されるのか？

1. ボタンを押した瞬間、UIスレッドで「ステータスバー表示」の命令が出ます。
2. しかし、WPFの画面描画（レンダリング）が実際に実行される前に、コードは `Task.Run` に進みます。
3. `Task.Run` の中で、すぐに `Dispatcher.Invoke` が呼ばれ、UIスレッドに「クリップボード処理を今すぐやって！」と割り込みをかけます。
4. UIスレッドは、**画面を書き換える（ステータスバーを表示する）前にクリップボードの重い処理を始めてしまう**ため、結果としてすべてが終わるまで画面の表示が更新されません。

---

## 解決策：`Dispatcher.InvokeAsync` を使う

解決するには、`Invoke`（同期）ではなく **`InvokeAsync`（非同期）** に変更します。
これにより、「画面の描画（ステータスバー表示）」が完了した後に、クリップボード処理が実行されるようになります。

コードビハインドの `Task.Run` の中身を以下のように書き換えてみてください。

```csharp
await Task.Run(async () => // ※中で await を使うので async を追加
{
    if (_hugeBitmap == null) return;

    // A. ファイルに保存処理（これはバックグラウンドで動く）
    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "huge_output.png");
    using (var fileStream = new FileStream(filePath, FileMode.Create))
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(_hugeBitmap));
        encoder.Save(fileStream);
    }

    // B. クリップボードへのコピー（InvokeAsync + await に変更）
    // これにより、UIスレッドの「画面描画」の順番を追い越さずに、綺麗に並んで実行されます
    await Dispatcher.InvokeAsync(() =>
    {
        Clipboard.SetImage(_hugeBitmap);
    });
});

```

### 💡 もうひとつのアプローチ（これでも直ります）

もし「ファイル保存」にもそこそこ時間がかかっているなら、**クリップボード処理を `Task.Run` の外に出す** のが一番シンプルです。

```csharp
// 1. 開始：ステータスバー表示
MainContainer.IsEnabled = false;
MyStatusBar.Visibility = Visibility.Visible;
StatusTextBlock.Text = "画像を処理中...";

// 2. 凍結
_hugeBitmap.Freeze();

// 3. ファイル保存だけを裏スレッドでやる
await Task.Run(() =>
{
    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "huge_output.png");
    using (var fileStream = new FileStream(filePath, FileMode.Create))
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(_hugeBitmap));
        encoder.Save(fileStream);
    }
});

// 4. ファイル保存が終わったら、UIスレッドに戻ってきてクリップボードに入れる
// (await の後ろなので、自動的にUIスレッドで実行されます)
StatusTextBlock.Text = "クリップボードにコピー中...";
Clipboard.SetImage(_hugeBitmap); 

```

この方法だと、ステータスバーのテキストを「ファイル保存中...」⇒「クリップボードにコピー中...」と途中で切り替えることもできるため、ユーザーにとってもより親切になります！

<br><br><br><br><br><br>





# ファイル保存は実行しないで、クリップボードへのコピーだけを実行すると、ステータスバーが非表示のまま実行されて処理終了時に表示されてしまう

<br><br><br>



ファイル保存を削って「クリップボードへのコピーだけ」にすると、`Task.Run` を使っていても、実質的に「重い処理のほぼすべてがUIスレッド（`Dispatcher`）の上で実行される」ことになります。

WPFの仕様上、UIスレッドに「ステータスバーを表示せよ！」と命令（`Visibility = Visible`）しても、**その直後に同じUIスレッドで重い処理（`Clipboard.SetImage`）が走ると、画面の描き替え（レンダリング）が後回しにされてしまいます。** その結果、すべての処理が終わった瞬間に、一瞬だけステータスバーが表示されて消える、という現象が起きます。

これを解決するには、「ステータスバーを表示した直後に、一瞬だけUIスレッドを解放して画面を描き替えさせる」必要があります。

一番スマートで確実な解決策は、`Task.Run` を使わず、**`Task.Yield()`** または **`Task.Delay()`** を使って描画の隙間を作ることです。

---

### 修正後のコード（クリップボードのみ版）

イベントハンドラの中身を以下のように修正してください。

```csharp
private async void ExportButton_Click(object sender, RoutedEventArgs e)
{
    if (_hugeBitmap == null) return;

    // 1. UIを「処理中状態」にする（この時点ではまだ画面に反映されない）
    MainContainer.IsEnabled = false;
    MyStatusBar.Visibility = Visibility.Visible;
    StatusTextBlock.Text = "クリップボードにコピー中...";

    // 2. 【ここが重要】UIスレッドの順番を一度システムに譲り、画面を描画させる
    // 1ミリ秒だけ待つことで、WPFが「ステータスバーを表示する」という描画処理を完了できます
    await Task.Delay(1);

    try
    {
        // 3. 画面が書き換わった後、クリップボードへのコピーを実行する
        // (10000px超えだと数秒かかりますが、すでにステータスバーは表示されています)
        Clipboard.SetImage(_hugeBitmap);

        StatusTextBlock.Text = "完了しました！";
        MessageBox.Show("クリップボードへのコピーが完了しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
    }
    catch (Exception ex)
    {
        MessageBox.Show($"エラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
    }
    finally
    {
        // 4. 元に戻す
        MainContainer.IsEnabled = true;
        MyStatusBar.Visibility = Visibility.Collapsed;
        StatusTextBlock.Text = "";
    }
}

```

### なぜこれで解決するのか？

`await Task.Delay(1);` を挟むことで、プログラムの処理が一時的にストップします。この一瞬の隙に、WPFは溜まっていた「ステータスバーを表示する」という画面描き替えタスクを消化します。

画面が綺麗に「処理中」に切り替わった後で、満を持して `Clipboard.SetImage` が実行されるため、ユーザーを不安にさせないUIになります。

<br><br><br><br><br><br>






#             await Task.Delay(1);では表示されなかったけど、            await Task.Delay(100);に変更したら表示された

<br><br><br>


なるほど、`1`ミリ秒ではWPFの描画レイヤー（レンダリングキュー）が追いつかず、`100`ミリ秒待つことでようやく実際の画面描き替えが間に合ったのですね。検証ありがとうございます！

実質的に `Clipboard.SetImage` がUIスレッドを完全に占有してしまうため、PCのスペックやその時のグラフィックの負荷状況によっては、`1`ミリ秒だと「描画の準備（レイアウト計算など）」だけで終わってしまい、実際の画面へのピクセル反映（レンダリング）まで辿り着けないことがあります。

もし「100ミリ秒の固定ディレイだと、環境によってはまだ足りないかも？」と不安な場合や、「少しでも無駄な待ち時間を減らしたい」という場合は、**WPFの描画優先度を明示的に指定して待つ**以下の方法（`DispatcherUtil` パターン）を使うと、より確実かつ最速で画面を更新できます。

### より確実なアプローチ：描画（Render）の完了を待つ

`Task.Delay` の代わりに、WPFの描画処理（`DispatcherPriority.Render`）が確実に終わるまで待機するコードです。

```csharp
private async void ExportButton_Click(object sender, RoutedEventArgs e)
{
    if (_hugeBitmap == null) return;

    // 1. UIを処理中状態にする
    MainContainer.IsEnabled = false;
    MyStatusBar.Visibility = Visibility.Visible;
    StatusTextBlock.Text = "クリップボードにコピー中...";

    // 2. WPFの「画面描画」が完全に終わるまで、非同期で待機する
    await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.Render);

    try
    {
        // 3. 画面が確実に書き換わった後、クリップボード処理を実行
        Clipboard.SetImage(_hugeBitmap);
        
        // （以下略）

```

### なぜこれで解決するのか？

`Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render)` は、「**画面の描画処理（Render）と同じ優先順位で、空っぽの処理（`() => { }`）をキューに入拘する**」という意味になります。

これが `await` で実行されると、WPFは「溜まっている描画処理」をすべて終わらせてからこの空っぽの処理を実行するため、**「これが通過した＝画面の書き換えが100%完了した」** という保証になります。無駄なスリープ時間（100msなど）を挟む必要もなくなるため、WPFで描画を強制したいときの鉄板のテクニックです。

現状の `Task.Delay(100);` でも十分に実用的ですが、もし「環境によってたまに動かない」といった挙動が見られた場合は、この `DispatcherPriority` を指定する方法も試してみてください！

<br><br><br><br><br><br>





# DispatcherPriority.Renderでは描画されなかったけど、値6のLoadedでも描画されず、値5のInputでは描画された

<br><br><br>


非常に鋭い検証をありがとうございます！まさにWPFのディスパッチャー（スレッドの優先順位キュー）の核心に触れる挙動です。

`Render`（値7）や `Loaded`（値6）でダメで、`Input`（値5）に下げたら描画された理由は、**「ステータスバーを表示する（Visibilityを変更する）」という処理が、WPF内部でどのようなステップを踏んで画面に反映されるか**に関係しています。

---

### なぜ `Input（値5）` まで下げる必要があったのか？

WPFでコントロールの見た目（`Visibility` など）を変更すると、内部では以下の順序で処理が進みます。

1. **レイアウト計算（Measure / Arrange）：** コントロールのサイズや配置を再計算する。
2. **描画命令の生成（Render）：** 画面にどう描くかのデータを組み立てる。
3. **描画（Render優先度での処理）：** 実際にグラフィックカード等へデータを送る。

ここで盲点なのが、**1の「レイアウト計算」の要求自体が、WPFの内部で `Render（値7）` より少し低い優先度（あるいは同等のタイミング）で処理待ちキューに入る点**です。

* **`Render (7)` / `Loaded (6)` で待った場合：**
「レイアウトの再計算」がまだ完了していない（＝まだ画面に描くデータが用意できていない）状態のまま、`await` が通過してしまいます。その直後に `Clipboard.SetImage` がUIスレッドを100%占有するため、結局描画が間に合いません。
* **`Input (5)` で待った場合：**
`Input` はマウスやキーボードの入力を処理する優先度です。WPFは「画面のレイアウト再計算や描画（7〜6付近の処理）をすべて終わらせてから、次のユーザー入力を受け付ける」という設計になっているため、`Input` まで優先度を下げて待つことで、**レイアウト計算も実際の描画もすべて確実に完了した状態**を作ることができます。

---

### まとめとしてのベストプラクティス

今回検証していただいた通り、UIスレッドを完全に占有するような重い処理の直前に画面描画を「確実に」完了させたい場合は、優先度を `Input（値5）` またはそれ以下（`Background（値4）` など）に設定して待つのがWPFの仕組み上、最も確実です。

```csharp
// これがWPFで「直前のUI変更を100%画面に反映させてから次に進む」ための最善のコードになります
await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.Input);

```

100msの固定ディレイ（`Task.Delay(100)`）だと、PCのスペックや負荷によっては稀にすり抜ける可能性がありますが、この `DispatcherPriority.Input` を使った方法であれば、**「WPFの描画レイヤーが完全に仕事を終えた瞬間」をピンポイントで捉えて次へ進む**ため、最も堅牢で、かつ無駄な待ち時間がないコードになります。素晴らしい検証結果でした！


<br><br><br><br><br><br>





