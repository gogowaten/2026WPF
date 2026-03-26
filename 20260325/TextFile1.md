# MainWindowのMyDataのPointsにPointを追加しても、MyThumbの中で表示しているGeoShapeが再描画されない原因は？
目的は、
DataThumbのMyDataがGeoShapeだったときに、見た目上のGeoShapeがピッタリ収まるサイズでDataThumbを表示したい、さらに、PointsのPointの増減やPointの値変更に合わせてGeoShapeの描画が更新されるようにしたい

```cs
namespace _20260325
{
    public partial class GeoShapeData : ShapeData
    {
        //[ObservableProperty] private ObservableCollection<Point> _points = [];
        [ObservableProperty] private PointCollection _points = [];
        [ObservableProperty] private PenLineCap _strokeEndLineCap = PenLineCap.Flat;
        [ObservableProperty] private PenLineCap _strokeStartLineCap = PenLineCap.Flat;
        [ObservableProperty] private double _strokeMiterLimit = 1.0;

    }
    public partial class EllipseData : ShapeData { }
    public abstract partial class ShapeData : Data
    {
        [ObservableProperty] private Brush? _fill;
        [ObservableProperty] private Brush? _stroke;
        [ObservableProperty] private double _strokeThickness = 1.0;
        [ObservableProperty] private PenLineJoin _strokeLineJoin = PenLineJoin.Miter;
    }
    public abstract partial class Data : ObservableObject
    {
        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private double _left;
        [ObservableProperty] private double _top;
        [ObservableProperty] private int _zIndex;
        [ObservableProperty] private double _width;
        [ObservableProperty] private double _height;
        [ObservableProperty] private Brush? _background = null;
    }
    
    
    public class ConvStrokePen : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var thick = (double)values[0];
            var miter = (double)values[1];
            var end = (PenLineCap)values[2];
            var start = (PenLineCap)values[3];
            var join = (PenLineJoin)values[4];
            Pen pen = new(Brushes.Transparent, thick)
            {
                EndLineCap = end,
                StartLineCap = start,
                LineJoin = join,
                MiterLimit = miter
            };
            return pen;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



    public class GeoShape : Shape
    {
        #region 依存関係プロパティ
        public Pen StrokePen
        {
            get { return (Pen)GetValue(StrokePenProperty); }
            set { SetValue(StrokePenProperty, value); }
        }
        public static readonly DependencyProperty StrokePenProperty =
            DependencyProperty.Register(nameof(StrokePen), typeof(Pen), typeof(GeoShape), new FrameworkPropertyMetadata(null, OnStrokePenChanged));

        private static void OnStrokePenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GeoShape GeoShape)
            {
                GeoShape.UpdateRenderBounds();
            }
        }

        public Rect RenderBounds
        {
            get { return (Rect)GetValue(RenderBoundsProperty); }
            set { SetValue(RenderBoundsProperty, value); }
        }
        public static readonly DependencyProperty RenderBoundsProperty =
            DependencyProperty.Register(nameof(RenderBounds), typeof(Rect), typeof(GeoShape), new FrameworkPropertyMetadata(new Rect(0, 0, 0, 0), FrameworkPropertyMetadataOptions.AffectsRender));

        //public ObservableCollection<Point> Points
        //{
        //    get { return (ObservableCollection<Point>)GetValue(PointsProperty); }
        //    set { SetValue(PointsProperty, value); }
        //}
        //public static readonly DependencyProperty PointsProperty =
        //    DependencyProperty.Register(nameof(Points), typeof(ObservableCollection<Point>), typeof(GeoShape), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnStrokePenChanged));

        public PointCollection Points
        {
            get { return (PointCollection)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register(nameof(Points), typeof(PointCollection), typeof(GeoShape), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnStrokePenChanged));

        #endregion 依存関係プロパティ


        public GeoShape()
        {
            MultiBinding mb = new() { Converter = new ConvStrokePen() };
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeThicknessProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeMiterLimitProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeEndLineCapProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeStartLineCapProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(StrokeLineJoinProperty) });
            SetBinding(StrokePenProperty, mb);

        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                if (Points is null || Points.Count == 0) { return Geometry.Empty; }

                StreamGeometry geo = new();
                using (var context = geo.Open())
                {
                    DrawBezier(context, Points[0], false, false, false);
                }

                geo.Freeze();
                return geo;
            }
        }

        private void DrawBezier(StreamGeometryContext context, Point begin, bool isFill, bool isClose, bool isSmoothJoin)
        {
            context.BeginFigure(begin, isFill, isClose);
            List<Point> bezier = Points.ToList();
            //var bezier = Points.Clone();
            bezier.RemoveAt(0);

            context.PolyBezierTo(bezier, true, isSmoothJoin);
        }

        public void UpdateRenderBounds()
        {
            if (Points is null || Points.Count == 0)
            {
                RenderBounds = new Rect();
                return;
            }

            // 見た目上のBoundsをpenを使って取得
            Rect bounds = DefiningGeometry.GetRenderBounds(StrokePen);
            //Width = bounds.Width;
            //Height = bounds.Height;
            RenderBounds = bounds;
            //InvalidateVisual();
        }

    }


    public class DataThumb : Thumb
    {

        public Data MyData
        {
            get { return (Data)GetValue(MyDataProperty); }
            set { SetValue(MyDataProperty, value); }
        }
        public static readonly DependencyProperty MyDataProperty =
            DependencyProperty.Register(nameof(MyData), typeof(Data), typeof(DataThumb), new PropertyMetadata(null));

        static DataThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DataThumb), new FrameworkPropertyMetadata(typeof(DataThumb)));
        }
        public DataThumb()
        {
            //MyData = new GeoShapeData() { Name = "ベジェ曲線", Stroke = Brushes.MediumAquamarine, StrokeThickness = 20.0, Points = [new Point(50, 70), new Point(250, 150), new Point(50, 250), new Point(50, 200), new Point(50, 150), new Point(150, 100), new Point(250, 250),], StrokeEndLineCap = PenLineCap.Round };

            //DataContext = MyData;

        }
    }

    public partial class MainWindow : Window
    {
        public GeoShapeData MyData { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            MyData = new GeoShapeData()
            {
                Name = "ベジェ曲線",
                Stroke = Brushes.MediumAquamarine,
                StrokeThickness = 20.0,
                Points =
                [
                    new Point(50, 70),
                    new Point(250, 150),
                    new Point(50, 250),
                    new Point(50, 200),
                    new Point(50, 150),
                    new Point(150, 100),
                    new Point(250, 250),
                ],
                StrokeEndLineCap = PenLineCap.Round,
                Background = Brushes.Gray
            };

            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dc = MyThumb.DataContext;
            var data = MyThumb.MyData;

            // 以下は描画更新される
            //MyData.Stroke = Brushes.Red;
            //MyData.StrokeThickness = 30;

            // Pointの追加では描画更新されない
            MyData.Points.Add(new Point(10, 10));
            MyData.Points.Add(new Point(10, 100));
            MyData.Points.Add(new Point(10, 200));

            var thumbdata = MyThumb.MyData;
        }
    }


}
```

```XAML
<Window x:Class="_20260325.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:_20260325" d:DataContext="{d:DesignInstance Type=local:MainWindow}"
        mc:Ignorable="d"
        Title="MainWindow" Height="367" Width="654">
  <Grid>
    <Grid.ColumnDefinitions>
      <ColumnDefinition/>
      <ColumnDefinition Width="200"/>
    </Grid.ColumnDefinitions>

    <Canvas x:Name="MyCanvas">
      <!--<local:GeoShape Points="50,70 250,150 50,250 50,200 50,150 150,100 250,250" Stroke="Red" StrokeThickness="10"/>-->
      <!--<local:DataThumb x:Name="MyThumb" DataContext="{Binding MyData}"/>-->
      <local:DataThumb x:Name="MyThumb" MyData="{Binding MyData}"/>
      <!--<local:DataThumb DataContext="{Binding }"/>-->
      <!--<local:DataThumb/>-->
    </Canvas>

    <StackPanel Grid.Column="1" DataContext="{Binding ElementName=MyThumb}">
      <Button Content="test" Click="Button_Click"/>
      <TextBlock Text="{Binding ActualWidth, StringFormat='ActualWidth = {0:0}'}"/>
      <TextBlock Text="{Binding ActualHeight, StringFormat='ActualHeight = {0:0}'}"/>
      <TextBlock Text="{Binding Height, StringFormat='Height = {0:0}'}"/>
      <TextBlock Text="{Binding Width, StringFormat='Width = {0:0}'}"/>
      <TextBlock Text="{Binding MyData.Width, StringFormat='MyData.Width = {0:0}'}"/>
      <TextBlock Text="{Binding MyData.Height, StringFormat='MyData.Height = {0:0}'}"/>
    </StackPanel>
  </Grid>
</Window>


<ResourceDictionary
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:local="clr-namespace:_20260325">

  <DataTemplate DataType="{x:Type local:EllipseData}">
    <Ellipse Width="{Binding Width}"
             Height="{Binding Height}"
             Fill="{Binding Fill}"
             StrokeThickness="{Binding StrokeThickness}"
             Stroke="{Binding Stroke}"/>
  </DataTemplate>
  <DataTemplate DataType="{x:Type local:GeoShapeData}">
    <local:GeoShape Points="{Binding Points}"
                    Stroke="{Binding Stroke}"
                    StrokeThickness="{Binding StrokeThickness}"
                    
                    />
  </DataTemplate>

  <!--<local:DataThumb x:Name="MyThumb" MyData="{Binding MyData}"/>-->
  <!--MainWindowで上の時は下--><!--
  <Style TargetType="{x:Type local:DataThumb}">
    <Setter Property="Template">
      <Setter.Value>
        <ControlTemplate TargetType="{x:Type local:DataThumb}">
          <Grid Background="{Binding MyData.Background}">
            <ContentPresenter Content="{Binding MyData}"/>
          </Grid>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>-->

  <!--<local:DataThumb x:Name="MyThumb" MyData="{Binding MyData}"/>-->
  <!--MainWindowで上の時は下-->
  <Style TargetType="{x:Type local:DataThumb}">
    <Setter Property="Template">
      <Setter.Value>
        <ControlTemplate TargetType="{x:Type local:DataThumb}">
          <Grid Background="{Binding MyData.Background}">
            <ContentPresenter Content="{Binding MyData}"/>
          </Grid>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>

  <!--<local:DataThumb x:Name="MyThumb" DataContext="{Binding MyData}"/>-->
  <!--MainWindowで上の時は下-->
  <!--<Style TargetType="{x:Type local:DataThumb}">
    <Setter Property="Template">
      <Setter.Value>
        <ControlTemplate TargetType="{x:Type local:DataThumb}">
          <Grid Background="{Binding Background}">
            <ContentPresenter Content="{Binding }"/>
          </Grid>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>-->


  <Style TargetType="{x:Type local:CustomThumb}">
    <Setter Property="Template">
      <Setter.Value>
        <ControlTemplate TargetType="{x:Type local:CustomThumb}">
          <ContentPresenter Content="{TemplateBinding MyContent}"/>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>



</ResourceDictionary>

```


---


ご提示いただいたコードにおいて、`Points.Add()` をしても再描画されない、およびサイズが自動で収まらない原因は主に3つあります。

### 1. なぜ `Points.Add` で再描画されないのか
WPFの `PointCollection` は、それ自体が変更通知（INotifyCollectionChanged）を持っていますが、**`Shape.Points` プロパティ（依存関係プロパティ）そのものが置き換わらない限り、WPFのレンダリングシステムは「プロパティが変化した」とみなしません。**

`MyData.Stroke = Brushes.Red;` が反映されるのは、プロパティのインスタンスそのものが入れ替わり、`AffectsRender` フラグが反応するためです。一方、コレクションの中身が増えるだけでは `PointsProperty` の変更コールバックは走りません。

### 2. サイズを「ピッタリ収まる」ようにするには
現在の `GeoShape` は `DefiningGeometry` を返していますが、`Shape` クラスは通常、自身の `Width/Height` が指定されていない場合、Stretch設定に従います。
「描画内容に合わせる」には、`DefiningGeometry` の境界線を計算し、それを `DataThumb` や `Data` の `Width/Height` にフィードバックする必要があります。

---

### 修正案：動的な更新と自動サイズ調整の実装

以下の手順で修正することをお勧めします。

#### 手順 A: PointCollection の変更を監視する
`GeoShape` 内で、`Points` プロパティが設定されたときに、そのコレクション自体の変更イベント（`Changed`）を購読します。

```cs
// GeoShape.cs 内の PointsProperty 登録部分
public static readonly DependencyProperty PointsProperty =
    DependencyProperty.Register(nameof(Points), typeof(PointCollection), typeof(GeoShape), 
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnPointsChanged));

private static void OnPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
{
    var shape = (GeoShape)d;
    
    // 古いコレクションのイベントを解除
    if (e.OldValue is PointCollection oldPC && !oldPC.IsFrozen)
    {
        oldPC.Changed -= shape.OnPointsCollectionChanged;
    }
    // 新しいコレクションのイベントを購読
    if (e.NewValue is PointCollection newPC && !newPC.IsFrozen)
    {
        newPC.Changed += shape.OnPointsCollectionChanged;
    }
    
    shape.UpdateRenderBounds();
}

private void OnPointsCollectionChanged(object? sender, EventArgs e)
{
    // コレクションの中身が変わったときに再描画とサイズ更新を強制
    InvalidateVisual();
    UpdateRenderBounds();
}
```

#### 手順 B: RenderBounds を DataModel へ反映させる
「見た目上のサイズに合わせたい」という目的を達成するため、`GeoShape` が計算した `RenderBounds` を、バインドされている `GeoShapeData` (MyData) の `Width/Height` に書き戻す仕組みを作ります。

```cs
public void UpdateRenderBounds()
{
    if (Points is null || Points.Count == 0) return;

    // 定義済みのジオメトリから描画上の境界を取得
    Rect bounds = DefiningGeometry.GetRenderBounds(StrokePen);
    RenderBounds = bounds;

    // 【重要】DataContext(GeoShapeData) のサイズを更新する
    // これにより DataThumb のサイズが連動して変わるようになります
    if (DataContext is GeoShapeData data)
    {
        // 線の太さ（StrokeThickness）を含んだサイズをセット
        data.Width = bounds.Width;
        data.Height = bounds.Height;
        
        // 必要に応じて Top / Left をオフセットさせる処理もここに書くと
        // 座標(10, 10)に描画したものがThumbの左上にピッタリ来ます
    }
}
```

#### 手順 C: XAML でのサイズ同期
`DataThumb` の `Style` 内で、`Grid` などのコンテナが `Data` の `Width/Height` を参照するようにします。

```xml
<Style TargetType="{x:Type local:DataThumb}">
  <Setter Property="Template">
    <Setter.Value>
      <ControlTemplate TargetType="{x:Type local:DataThumb}">
        <Grid Background="{Binding MyData.Background}" 
              Width="{Binding MyData.Width}" 
              Height="{Binding MyData.Height}">
          <ContentPresenter Content="{Binding MyData}"/>
        </Grid>
      </ControlTemplate>
    </Setter.Value>
  </Setter>
</Style>
```

---

### アドバイス：座標のオフセットについて
`Points` に `(100, 100)` のような値をいれると、`DataThumb` の内部（Canvas座標の 0,0 ではなく）で 100px ズレた位置に描画されます。
もし「常にデータの最小座標を (0,0) として扱い、Thumb 自体を Canvas 上で移動させたい」場合は、`UpdateRenderBounds` 内で `data.Left += bounds.X;` のように座標を調整し、`DefiningGeometry` 側で `TranslateTransform` をかけて座標をリセットする処理が必要になります。

次は、**「点が増えたときに自動的に Thumb の Canvas.Left/Top も動かして、図形がはみ出さないように調整する処理」**を実装しましょうか？


<br><br><br>



---


# Point追加などすると期待通りの動作になったけど、起動直後だけなにも描画されない。ActualHeightが0になっている


---



起動直後に描画されない原因は、**`GeoShape` がロードされる（Visual Tree に追加される）前に `DefiningGeometry` や `UpdateRenderBounds` が呼ばれ、その時点ではまだバインディングが解決しきっていない、あるいは `StrokePen` が初期値（null）であるため**と考えられます。

特に `DefiningGeometry.GetRenderBounds(StrokePen)` は、`StrokePen` が null だと正しい矩形を返せません。

### 解決策：Loaded イベントで強制更新する

`GeoShape` のコンストラクタで `Loaded` イベントを購読し、コントロールが画面に配置された直後に計算を走らせるのが最も確実です。

#### 1. GeoShape.cs の修正

```cs
public GeoShape()
{
    // ... 既存の MultiBinding 処理 ...

    // ロード完了時に一度計算を走らせる
    this.Loaded += (s, e) => {
        UpdateRenderBounds();
        InvalidateVisual(); // 再描画を強制
    };
}

public void UpdateRenderBounds()
{
    // 1. Points が空、または Pen がまだ生成されていない場合はスキップ
    if (Points == null || Points.Count == 0 || StrokePen == null)
    {
        return;
    }

    // 2. ジオメトリの計算
    var geometry = DefiningGeometry;
    if (geometry == Geometry.Empty) return;

    // 3. 描画範囲の取得
    Rect bounds = geometry.GetRenderBounds(StrokePen);
    
    // なにも描画するものがない（サイズが0）場合は更新しない
    if (bounds.Width == 0 || bounds.Height == 0) return;

    RenderBounds = bounds;

    // 4. DataContext (GeoShapeData) への反映
    if (DataContext is GeoShapeData data)
    {
        // 起動直後の ActualHeight=0 を防ぐため、明示的に Width/Height を入れる
        data.Width = bounds.Width;
        data.Height = bounds.Height;
    }
}
```

#### 2. XAML の DataTemplate を確認
`DataTemplate` 内の `GeoShape` に、自身の `DataContext` が `GeoShapeData` であることを明示的に伝えると、より安定します。

```xml
<DataTemplate DataType="{x:Type local:GeoShapeData}">
    <local:GeoShape x:Name="Shape"
                    Points="{Binding Points}"
                    Stroke="{Binding Stroke}"
                    StrokeThickness="{Binding StrokeThickness}"
                    StrokeMiterLimit="{Binding StrokeMiterLimit}"
                    StrokeEndLineCap="{Binding StrokeEndLineCap}"
                    StrokeStartLineCap="{Binding StrokeStartLineCap}"
                    StrokeLineJoin="{Binding StrokeLineJoin}" />
</DataTemplate>
```

---

### なぜ ActualHeight が 0 になるのか？

WPF の `Thumb` や `ContentPresenter` は、中身（GeoShape）のサイズが決まらない限り、自身のサイズを 0 とみなします。

1.  起動時：`MyData` がセットされる。
2.  `DataThumb` が生成されるが、まだ `GeoShape` の `Points` や `StrokePen` のバインディングが完了していない。
3.  `UpdateRenderBounds` が呼ばれるが、`StrokePen` が null なので `Rect(0,0,0,0)` が返る。
4.  結果、`Data.Width / Height` に 0 が書き込まれ、見た目上消えてしまう。

**`Loaded` イベント** を使うことで、すべてのプロパティバインディングが一通り完了した状態で最初の計算が行われるため、この「初期値 0 問題」を回避できます。

もしこれでも表示されない場合は、`UpdateRenderBounds` の中で **`data.Left` と `data.Top` も `bounds.X` と `bounds.Y` で補正する処理** を追加してみましょうか？（図形がマイナス座標にある場合、はみ出して見えないことがあるためです）


---


<br><br><br>


# 表示されない。起動時に実行されるUpdateRenderBoundsでの処理の中では正しくBoundsが取得できているので、起動直後のMyDataのWidthやHeightは正しい値が入っているけど、MyThumbのActualHeightなどは0になっている


---





原因が見えてきました。`MyData`（ViewModel側）の `Width/Height` に正しい値が入っているのに `MyThumb`（View側）の `ActualHeight` が **0** のままなのは、**`DataThumb` の `Style`（ControlTemplate）内のバインディングが、初期描画のタイミングでうまく同期できていない**可能性が高いです。

特に、`ContentPresenter` は中身のサイズを自動で外側に伝えない性質があるため、外側の `Grid` や `Thumb` 自身が明示的に `Data` のサイズを参照する必要があります。

### 解決策：TemplateBinding と SizeToContent の意識

以下の 2 点を修正・確認してください。

#### 1. DataThumb の Style を修正する
`ControlTemplate` 内の `Grid` が、確実に `MyData` の `Width/Height` を反映するように `Width="{Binding MyData.Width}"` を記述していますが、これが `Binding` だと解決が一段階遅れることがあります。

また、`DataThumb` 自身も `Width/Height` を持つべきなので、以下のように **`TemplateBinding`** や **`Setter`** で同期させます。

```xml
<Style TargetType="{x:Type local:DataThumb}">
  <Setter Property="Width" Value="{Binding MyData.Width, Mode=TwoWay}"/>
  <Setter Property="Height" Value="{Binding MyData.Height, Mode=TwoWay}"/>
  <Setter Property="Canvas.Left" Value="{Binding MyData.Left, Mode=TwoWay}"/>
  <Setter Property="Canvas.Top" Value="{Binding MyData.Top, Mode=TwoWay}"/>
  
  <Setter Property="Template">
    <Setter.Value>
      <ControlTemplate TargetType="{x:Type local:DataThumb}">
        <Grid Background="{Binding MyData.Background}" 
              Width="{TemplateBinding Width}" 
              Height="{TemplateBinding Height}">
          <ContentPresenter Content="{Binding MyData}"/>
        </Grid>
      </ControlTemplate>
    </Setter.Value>
  </Setter>
</Style>
```

#### 2. GeoShapeData (MyData) に初期値を入れる
もし `Width/Height` が `0` の状態で `Binding` が開始されると、WPF のレイアウトシステムが「サイズ 0」として固定してしまうことがあります。
`GeoShapeData` のプロパティが変更されたときに、確実に `PropertyChanged` イベントが飛んでいるか（`ObservableProperty` が機能しているか）確認してください。

---

### まだ直らない場合に疑うべきポイント：Canvas の特性
`Canvas` の中に置かれた要素は、**自身の `Width/Height` が指定されていない限り、中身がどれだけ大きくても `ActualWidth/Height` は 0 になる** ことがあります。

もし `DataThumb` を `Canvas` に配置しているなら、以下の点を確認してください：
* `DataThumb` 自身の `Width` と `Height` がセットされているか？（上記 Style の Setter で解決します）
* `GeoShape` 内で `UpdateRenderBounds` を呼んだあと、`Dispatcher.Yield()` などで一呼吸置かずに値をセットしているか？（起動直後はレイアウトパスが複雑なため）

### 試してほしいデバッグコード
`GeoShape.cs` の `UpdateRenderBounds` の最後で、強制的にレイアウトを更新させてみてください。

```cs
public void UpdateRenderBounds()
{
    // ... 前半の計算処理 ...

    if (DataContext is GeoShapeData data)
    {
        data.Width = bounds.Width;
        data.Height = bounds.Height;

        // 強制的に親（Thumbなど）にサイズが変わったことを通知する
        this.InvalidateMeasure();
        if (Parent is FrameworkElement parent)
        {
            parent.InvalidateMeasure();
        }
    }
}
```

これで `MyThumb` の `ActualWidth` が `MyData.Width` と同じ値になるはずです。
次は、**「図形の位置（Left/Top）がズレる問題」**への対応が必要になりそうですか？（描画範囲が (50, 70) から始まっている場合、そのままでは Thumb の中に余白ができてしまいます）


---


<br><br><br>



# DataThumbとDataをバインドすることで起動直後でも表示されるようになった！
```XAML
  <Style TargetType="{x:Type local:DataThumb}">
    <Setter Property="Width" Value="{Binding MyData.Width, Mode=TwoWay}"/>
    <Setter Property="Height" Value="{Binding MyData.Height, Mode=TwoWay}"/>
```


疑問1. 試しにContentPresenterのWidthとHeightにバインドでも表示されたけど、DataThumbにバインドするのとでは、どんな違いがある？
            <ContentPresenter Width="{Binding MyData.Width}" Height="{Binding MyData.Height}" Content="{Binding MyData}"/>

疑問2. GridのWidthとHeightには以下のようにバインドしているのはなんのため？
                Width="{TemplateBinding Width}"
                Height="{TemplateBinding Height}"

疑問3. 解決したのでUpdateRenderBoundsメソッドの以下の部分はもう必要ない？
// 強制的に親（Thumbなど）にサイズが変わったことを通知する
        this.InvalidateMeasure();
        if (Parent is FrameworkElement parent)
        {
            parent.InvalidateMeasure();
        }



---



無事に表示されたようで良かったです！バインディングが正しく繋がると、WPFのレイアウトシステムが本来の動きをしてくれるようになりますね。

ご質問いただいた3つの疑問について、WPFの内部構造（コントロールテンプレートとレイアウトパス）の観点から解説します。

---

### 疑問1. DataThumb と ContentPresenter どちらにバインドすべき？

**結論：`DataThumb`（親）にバインドするのが正解です。**

* **DataThumbにバインドする場合:**
    `Thumb` 自体がサイズを持つことになります。これにより、`Canvas.Left` や `Canvas.Top`、あるいはマウスでのドラッグ判定（Hit Test）が、**「図形の見た目通りの大きさ」**に対して正しく行われます。
* **ContentPresenterだけにバインドする場合:**
    `Thumb` 自体のサイズは `0`（またはデフォルト）のまま、中身だけが突き抜けて表示されている状態になる可能性があります。この場合、図形の外側（だけど描画されている部分）をクリックしてもドラッグが反応しないといった、UI上の不整合が起きやすくなります。

### 疑問2. Grid に TemplateBinding している理由

**結論：親（DataThumb）が決めたサイズを、内部レイアウト（Grid）に「強制的に伝える」ためです。**

`ControlTemplate` を作成する際、以下の階層構造になります：
1.  **DataThumb (本体)**：`Width/Height` を持っている。
2.  **Grid (テンプレートのルート)**：これにサイズ指定がないと、中身に合わせて縮むか、親一杯に広がろうとします。
3.  **ContentPresenter**：中身（GeoShape）を表示する。

`Grid` に `TemplateBinding` を書くことで、**「DataThumbのサイズが変わったら、中のGridも一分のア狂いもなく同じサイズになってね」**という命令になります。これを書かないと、背景色（`Background`）を指定したときに、図形の周りの余白部分がクリックに反応しなくなるなどの問題が出ることがあります。

### 疑問3. InvalidateMeasure() はもう不要？

**結論：基本的には「不要」ですが、お守りとして残しても害はありません。**

WPFの依存関係プロパティ（特に `Width` や `Height`）に値をセットすると、内部で自動的に「レイアウトの再計算が必要（InvalidateMeasure相当）」というフラグが立ちます。

* **なぜ解決したか：** `MyData.Width` を `DataThumb.Width` にバインドしたことで、`data.Width = bounds.Width;` と代入した瞬間に `DataThumb` のプロパティが変わり、WPFが自動で再計算を始めたからです。
* **消してもいいタイミング：** 実行してみて、Pointを追加した瞬間に「パッ」とサイズが変わるなら、手動の `InvalidateMeasure` は削除してコードをスッキリさせて大丈夫です。

---

### 次のステップ：位置の自動調整（オフセット）

現在、`Points` に `(50, 70)` などの値を入れると、`DataThumb` の左上に **50px, 70px の余白**ができてしまいませんか？

もし「描画された図形の左上角」を `DataThumb` の `(0, 0)` にピッタリ合わせたい場合は、`GeoShape` の中で `DrawingContext.PushTransform` を使って、計算した `bounds.X/Y` 分だけ座標をマイナス方向にずらす処理が必要になります。

次は、**「図形を常に左上に詰め、その分 DataThumb 自体の座標（Left/Top）を移動させる」**という、より高度な配置ロジックに興味はありますか？（これを行うと、Illustratorなどのツールのような挙動になります！）



---

<br><br><br>


# (0,0)ぴったりに合わせる、位置の自動調整（オフセット）を解説して


---


図形を `(0, 0)` にピッタリ合わせる「オフセット調整」は、ベクター描画ソフトを作る上で非常に重要なステップです。

現在の状態では、`Points` に `(100, 100)` と指定すると、`DataThumb` の左上に **100px の空き地**ができてしまいます。これを「見た目上の左上」に詰め、その分 `DataThumb` 自体を `Canvas` 上で移動させるロジックを解説します。

---

### 1. オフセット調整の考え方

概念としては以下の2ステップになります。

1.  **データの補正（View側の描画）**: `GeoShape` の内部で、描画座標から `Bounds.X` と `Bounds.Y` を引き算して、常に `(0, 0)` から描画が始まるようにします。
2.  **位置の同期（Model側の更新）**: `DataThumb`（または `MyData`）の `Left/Top` プロパティに、本来の `Bounds.X/Y` を加算して、Canvas上での位置を正しく保ちます。



---

### 2. GeoShape.cs の修正（描画のずらし）

`DefiningGeometry` で返すジオメトリ全体に、マイナス方向の「平行移動（Transform）」をかけます。

```cs
protected override Geometry DefiningGeometry
{
    get
    {
        if (Points is null || Points.Count == 0) return Geometry.Empty;

        StreamGeometry geo = new();
        using (var context = geo.Open())
        {
            DrawBezier(context, Points[0], false, false, false);
        }

        // --- ここから追加 ---
        // 1. まず「生の」境界を取得
        Rect bounds = geo.GetRenderBounds(new Pen(Brushes.Black, StrokeThickness)); 
        
        // 2. 左上(X, Y)を 0 にするための変換を作成
        TranslateTransform transform = new(-bounds.X, -bounds.Y);
        
        // 3. ジオメトリ自体を変形（左上に詰める）
        Geometry transformedGeo = geo.GetFlattenedPathGeometry(); // または Clone
        transformedGeo.Transform = transform;
        
        transformedGeo.Freeze();
        return transformedGeo;
    }
}
```

---

### 3. UpdateRenderBounds の修正（Modelへの反映）

描画を `(0, 0)` に詰めたので、その「詰めた分（元々の X, Y）」を `MyData.Left / Top` に足し合わせます。

```cs
public void UpdateRenderBounds()
{
    if (Points is null || Points.Count == 0 || StrokePen == null) return;

    // ジオメトリを取得（この時点で internal には 0,0 基準になっている）
    var geometry = DefiningGeometry;
    Rect bounds = geometry.GetRenderBounds(StrokePen);

    // 元々の Points の中での「左上座標」を取得するために、
    // 変形前のジオメトリ（または計算用の一時的なもの）から X, Y を拾う必要があります。
    // ※ 簡略化のため、ここでは「現在の Points の最小 X, Y」を元にします。
    double minX = Points.Min(p => p.X) - (StrokeThickness / 2);
    double minY = Points.Min(p => p.Y) - (StrokeThickness / 2);

    if (DataContext is GeoShapeData data)
    {
        // サイズを更新
        data.Width = bounds.Width;
        data.Height = bounds.Height;

        // 【重要】位置を「図形の左上」に合わせて移動させる
        // 既存の Left/Top をベースに、図形が動いた分だけオフセットさせる
        data.Left = minX;
        data.Top = minY;
    }
}
```

---

### 4. XAML での位置バインド

`DataThumb` の `Style` に、`Left` と `Top` の同期を追加します。

```xml
<Style TargetType="{x:Type local:DataThumb}">
    <Setter Property="Canvas.Left" Value="{Binding MyData.Left, Mode=TwoWay}"/>
    <Setter Property="Canvas.Top" Value="{Binding MyData.Top, Mode=TwoWay}"/>
    
    <Setter Property="Width" Value="{Binding MyData.Width, Mode=TwoWay}"/>
    <Setter Property="Height" Value="{Binding MyData.Height, Mode=TwoWay}"/>
    </Style>
```

---

### これで何が起きるか？

1.  あなたが `Points.Add(new Point(10, 10))` とすると...
2.  `GeoShape` が「一番左にある点は 10px だな」と判断します。
3.  描画内容を `-10px` ズラして、`DataThumb` の **(0, 0)** に表示します。
4.  同時に `DataThumb` 本体の `Canvas.Left` を `+10px` 移動させます。

結果として、**「ユーザーからは図形が正しい位置にあるように見えるが、内部的には Thumb の枠内にピッタリ収まっている」**という状態が作れます。

次は、**「マウスで Points 内の各点をドラッグして変形させる機能」**についても解説が必要でしょうか？


---


<br><br><br>



# ”// 3. ジオメトリ自体を変形（左上に詰める）
Geometry transformedGeo = geo.GetFlattenedPathGeometry(); // または Clone
”
上記ではtransformedGeoは常に空になっていたので下記のようにCloneにしたら正しい値が取得できた
”Geometry transformedGeo = geo.Clone();”
GetFlattenedPathGeometryを解説して
あと、MyData.StrokeStartLineCap = PenLineCap.Square;のようにLineCapを変更したり、Pointを追加すると描画位置が微妙にずれるのを直すには？



---



`GetFlattenedPathGeometry` で空になってしまったのは、`StreamGeometry`（読み取り専用で高速なジオメトリ）をそのまま平坦化しようとした際のWPFの内部挙動によるものです。`Clone()` で正解です！

ご質問の2点について解説します。

---

### 1. GetFlattenedPathGeometry とは？

このメソッドは、**「曲線（ベジェ曲線や円弧）を、指定した許容誤差の範囲内で『短い直線の集合』に変換する」**機能です。

* **なぜ使うのか**: 複雑な曲線の「正確な長さ」を計算したり、図形同士の重なり判定を高速化したりするために、曲線をカクカクの多角形（Polyline）に近似します。
* **なぜ空になったか**: `StreamGeometry` は非常にメモリ効率が良い反面、特定の操作（Flattenなど）を行う際にコンテキストが正しく渡らない場合があります。また、`StreamGeometry` は一度 `Freeze` したり `Close` したりすると、内部構造へのアクセスが制限されることがあるため、`Clone()` して `PathGeometry` として扱うのが一般的です。

---

### 2. LineCap 変更や Point 追加で位置がズレる原因

位置が微妙にズレる最大の理由は、**「点の座標（最小値）」と「ペン先を含めた描画範囲（RenderBounds）」の不一致**です。

`PenLineCap.Square` や `Round` を使うと、ペン先が点の座標よりも**外側にはみ出します**。
現在のコードで `minX = Points.Min(...)` と計算していると、「点の位置」は固定ですが、「描画のハシッコ」は LineCap の形状によって変わるため、`DataThumb` の左上に隙間ができたり、逆にはみ出したりします。



#### 解決策：常に「描画の左上 (Bounds.X/Y)」を基準にする

「点の最小値」ではなく、`GetRenderBounds` で得られた **「インクが塗られている一番左上の座標」** をオフセットの基準に統一します。

#### GeoShape.cs の修正案

```cs
protected override Geometry DefiningGeometry
{
    get
    {
        if (Points is null || Points.Count == 0) return Geometry.Empty;

        StreamGeometry geo = new();
        using (var context = geo.Open())
        {
            DrawBezier(context, Points[0], false, false, false);
        }

        // 1. 【重要】まず「現在の設定（Pen）」での生の描画範囲を取得
        // StrokePen が更新されるたびにここを通るので、LineCapの変化も反映される
        Rect rawBounds = geo.GetRenderBounds(StrokePen); 
        
        // 2. 描画の「左上端（インクのハシ）」を 0,0 に持ってくる
        TranslateTransform transform = new(-rawBounds.X, -rawBounds.Y);
        
        Geometry transformedGeo = geo.Clone();
        transformedGeo.Transform = transform;
        
        // 3. 後続の UpdateRenderBounds で使うために、この X,Y オフセットを保持しておく
        _lastOffset = new Vector(rawBounds.X, rawBounds.Y);

        transformedGeo.Freeze();
        return transformedGeo;
    }
}
private Vector _lastOffset;

public void UpdateRenderBounds()
{
    if (Points is null || Points.Count == 0 || StrokePen == null) return;

    // 変形後のジオメトリのサイズを取得
    var geometry = DefiningGeometry; 
    Rect bounds = geometry.GetRenderBounds(StrokePen);

    if (DataContext is GeoShapeData data)
    {
        // 描画サイズをセット
        data.Width = bounds.Width;
        data.Height = bounds.Height;

        // 【重要】前回保存した「インクの左上端」の座標を Canvas 上の Left/Top に反映
        // これで LineCap が伸びても、その分 Thumb 自体が外側に移動するのでズレない
        data.Left = _lastOffset.X;
        data.Top = _lastOffset.Y;
    }
}
```

---

### まとめ：ズレをなくすポイント

1.  **計算の基準を一つにする**: `Points.Min`（点）を使うのをやめ、`GetRenderBounds.X/Y`（見た目）を唯一の正解にします。
2.  **StrokePen を最新にする**: `StrokeStartLineCap` などの変更が `StrokePen` に反映され、それが `DefiningGeometry` の再計算（`InvalidateVisual`）を走らせるようにします。

これで、ペン先を太くしても、角を丸くしても、常に図形が `DataThumb` の左上に吸い付いたまま、配置だけが正しく調整されるようになります。

次は、**「図形をドラッグしたときに、Points の中身の数値を書き換える逆方向の処理」**に挑戦してみますか？