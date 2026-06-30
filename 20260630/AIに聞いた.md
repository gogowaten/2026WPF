# WPFでクリップボードへのコピー処理中にプログレスバーを動かすには？
* 10000x10000ピクセルの画像をコピーしていて完了まで数秒かかる
* プログレスバーは進捗率ではなくて動いているのが分かれば良い「IsIndeterminate="True"」
* プログレスバーのIsIndeterminateは処理開始前は"false"、処理中は"true"

以下のコードではプログレスバーが停止したまま処理が終わってしまう

```cs
        private async void CopyTest()
        {
            if (MyBitmapSource is null) { return; }

            // 1. UIを「処理中状態」にする（この時点ではまだ画面に反映されない）
            MainContainer.IsEnabled = false;
            MyProgressBar.IsIndeterminate = true;
            
            if (MyBitmapSource.IsFrozen) { MyBitmapSource.Freeze(); }

            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);

            Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        // 3. 画面が書き換わった後、クリップボードへのコピーを実行する
                        Clipboard.SetImage(MyBitmapSource);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"エラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {                        
                        MainContainer.IsEnabled = true;
                        MyProgressBar.IsIndeterminate = false;
                    }
                });
        }
```

<br><br><br>


WPFでプログレスバー（`IsIndeterminate="True"`）が動かない原因は、**クリップボードへのコピー処理（`Clipboard.SetImage`）をUIスレッド（メインスレッド）で実行してしまっているから**です。

現在のコードでは、`Application.Current.Dispatcher.Invoke(...)` を使って、重い処理をわざわざUIスレッドに投げ直しています。そのため、コピー中の数秒間はUIスレッドが完全にロックされ、プログレスバーのアニメーションを描画する余裕がなくなっています。

これを解決するには、**`Clipboard.SetImage` をUIスレッドとは別のスレッド（バックグラウンドスレッド）で実行する**必要があります。

---

## 解決コード（STAスレッドでのバックグラウンド処理）

WPFのクリップボード（`Clipboard`）は、**STA（Single Threaded Apartment）スレッド**からしか呼び出せないという制約があります。そのため、単純に `Task.Run` を使うだけだとエラーになります。

新しくSTAスレッドを作って、そこでコピー処理を走らせるのが一番確実です。

```csharp
private async void CopyTest()
{
    if (MyBitmapSource is null) { return; }

    // 1. UIを「処理中状態」にする（IsIndeterminateをTrueに）
    MainContainer.IsEnabled = false;
    MyProgressBar.IsIndeterminate = true;
    
    // 画像は別スレッドに渡す前に必ず Freeze する必要があります
    if (!MyBitmapSource.IsFrozen && MyBitmapSource.CanFreeze)
    {
        MyBitmapSource.Freeze();
    }

    // 2. 新しいSTAスレッドを作成して、バックグラウンドでクリップボード処理を実行
    bool isSuccess = false;
    Exception exception = null;

    await Task.Run(() =>
    {
        // STAスレッドを立てるための設定
        var thread = new System.Threading.Thread(() =>
        {
            try
            {
                // バックグラウンドのSTAスレッドでコピーを実行
                Clipboard.SetImage(MyBitmapSource);
                isSuccess = true;
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });

        thread.SetApartmentState(System.Threading.ApartmentState.STA); // 必須設定
        thread.Start();
        thread.Join(); // スレッドの完了を待つ
    });

    // 3. UIスレッドに戻ってきたので、結果の表示とUIの復元を行う
    if (!isSuccess && exception != null)
    {
        MessageBox.Show($"エラーが発生しました: {exception.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    MainContainer.IsEnabled = true;
    MyProgressBar.IsIndeterminate = false;
}

```

---

## ポイントの解説

* **`MyBitmapSource.Freeze()` の重要性**
WPFのUI要素や画像（`BitmapSource`）は、通常それを作ったUIスレッドからしかアクセスできません。しかし、`Freeze()`（凍結）を行うことでオバジェクトが変更不可（読み取り専用）になり、**別のスレッドへ安全に渡せる**ようになります。
* **なぜ `Task.Run` の中で `Thread` を作っているのか？**
`Task.Run` が使用するスレッドプールは「MTAスレッド」です。WPFのクリップボードは「STAスレッド」を要求するため、タスクの中で明示的に `SetApartmentState(ApartmentState.STA)` を指定したスレッドを起動しています。
* **UIのフリーズが解ける理由**
重いコピー処理が別スレッド（バックグラウンド）に引っ越したため、UIスレッドが完全に暇になります。これにより、プログレスバーのアニメーション（描画処理）が毎フレーム滑らかに実行されるようになります。

<br><br><br><br><br><br>

