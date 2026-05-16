WPFの`BitmapSource`をデバッグ中にサクッと確認できるカスタムVisualizer（デバッガービジュアライザー）があると、画像処理やUI開発の効率が劇的に上がりますよね。

.NET Core / .NET 5+ 以降のVisualizer開発は、「デバッガー側（UIを表示する処理）」**と**「デバッグ対象側（データをシリアライズして送る処理）」を分離して作成する必要があります。

以下に、Visualizerを作成する手順と実装コードのベースをまとめました。

---

## 1. プロジェクトの作成

Visual Studioで新規プロジェクトを作成します。

* **プロジェクトの種類:** クラスライブラリ (`.NET Standard 2.0` または `.NET 8.0` など、ターゲット環境に合わせて選択)
* **必要なNuGetパッケージ:**
* `Microsoft.VisualStudio.DebuggerVisualizers` (使用しているVisual Studioのバージョンに合わせる)



---

## 2. ソースコードの実装

Visualizerは、データを送る「Source」と、それを受け取って表示する「Visualizer」の2つのクラスが必要です。

### ① デバッグ対象側 (VisualizerObjectSource)

`BitmapSource`はそのままではシリアライズできないため、`PngBitmapEncoder`等を使ってバイト配列（またはメモリストリーム）に変換して送る必要があります。

```csharp
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.DebuggerVisualizers;

namespace WpfBitmapVisualizer
{
    public class BitmapSourceVisualizerSource : VisualizerObjectSource
    {
        public override void GetData(object target, Stream outgoingData)
        {
            if (target is BitmapSource bitmapSource)
            {
                // BitmapSourceをPNG形式でシリアライズ
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                
                using (var ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    var bytes = ms.ToArray();
                    
                    // バイト配列をデバッガー側に送信
                    Serialize(outgoingData, bytes);
                }
            }
        }
    }
}

```

### ② デバッガー側 (DialogDebuggerVisualizer)

受け取ったバイト配列を再度`BitmapSource`に復元し、WPFのウィンドウ等で表示します。

```csharp
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.DebuggerVisualizers;
using WpfBitmapVisualizer;

// VisualizerをVisual Studioに登録する属性
[assembly: DebuggerVisualizer(
    typeof(BitmapSourceVisualizer),
    typeof(BitmapSourceVisualizerSource),
    Target = typeof(BitmapSource),
    Description = "WPF BitmapSource Visualizer")]

namespace WpfBitmapVisualizer
{
    public class BitmapSourceVisualizer : DialogDebuggerVisualizer
    {
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            // 送られてきたバイト配列を取得
            if (objectProvider.GetData() is MemoryStream ms)
            {
                byte[] bytes = (byte[])Unserialize(ms);
                
                // バイト配列からBitmapImageを復元
                var bitmap = new BitmapImage();
                using (var imageStream = new MemoryStream(bytes))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = imageStream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }

                // 表示用の簡易ウィンドウを作成
                var window = new Window
                {
                    Title = "BitmapSource Viewer",
                    Width = 600,
                    Height = 400,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                var imageControl = new Image
                {
                    Source = bitmap,
                    Stretch = System.Windows.Media.Stretch.Uniform
                };

                window.Content = imageControl;

                // ウィンドウを表示
                window.ShowDialog();
            }
        }
    }
}

```

---

## 3. 配置と使い方

ビルドしてできた `WpfBitmapVisualizer.dll` を、Visual Studioが認識できる特定のフォルダに配置します。

### 配置先フォルダ

使用しているVisual Studioのバージョンによって異なります。

* **VS 2022 の場合:**
`%USERPROFILE%\Documents\Visual Studio 2022\Visualizers`
（マイドキュメント内の `Visual Studio 2022\Visualizers` フォルダ。存在しない場合は作成してください）

### 使い方

1. 配置後、Visual Studioを再起動（またはデバッグを開始）します。
2. WPFアプリケーションのデバッグ中、`BitmapSource`型の変数にブレークポイントで止まります。
3. 変数にマウスカーソルを合わせ、データヒント（虫眼鏡アイコン）をクリックすると、作成したカスタムウィンドウが開き、画像が表示されます。

---

## 💡 より使いやすくするためのTips

* **スクロールとズーム:** 画像が大きい場合を想定して、`Image`コントロールを`ScrollViewer`で囲ったり、マウスホイールで拡大縮小（Zoom）できる機能を入れると実用性が跳ね上がります。
* **メタ情報の表示:** ウィンドウの下部に、画像の「幅、高さ、ピクセルフォーマット（PixelFormats）」などをテキストで表示しておくと、デバッグがさらに捗ります。