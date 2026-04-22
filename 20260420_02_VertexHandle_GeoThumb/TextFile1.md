# PathGeometryの各頂点を編集するための、頂点移動用のハンドルをAdornerを使って表示するクラスを作って

この図形の頂点を移動編集したい。各頂点にドラッグ移動できるハンドルを表示したい。どのようなコードを書けば良い？<br>


---



ドラッグ可能な `PathGeometry` の頂点を編集するために、`Adorner` を使用して各頂点に `Thumb`（ハンドル）を表示・移動させる実装例を紹介します。

ポイントは、**Adorner 内で VisualCollection を管理し、各頂点のインデックスを保持した Thumb を配置する**ことです。

### 1. 頂点編集用 Adorner クラスの実装

この `VertexAdorner` は、ターゲットとなる `GeoLine` の `MyPoints` の数だけ `Thumb` を生成し、ドラッグに合わせて `PointCollection` の値を直接更新します。

```cs
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;

namespace _20260420
{
    public class VertexAdorner : Adorner
    {
        private readonly VisualCollection _visuals;
        private readonly GeoLine _adornedElement;

        public VertexAdorner(UIElement adornedElement) : base(adornedElement)
        {
            _adornedElement = (GeoLine)adornedElement;
            _visuals = new VisualCollection(this);

            // 頂点の数だけハンドルを作成
            UpdateHandles();
        }

        public void UpdateHandles()
        {
            _visuals.Clear();
            var points = _adornedElement.MyPoints;
            if (points == null) return;

            for (int i = 0; i < points.Count; i++)
            {
                var thumb = new Thumb
                {
                    Width = 10,
                    Height = 10,
                    Background = Brushes.Red,
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(1),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = i // インデックスを保持
                };

                thumb.DragDelta += Thumb_DragDelta;
                _visuals.Add(thumb);
            }
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is Thumb thumb && thumb.Tag is int index)
            {
                var points = _adornedElement.MyPoints;
                if (points != null && index < points.Count)
                {
                    Point p = points[index];
                    // 頂点座標を更新
                    points[index] = new Point(p.X + e.HorizontalChange, p.Y + e.VerticalChange);
                }
            }
        }

        // 配置の制御
        protected override Size ArrangeOverride(Size finalSize)
        {
            var points = _adornedElement.MyPoints;
            if (points == null) return finalSize;

            for (int i = 0; i < _visuals.Count; i++)
            {
                if (_visuals[i] is Thumb thumb && i < points.Count)
                {
                    // ハンドルの中心が頂点に来るように配置
                    double left = points[i].X - (thumb.Width / 2);
                    double top = points[i].Y - (thumb.Height / 2);
                    thumb.Arrange(new Rect(left, top, thumb.Width, thumb.Height));
                }
            }
            return finalSize;
        }

        protected override int VisualChildrenCount => _visuals.Count;
        protected override Visual GetVisualChild(int index) => _visuals[index];
    }
}
```

---

### 2. GeoLine 側での Adorner 表示制御

`GeoLine` (またはそれを保持する `GeoThumb`) がクリックされた際などに Adorner を表示するようにします。ここでは `GeoLine` のコードに Adorner を制御するロジックを追加する例です。

```cs
// GeoLine クラス内に追記
private VertexAdorner? _vertexAdorner;

// 例えば、マウス左ボタン押し下げでハンドルを表示する場合
protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
{
    base.OnMouseLeftButtonDown(e);
    ShowVertexAdorner();
}

public void ShowVertexAdorner()
{
    var layer = AdornerLayer.GetAdornerLayer(this);
    if (layer == null) return;

    if (_vertexAdorner == null)
    {
        _vertexAdorner = new VertexAdorner(this);
        layer.Add(_vertexAdorner);
    }
}

public void HideVertexAdorner()
{
    var layer = AdornerLayer.GetAdornerLayer(this);
    if (layer != null && _vertexAdorner != null)
    {
        layer.Remove(_vertexAdorner);
        _vertexAdorner = null;
    }
}

// ポイントが変更されたら Adorner の配置も更新する必要がある
// PointCollection_Changed 内で _vertexAdorner.InvalidateArrange() を呼ぶ
private void PointCollection_Changed(object? sender, EventArgs e)
{
    _cachedGeometry = null;
    InvalidateMeasure();
    _vertexAdorner?.InvalidateArrange(); // 追加
}
```

---

### 3. 実装のポイント解説

* **PointCollection の挙動**: 
    WPF の `PointCollection` は要素（Point）が書き換えられると `Changed` イベントを発火します。提示された `GeoLine` クラスでは既に `OnPointCollectionChanged` でイベントを購読しているため、`Adorner` 経由で値を書き換えるだけで自動的に図形が再描画されます。
* **AdornerLayer**: 
    Adorner は `AdornerLayer` 上に描画されます。通常、`Window` や `Canvas` の直下に自動的に生成されます。
* **座標系**: 
    `GeoLine` は `DefiningGeometry` をオーバーライドして中身を描画しているため、`VertexAdorner` 内で扱う `points[i]` の座標は `GeoLine` 自身のローカル座標系と一致します。そのため、`ArrangeOverride` でそのまま座標を使えます。

### アドバイス
もし頂点の数が増減（追加・削除）する可能性がある場合は、`PointCollection_Changed` イベントの中で `_vertexAdorner.UpdateHandles()` を呼び出して、ハンドルの生成をやり直す処理を追加してください。


<br><br><br><br><br><br>

---


# 提示されたVertexAdornerでは、ArrangeOverride内ですべてのハンドルに対してArrangeを実行しているのが気になったので、以下のVertexAdorner2を書いてみた。比較と評価をして

```cs
    public class VertexAdorner2 : Adorner
    {
        protected override int VisualChildrenCount => _visuals.Count;
        protected override Visual GetVisualChild(int index) => _visuals[index];

        private readonly VisualCollection _visuals;
        private readonly GeoLine _adornedElement;
        private readonly ObservableCollection<Thumb> MyThumbs;
        private readonly Canvas MyCanvas;

        public VertexAdorner2(UIElement adornedElement) : base(adornedElement)
        {
            _adornedElement = (GeoLine)adornedElement;
            _visuals = new(this);
            MyCanvas = new Canvas();
            MyThumbs = [];
            _visuals.Add(MyCanvas);

            // 頂点の数だけハンドルを作成
            UpdateHandles();
        }

        public void UpdateHandles()
        {
            MyCanvas.Children.Clear();

            var points = _adornedElement.MyPoints;
            if (points == null) { return; }

            for (int i = 0; i < points.Count; i++)
            {
                var thumb = new Thumb()
                {
                    Width = 10,
                    Height = 10,
                    Background = Brushes.Red,
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(1),
                    Cursor = Cursors.Hand,
                    Tag = i // インデックスを保持
                };

                Canvas.SetLeft(thumb, points[i].X - 5);
                Canvas.SetTop(thumb, points[i].Y - 5);

                thumb.DragDelta += Thumb_DragDelta;
                _ = MyCanvas.Children.Add(thumb);
                MyThumbs.Add(thumb);
            }
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is Thumb thumb && thumb.Tag is int index)
            {
                var points = _adornedElement.MyPoints;
                if (points != null && index < points.Count)
                {
                    Point p = points[index];
                    // 頂点座標を更新
                    points[index] = new Point(p.X + e.HorizontalChange, p.Y + e.VerticalChange);
                    Canvas.SetLeft(thumb, p.X - 5 + e.HorizontalChange);
                    Canvas.SetTop(thumb, p.Y - 5 + e.VerticalChange);
                }
                e.Handled = true;
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            MyCanvas.Arrange(new Rect(finalSize));
            return base.ArrangeOverride(finalSize);
        }

      
    }
```

---


ご提示いただいた `VertexAdorner2` は、WPFのレイアウトシステムの特性を活かした非常にスマートなアプローチです。

最初に提示したコード（以下 `V1`）と、ご自身で書かれた `V2` を比較・評価します。

---

### 比較と評価

| 比較項目 | `VertexAdorner` (V1) | `VertexAdorner2` (V2) | 評価 |
| :--- | :--- | :--- | :--- |
| **描画負荷** | `ArrangeOverride` で全ハンドルをループ処理。 | `Canvas` に丸投げ。移動したハンドルのみ `SetLeft/Top` で更新。 | **V2が優位** |
| **コードの簡潔さ** | Adorner直下に配置するため、標準的な実装に近い。 | 内部に `Canvas` を持つため、少し構造が複雑。 | **V1がシンプル** |
| **座標管理** | `PointCollection` の値を参照して配置を決定。 | `Canvas.Left/Top` 添付プロパティで状態を保持。 | **V2がWPFらしい** |
| **保守性** | 頂点が増減した時の再計算が直感的。 | 移動時に `Point` と `Canvas` の両方を更新する必要がある。 | **一長一短** |

---

### 評価のポイント

#### 1. パフォーマンスと責務の分離（V2の勝利）
`V1` のように `ArrangeOverride` で全要素を `Arrange` するのは、頂点数が少ない（数十個程度）なら無視できる負荷ですが、数百個になるとレイアウトパスの重さが目立ってきます。
`V2` は **「座標管理を Canvas に任せる」** という戦略をとっているため、ドラッグ中の更新が「動かしているハンドル1つだけ」で済みます。これは非常に効率的です。

#### 2. Canvas を噛ませるメリット
Adorner は本来「装飾」のためのレイヤーであり、複雑な子要素の配置ロジックを自前で書く必要がありますが、`V2` のように内部に `Canvas` を1枚敷くことで、**「通常の Canvas 上のコントロール操作」** として扱えるようになります。これにより、将来的に「ハンドルのデザインをテンプレート化したい」といった要望にも柔軟に応えられます。

---

### ブラッシュアップのためのアドバイス

`V2` は非常に良い実装ですが、以下の2点を修正・考慮するとさらに堅牢になります。

#### ① `PointCollection` 更新への反応
現在 `V2` では `Thumb_DragDelta` で `Canvas.SetLeft` していますが、**「ドラッグ以外（外部のコードなど）で `MyPoints` が書き換わった場合」** にハンドルの位置が追従しません。
`GeoLine` 側で `PointCollection_Changed` が起きた際に、`UpdateHandles()` を呼び出すか、既存の `Thumb` の `Canvas.Left/Top` を一斉更新する処理が必要です。

#### ② `MyThumbs` (ObservableCollection) の必要性
コード内に `MyThumbs` が定義されていますが、`MyCanvas.Children` があれば参照は足りるため、メモリ管理の観点からは削除しても問題ありません（重複管理による不整合を防げます）。

### 結論
**`VertexAdorner2` のほうが優れています。**

WPFの思想である「適材適所（配置はパネルに任せる）」に則っており、特にドラッグ中の応答性能において有利です。

もし、さらに進化させるなら、`PointCollection` を `Canvas` 内の `Thumb` 群と同期させるための「同期メソッド」を1つ用意し、それをドラッグ時とデータ更新時の両方から呼ぶようにすると、コードがよりスッキリするでしょう。

```cs
// こんなイメージの同期メソッドがあると管理が楽です
private void SyncThumbPosition(int index, Point p) {
    var thumb = MyCanvas.Children[index] as Thumb;
    Canvas.SetLeft(thumb, p.X - 5);
    Canvas.SetTop(thumb, p.Y - 5);
}
```

非常に筋の良い改善案だと思います！