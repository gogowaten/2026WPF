# 入れ子構造のDataModelをItemsControlですべて表示するには？
DataModelのコード
```cs
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Automation.Peers;
using System.Windows.Media;

namespace _20260306
{
    public partial class GroupData : Data
    {
        [ObservableProperty] private ObservableCollection<Data> _dataList = [];

        public GroupData() { Name = "GroupData"; }
    }

    public partial class EllipseData : ShapeData { public EllipseData() { Width = 50; Height = 50; } }

    public partial class RectangleData : ShapeData
    {
        public RectangleData()
        {
            Width = 100; Height = 50;X = 200; Y = 0;
            Fill = new SolidColorBrush(Color.FromArgb(50, 0, 200, 255));
        }
    }

    public abstract partial class ShapeData : Data
    {
        [ObservableProperty] private Brush _fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0));
    }


    public abstract partial class Data : ObservableObject
    {
        [ObservableProperty] private double _width;
        [ObservableProperty] private double _height;
        [ObservableProperty] private double _x;
        [ObservableProperty] private double _y;
        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] bool _isSelected = false;
    }
}

```

MainWindow.xaml.cs
```cs
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20260306
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public RectangleData MyRectangleData { get; set; } = new RectangleData();
        public EllipseData MyEllipseData { get; set; } = new EllipseData();
        public EllipseData MyEllipseData金色 { get; set; } = new EllipseData() { X = 150, Y = 50, Fill = Brushes.Gold };
        public EllipseData MyEllipseDataオリーブ { get; set; } = new EllipseData() { X = 100, Y = 150, Fill = Brushes.Olive };
        public EllipseData MyEllipseData緑 { get; set; } = new EllipseData() { X = 100, Y = 100, Fill = Brushes.MediumAquamarine };
        public GroupData MyGroupData { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();

            MyGroupData.DataList.Add(new RectangleData() { X = 20, Y = 80, Width = 50, Height = 50, Fill = Brushes.Gray });
            MyGroupData.DataList.Add(new EllipseData() { X = 40, Y = 120, Width = 50, Height = 50, Fill = Brushes.Salmon });
            MyGroupData.DataList.Add(MyEllipseData緑);

            GroupData groupData = new();
            groupData.DataList.Add(MyEllipseDataオリーブ);
            groupData.DataList.Add(MyEllipseData金色);
            MyGroupData.DataList.Add(groupData);


            this.DataContext = this;
        }
    }
}
```
これの「MyGroupData」をMainWindow.xamlにItemsControlで表示するXAMLを書いて


---


入れ子構造（再帰的な構造）のデータを `ItemsControl` で表示する場合、一番のポイントは **「`GroupData` 用の `DataTemplate` の中にもう一度 `ItemsControl` を書く」** ことです。

これにより、データが `GroupData` である限り、無限に階層を掘り下げて描画してくれます。また、各アイテムが $X, Y$ 座標を持っているため、`ItemsPanel` に `Canvas` を使用するのが最適です。

以下に `MainWindow.xaml` の実装例をまとめました。

---

### MainWindow.xaml

```xml
<Window x:Class="_20260306.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:_20260306"
        Title="Recursive Shapes" Height="450" Width="800">
    
    <Window.Resources>
        <Style x:Key="ShapeItemContainerStyle" TargetType="ContentPresenter">
            <Setter Property="Canvas.Left" Value="{Binding X}" />
            <Setter Property="Canvas.Top" Value="{Binding Y}" />
        </Style>

        <DataTemplate DataType="{x:Type local:RectangleData}">
            <Rectangle Width="{Binding Width}" 
                       Height="{Binding Height}" 
                       Fill="{Binding Fill}" 
                       Stroke="Black" StrokeThickness="1"/>
        </DataTemplate>

        <DataTemplate DataType="{x:Type local:EllipseData}">
            <Ellipse Width="{Binding Width}" 
                     Height="{Binding Height}" 
                     Fill="{Binding Fill}" 
                     Stroke="Black" StrokeThickness="1"/>
        </DataTemplate>

        <DataTemplate DataType="{x:Type local:GroupData}">
            <ItemsControl ItemsSource="{Binding DataList}"
                          ItemContainerStyle="{StaticResource ShapeItemContainerStyle}">
                <ItemsControl.ItemsPanel>
                    <ItemsPanelTemplate>
                        <Canvas IsItemsHost="True" />
                    </ItemsPanelTemplate>
                </ItemsControl.ItemsPanel>
            </ItemsControl>
        </DataTemplate>
    </Window.Resources>

    <Grid>
        <ItemsControl ItemsSource="{Binding MyGroupData.DataList}"
                      ItemContainerStyle="{StaticResource ShapeItemContainerStyle}">
            <ItemsControl.ItemsPanel>
                <ItemsPanelTemplate>
                    <Canvas Background="#F0F0F0" />
                </ItemsPanelTemplate>
            </ItemsControl.ItemsPanel>
        </ItemsControl>
    </Grid>
</Window>

```

---

### 実装のポイント解説

1. **再帰的な DataTemplate**:
`GroupData` に対する `DataTemplate` の中で、再び `ItemsSource="{Binding DataList}"` を指定した `ItemsControl` を配置しています。これが「入れ子の魔法」の正体です。WPFのテンプレートエンジンが、リストの中身が `Rectangle` なのか `Group` なのかを自動で判断して、適切なテンプレートを再帰的に適用してくれます。
2. **ItemContainerStyle**:
`ItemsControl` の直下の要素（`ContentPresenter`）に対して、`Canvas.Left` と `Canvas.Top` をバインドしています。これがないと、すべての図形が $(0, 0)$ に重なって表示されてしまいます。
3. **DataType による自動振分け**:
`DataTemplate` に `DataType` を指定し、`Key` を設定しないことで、WPFは型の一致するデータを見つけた時に自動的にそのテンプレートを適用します。

### 補足

もし「グループ内のアイテム座標 $(X, Y)$ を、グループの左上からの相対座標にしたい」という場合は、`GroupData` の `DataTemplate` 内にある `ItemsControl` を `Canvas` 等で包み、その Canvas 自体に `X, Y` を適用するように調整してくださいね。

これで、`MyGroupData` の中にどれだけ深く `GroupData` が入り込んでも、画面上にすべて描画されるようになります！

次は、これらのアイテムをマウスでドラッグして動かせるようにしてみますか？


---


# さっきと同じように「MyGroupData」をMainWindow.xamlにTreeViewで表示するXAMLを書いて


---


`ItemsControl` の時と同様、`TreeView` でも型（DataType）に基づいて表示を切り替えます。

大きな違いは、入れ子構造（子階層）を持つ `GroupData` には **`HierarchicalDataTemplate`** を使い、末端のデータ（EllipseやRectangle）には通常の **`DataTemplate`** を使う点です。

以下に `MainWindow.xaml` のコード例を示します。

---

### MainWindow.xaml

```xml
<Window x:Class="_20260306.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:_20260306"
        Title="Recursive TreeView" Height="450" Width="400">
    
    <Window.Resources>
        <HierarchicalDataTemplate DataType="{x:Type local:GroupData}" 
                                  ItemsSource="{Binding DataList}">
            <StackPanel Orientation="Horizontal">
                <TextBlock Text="📁 " Foreground="Goldenrod"/>
                <TextBlock Text="{Binding Name}" FontWeight="Bold"/>
                <TextBlock Text="{Binding DataList.Count, StringFormat=' ({0} items)'}" 
                           Foreground="Gray" FontSize="10" VerticalAlignment="Center"/>
            </StackPanel>
        </HierarchicalDataTemplate>

        <DataTemplate DataType="{x:Type local:EllipseData}">
            <StackPanel Orientation="Horizontal">
                <TextBlock Text="○ " Foreground="DodgerBlue" FontWeight="Bold"/>
                <TextBlock Text="Ellipse"/>
                <TextBlock Text="{Binding X, StringFormat=' (X:{0:F0},'}" Foreground="Gray" Margin="5,0,0,0"/>
                <TextBlock Text="{Binding Y, StringFormat=' Y:{0:F0})'}" Foreground="Gray"/>
            </StackPanel>
        </DataTemplate>

        <DataTemplate DataType="{x:Type local:RectangleData}">
            <StackPanel Orientation="Horizontal">
                <TextBlock Text="□ " Foreground="Tomato" FontWeight="Bold"/>
                <TextBlock Text="Rectangle"/>
                <TextBlock Text="{Binding X, StringFormat=' (X:{0:F0},'}" Foreground="Gray" Margin="5,0,0,0"/>
                <TextBlock Text="{Binding Y, StringFormat=' Y:{0:F0})'}" Foreground="Gray"/>
            </StackPanel>
        </DataTemplate>
    </Window.Resources>

    <Grid>
        <TreeView ItemsSource="{Binding MyGroupData.DataList}">
            <TreeView.ItemContainerStyle>
                <Style TargetType="TreeViewItem">
                    <Setter Property="IsSelected" Value="{Binding IsSelected, Mode=TwoWay}"/>
                    <Setter Property="IsExpanded" Value="True"/>
                </Style>
            </TreeView.ItemContainerStyle>
        </TreeView>
    </Grid>
</Window>

```

---

### 実装のポイント

* **`HierarchicalDataTemplate`**:
これが TreeView の核心です。`ItemsSource="{Binding DataList}"` と書くことで、「このノードの下には `DataList` の中身を表示してね」と指示しています。
* **型の自動判別**:
`ItemsControl` の時と同じく、`DataType` を指定した Template をリソースに置いておけば、WPFがデータの型を見て「これはグループだからフォルダのアイコンを出そう」「これは四角形だから □ を出そう」と自動で判断してくれます。
* **`ItemContainerStyle`**:
あなたの `Data` クラスには `IsSelected` プロパティがありますよね。これを `TreeViewItem` の `IsSelected` と双方向バインド (`TwoWay`) させることで、**「画面上のツリーで項目を選んだら、プログラム側の `IsSelected` も自動的に `true` になる」** という動きが作れます。

### 次のステップへの提案

これで「描画（Canvas）」と「構造の確認（TreeView）」の両方ができました。

**「TreeView で選んだ項目を、Canvas 上で強調表示（枠線を太くするなど）する」** といった連携機能を追加してみますか？