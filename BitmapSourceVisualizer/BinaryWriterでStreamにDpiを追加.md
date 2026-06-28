# このBitmapSourceのStreamに任意の型の値も一緒に詰め込んで、受け取り側でそれぞれを受け取るには？例えばBitmapSourceと10.0（double型）

---

<br><br><br>



`BitmapSource` と `double` 型（10.0）のような複数の異なるデータを一つの `Stream` にまとめて詰め込み、受け取り側でそれぞれを正確に取り出すには、**「データサイズ（またはメタデータ）を先に出力してから、実際のデータを書き込む」** というルール（プロトコル）を作るのが確実です。

これを行うには、**`BinaryWriter`** と **`BinaryReader`** を使うのが最もシンプルで軽量です。

---

### 1. 送信側：Streamにまとめて詰め込む

画像データの「バイトサイズ」を先に出力し、その後に「画像データ本体」、「double型の値」の順番で書き込みます。

```csharp
using System.IO;
using System.Windows.Media.Imaging;

public MemoryStream PackData(BitmapSource bitmapSource, double number)
{
    var ms = new MemoryStream();
    
    // BinaryWriterを使ってストリームに書き込む
    using (var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
    {
        // 1. まずBitmapSourceを一度別のメモリにPNGとして保存し、サイズを確定させる
        byte[] imageBytes;
        using (var imageStream = new MemoryStream())
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(imageStream);
            imageBytes = imageStream.ToArray();
        }

        // 2. 画像のバイト数を書き込む（受け取り側が何バイト読めばいいか知るため）
        writer.Write(imageBytes.Length);

        // 3. 画像のデータ本体を書き込む
        writer.Write(imageBytes);

        // 4. 任意の型の値を書き込む (double型)
        writer.Write(number);
    }

    // ストリームの位置を先頭に戻して返す
    ms.Position = 0;
    return ms;
}

```

---

### 2. 受信側：Streamからそれぞれを取り出す

書き込んだ順番と全く同じ順番で `BinaryReader` を使って読み込みます。

```csharp
using System.IO;
using System.Windows.Media.Imaging;

public (BitmapSource bitmap, double number) UnpackData(Stream stream)
{
    BitmapSource bitmap = null;
    double number = 0;

    // BinaryReaderを使ってストリームから読み込む
    using (var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true))
    {
        // 1. 最初に画像のバイト数を読み込む
        int imageLength = reader.ReadInt32();

        // 2. そのバイト数分だけ画像データを読み込む
        byte[] imageBytes = reader.ReadBytes(imageLength);
        
        // バイト配列からBitmapSourceを復元
        using (var imageStream = new MemoryStream(imageBytes))
        {
            var decoder = BitmapDecoder.Create(imageStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            bitmap = decoder.Frames[0];
        }

        // 3. 次にあるdouble型の値を読み込む
        number = reader.ReadDouble();
    }

    return (bitmap, number);
}

```

---

### 💡 なぜこの方法が良いのか？

画像データは可変長（サイズが毎回違う）データです。そのため、ストリームに直接「画像 ＋ 数値」と連続して流してしまうと、受け取り側は「どこまでが画像で、どこからが数値（double）なのか」の境界線が分からなくなってしまいます。

* 最初に `imageBytes.Length`（4バイトの整数）を書き込んでおくことで、受け取り側は「まず4バイト読んで、そこに書いてある数（例: 54321バイト）の分だけ画像を読めばいいんだな」と正確に判断できるようになります。
* 画像の読み込みが終わった直後のストリーム位置に、ちょうど `double` 型のデータ（8バイト）が待っているため、綺麗に分割して取り出すことができます。

もし今後、さらに文字列（`string`）や別の数値（`int`）などを追加したくなった場合も、`writer.Write(追加のデータ)` / `reader.ReadString()` のように**同じ順番で末尾に追加していくだけ**で簡単に拡張可能です。



---


<br><br><br>


# Visual Studio用のBitmapSourceのVisualizerで、BitmapSourceのデータの受取でdpiとピクセルフォーマットも受け取るにはどんな方法がある？

---
<br><br><br>



Visual Studioのデバッガービジュアライザーで、画像データ（PNG等）と一緒にDPIやピクセルフォーマットなどの追加情報を送るには、**それらをひとまとめにしたシリアライズ可能なコンテナ（データ用クラス）を用意し、それをシリアライズして `outgoingData`（Stream）に書き込む**のが最もスマートで確実な方法です。

現在、`PngBitmapEncoder.Save(outgoingData)` でStreamに直接PNGのバイナリを書き込んでしまっているため、バイナリデータしか受け取れなくなっています。これを独自のデータ構造に拡張しましょう。

以下に、DPI情報（`double`）やピクセルフォーマット（`string`など）を一緒に転送する実装例を提案します。

---

## 解決策：コンテナクラスを使ったカスタムシリアライズ

WPFの `PixelFormat` はそのままシリアライズできない場合があるため、名前（文字列）として渡すのが安全です。

### 1. 共有するデータ構造の定義

デバッガー側（Visualizer）とデバッグ対象側の両方から参照できる、または双方に定義したデータ構造を用意します。

```cs
[Serializable]
public class BitmapVisualizerData
{
    public double DpiX { get; set; }
    public double DpiY { get; set; }
    public string PixelFormat { get; set; }
    public byte[] ImageData { get; set; } // PNGバイナリをここに格納
}

```

### 2. 送信側（BitmapSourceObjectSource）の修正

PNGエンコーダーの出力を一度メモリに溜め、DPIなどのメタデータと一緒に `BinaryFormatter` や `JsonSerializer` 等でStreamに書き込みます。
*(※近年の.NET環境のビジュアライザーであれば、標準の `System.Text.Json` や `BinaryFormatter`（警告が出る場合はJson推奨）が使えます。ここでは最も手軽な `System.Runtime.Serialization.Formatters.Binary` またはJsonを想定したコードにします。)*

```cs
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.DebuggerVisualizers;

public class BitmapSourceObjectSource : VisualizerObjectSource
{
    public override void GetData(object target, Stream outgoingData)
    {
        if (target is not BitmapSource bmp)
        {
            base.GetData(target, outgoingData);
            return;
        }

        // 1. 本来のBitmapSourceから情報を正確に取得
        double dpiX = bmp.DpiX;
        double dpiY = bmp.DpiY;
        string format = bmp.Format.ToString();

        // 2. 画像をPNGとしてバイト配列化
        byte[] imageBytes;
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bmp));
        using (var ms = new MemoryStream())
        {
            encoder.Save(ms);
            imageBytes = ms.ToArray();
        }

        // 3. ひとまとめにしたデータオブジェクトを作成
        var data = new BitmapVisualizerData
        {
            DpiX = dpiX,
            DpiY = dpiY,
            PixelFormat = format,
            ImageData = imageBytes
        };

        // 4. シリアライズしてStreamに書き込む (ここではJsonSerializerの例)
        // ※ .NET Framework環境の場合は BinaryFormatter の使用や、
        // 単純にBinaryWriterでプリミティブ値を順に書き込む方法でもOKです。
        using (var writer = new StreamWriter(outgoingData, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            var json = System.Text.Json.JsonSerializer.Serialize(data);
            writer.Write(json);
        }
    }
}

```

### 3. 受信側（BitmapSourceDebuggerVisualizer）の修正

Streamからデータをデシリアライズし、画像データとメタデータをそれぞれ取り出します。

```cs
using System;
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.DebuggerVisualizers;

namespace BitmapSourceVisualizer
{
    public class BitmapSourceDebuggerVisualizer : DialogDebuggerVisualizer
    {
        protected override void Show(IDialogVisualizerService windowService,
                                     IVisualizerObjectProvider objectProvider)
        {
            if (objectProvider == null)
                throw new ArgumentNullException(nameof(objectProvider));

            // 1. シリアライズされたデータを取得
            using Stream stream = objectProvider.GetData();
            using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
            string json = reader.ReadToEnd();
            
            var data = System.Text.Json.JsonSerializer.Deserialize<BitmapVisualizerData>(json);
            if (data == null || data.ImageData == null) return;

            // 2. バイト配列からBitmapImageを復元
            BitmapImage bmp = new();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.StreamSource = new MemoryStream(data.ImageData);
            bmp.EndInit();
            bmp.Freeze();

            // 3. ウィンドウに画像と元のメタデータを渡す
            BitmapSourceVisualizerWindow win = new();
            
            // 元のDPIやフォーマットを表示したい場合は、ウィンドウ側にプロパティを用意して渡す
            // win.OriginalDpiX = data.DpiX;
            // win.OriginalDpiY = data.DpiY;
            // win.OriginalFormat = data.PixelFormat;

            win.SetImage(bmp);
            win.ShowDialog();
        }
    }
}

```

---

### 💡 Tips: シリアライザーを使わず軽量に送りたい場合

Json等のライブラリ依存を減らしたい、または処理を高速化したい場合は、`BinaryWriter` / `BinaryReader` を使って**手動でストリームに順番に書き込む**方法も非常におすすめです。

* **送信側:**
```cs
using (var writer = new BinaryWriter(outgoingData, System.Text.Encoding.UTF8, leaveOpen: true))
{
    writer.Write(bmp.DpiX);
    writer.Write(bmp.DpiY);
    writer.Write(bmp.Format.ToString());
    writer.Write(imageBytes.Length); // バイト数も書いておく
    writer.Write(imageBytes);
}

```


* **受信側:**
```cs
using (var reader = new BinaryReader(stream, System.Text.Encoding.UTF8))
{
    double dpiX = reader.ReadDouble();
    double dpiY = reader.ReadDouble();
    string format = reader.ReadString();
    int imageLength = reader.ReadInt32();
    byte[] imageBytes = reader.ReadBytes(imageLength);
    // あとは復元処理...
}

```



この方法であれば外部ライブラリのシリアライズ制限に引っかかることもなく、元の正確な `DpiX` や `PixelFormat`（文字列）を安全にビジュアライザー側へ持ち込めます。用途に合わせて試してみてください！
