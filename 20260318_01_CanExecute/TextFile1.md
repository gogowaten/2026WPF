# CommunityToolkit.MvvmのCommandを解説して


---


.NET 開発（WPF, WinUI, MAUI など）で欠かせない **CommunityToolkit.Mvvm** における Command（コマンド）の実装は、従来の面倒なボイラープレートコードを劇的に削減してくれます。

基本的には、`RelayCommand` というクラスと、それを自動生成する `[RelayCommand]` 属性の使い分けが肝になります。

---

## 1. 従来の書き方 vs ツールキットの書き方

以前は `ICommand` を継承したクラスを自作したり、長いプロパティを書く必要がありました。ツールキットを使うと、メソッドに属性をつけるだけで済みます。

### 現代的な書き方（属性を使用）
`partial` クラスに対して `[RelayCommand]` を付与すると、裏側で `GreetCommand` という名前のコマンドプロパティが自動生成されます。

```csharp
public partial class MyViewModel : ObservableObject
{
    [RelayCommand]
    private void Greet()
    {
        // 実行したい処理
        Debug.WriteLine("Hello, World!");
    }
}
```

---

## 2. 非同期コマンド (AsyncRelayCommand)

ネットワーク通信やファイル操作など、UIを固めない処理には `Task` を返すメソッドを使います。これも属性をつけるだけで、ツールキットが自動的に `IAsyncRelayCommand` として生成してくれます。

```csharp
[RelayCommand]
private async Task SaveDataAsync()
{
    await Task.Delay(1000); // 重い処理のシミュレーション
    // 保存処理
}
```

* **メリット:** 実行中かどうかを判定する `IsRunning` プロパティなどが自動で提供されます。

---

## 3. 実行可能条件 (CanExecute)

「テキストボックスが空ならボタンを押せないようにする」といった制御も簡単です。

```csharp
[RelayCommand(CanExecute = nameof(CanSubmit))]
private void Submit() 
{
    // 送信処理
}

private bool CanSubmit() => !string.IsNullOrEmpty(UserName);

// UserNameが変更されたときにコマンドの状態を再評価させる
[ObservableProperty]
[NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
private string? userName;
```

---

## 4. コマンドの全体像とデータフロー

MVVMパターンにおいて、Commandは View（画面）と ViewModel（ロジック）を繋ぐ「導火線」のような役割を果たします。



* **View:** ボタンクリックなどのイベントを検知。
* **Command:** Viewからの通知を受け取り、ViewModel内のメソッドを実行。
* **ViewModel:** データを処理し、必要に応じて View に通知。

---

## 主な特徴まとめ

| 機能 | 内容 |
| :--- | :--- |
| **自動生成** | メソッド名 + `Command` という名前でプロパティが作られる。 |
| **引数対応** | `void Execute(string name)` のように引数を取ることも可能。 |
| **キャンセルのサポート** | `Task` 実行時に `CancellationToken` を受け取ることができる。 |
| **例外処理** | 非同期実行中のエラーを補足する仕組みがある。 |

---

**次の一歩として、実際の XAML でのバインディング方法や、引数（CommandParameter）の渡し方について詳しく解説しましょうか？**


---


# 実際の XAML でのバインディング方法や、引数（CommandParameter）の渡し方について詳しく解説して


---


承知いたしました！`[RelayCommand]` で生成されたコマンドを、実際に XAML（View）側でどのように呼び出し、データを渡すのかを深掘りしましょう。

---

## 1. 基本的なバインディング

属性によって生成されたコマンドは、元のメソッド名に **"Command"** という接尾辞がついた名前で公開されます。

* **ViewModel側:** `void Login()` メソッド 
* **生成されるプロパティ:** `LoginCommand`

### XAML での記述
```xml
<Button Content="ログイン" 
        Command="{Binding LoginCommand}" />
```

---

## 2. 引数 (CommandParameter) の渡し方

ボタンを押したときに特定のデータ（IDや入力文字列など）をコマンドへ送りたい場合は、`CommandParameter` を使用します。

### ViewModel 側の準備
メソッドに引数を定義するだけで、ツールキットが型を自動判別して処理してくれます。

```csharp
[RelayCommand]
private void DeleteItem(string itemId) // string型の引数を受け取る
{
    Debug.WriteLine($"アイテム {itemId} を削除します");
}
```

### XAML 側の記述
`CommandParameter` 属性を使って値を渡します。

```xml
<Button Content="削除"
        Command="{Binding DeleteItemCommand}"
        CommandParameter="A123" /> 
```
> [!TIP]
> `CommandParameter="{Binding SelectedUser.Id}"` のように、他のプロパティとバインディングさせて動的に値を渡すことも一般的です。

---

## 3. 非同期コマンドでのキャンセル操作

`[RelayCommand]` は、引数に `CancellationToken` を含めると、自動的に「キャンセル可能な非同期コマンド」として扱ってくれます。

### ViewModel 側
```csharp
[RelayCommand]
private async Task DownloadAsync(CancellationToken token)
{
    try
    {
        await Task.Delay(5000, token); // 5秒かかる通信のシミュレーション
        Debug.WriteLine("ダウンロード完了");
    }
    catch (OperationCanceledException)
    {
        Debug.WriteLine("キャンセルされました");
    }
}
```

### XAML 側
ツールキットが生成する `IAsyncRelayCommand` には `CancelCommand` というプロパティが含まれているため、別のボタンで簡単にキャンセル処理を実装できます。

```xml
<StackPanel>
    <Button Content="開始" Command="{Binding DownloadCommand}" />
    <Button Content="キャンセル" Command="{Binding DownloadCommand.CancelCommand}" />
</StackPanel>
```

---

## 4. イベントをコマンドに変換する (EventToCommand)

「ボタンのクリック」以外のイベント（例：SelectionChanged や Loaded）でコマンドを動かしたい場合は、`Microsoft.Xaml.Behaviors` などのライブラリを併用するのが標準的です。



```xml
<ListView ItemsSource="{Binding Users}">
    <i:Interaction.Triggers>
        <i:EventTrigger EventName="SelectionChanged">
            <i:InvokeCommandAction Command="{Binding SelectUserCommand}" 
                                   CommandParameter="{Binding SelectedItem, RelativeSource={RelativeSource Self}}" />
        </i:EventTrigger>
    </i:Interaction.Triggers>
</ListView>
```

---

## まとめ

1.  **命名規則:** メソッド名 + `Command` でバインドする。
2.  **引数:** `CommandParameter` を使い、ViewModel側はメソッドの引数で受け取る。
3.  **非同期:** `CancellationToken` を渡せば、`.CancelCommand` で簡単に中断処理が作れる。
4.  **イベント:** 標準の `Command` プロパティがないコントロールには、Behavior（ビヘイビア）を使う。

**次は、これらのコマンドを使った際の「エラーハンドリング（例外が起きた時にどうするか）」について解説しましょうか？**


---


# CanExecuteを解説して


---


`CanExecute` は、簡単に言うと**「そのボタン（コマンド）が今、実行可能な状態かどうか」**を制御する仕組みです。

これが正しく設定されていると、実行できない状況ではボタンが自動的にグレーアウト（無効化）され、ユーザーの誤操作を防ぐことができます。

---

## 1. 基本的な仕組み

`[RelayCommand]` 属性を使う場合、`CanExecute` プロパティに「判定用のメソッド名」を指定します。

```csharp
public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))] // 状態が変わったらコマンドに通知
    private string? _password;

    // 1. コマンド本体
    [RelayCommand(CanExecute = nameof(CanLogin))]
    private void Login()
    {
        // ログイン処理
    }

    // 2. 判定ロジック（boolを返す必要がある）
    private bool CanLogin()
    {
        // パスワードが4文字以上ならボタンを有効にする
        return !string.IsNullOrWhiteSpace(Password) && Password.Length >= 4;
    }
}
```

### ポイント：状態の変化を知らせる
判定ロジック（`CanLogin`）の結果が変わる可能性がある場合、コマンド側に「もう一度チェックして！」と教える必要があります。
* **`[NotifyCanExecuteChangedFor(nameof(xxxCommand))]`**: プロパティが変更された際、自動的に `CanExecute` を再評価させます。

---

## 2. 非同期コマンドでの自動制御

`AsyncRelayCommand`（`Task` を返すメソッド）の場合、ツールキットはさらに賢く動きます。

デフォルトで **「実行中は二度押しできない」** という制御が入ります。

```csharp
[RelayCommand]
private async Task DownloadAsync()
{
    // ここが実行されている間、バインドされたボタンは自動的に無効（Disabled）になる
    await Task.Delay(3000);
}
```

もし、実行中もボタンを押せるようにしたい（多重実行を許可したい）場合は、属性で `AllowConcurrentExecutions = true` を指定します。

---

## 3. 手動で通知を送る方法

複雑な条件で、プロパティの属性だけでは対応できない場合は、コードから直接「再評価」を命令できます。

```csharp
// 特定のタイミングでボタンの状態を更新したい場合
LoginCommand.NotifyCanExecuteChanged();
```

---

## 4. UIとの連動イメージ

`CanExecute` が `false` を返すと、View（XAML）側では以下のような変化が起きます。



1.  **Button.IsEnabled:** 自動的に `false` になります。
2.  **見た目:** 標準的なスタイルであれば、ボタンが半透明やグレーになります。
3.  **イベント:** クリックしても `Login()` メソッドは呼ばれません。

---

## まとめ

* **CanExecute:** コマンドが実行可能か決める `bool` メソッド。
* **[RelayCommand(CanExecute = ...)]:** 属性で判定メソッドを紐付ける。
* **再評価:** プロパティが変わった時に `[NotifyCanExecuteChangedFor]` を付けて、ボタンの状態をリフレッシュさせる。

**この `CanExecute` を使って、特定の入力フォーム（バリデーション）と連動させる具体的なコード例などを見てみますか？**