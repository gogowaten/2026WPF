using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20260224
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Items RootItem { get; set; } = new(0, 0);
        public ObservableCollection<Item> ItemsList { get; set; } = [];


        public MainWindow()
        {
            InitializeComponent();

            //PrepareData();

            //this.DataContext = this;

            PrepareDataItems();
            this.DataContext = this;

            // RootItem の子供たちの中で何かが起きたら、RootItem 自身を再計算させる
            RootItem.Children.CollectionChanged += (s, e) => RootItem.UpdateBounds();

            // RootItem 自体のプロパティ（子供の X, Y, Width, Height）の変化を監視
            // これをしないと、子供が移動したときに Root のサイズが変わらない
            RootItem.UpdateBounds();

            //JsonSerializerOptions options = new() { WriteIndented = true };
            //RectangleItem rectangleItem = new(10, 20, 40, 50);
            //rectangleItem.A = 0;
            //rectangleItem.R = 0;
            //rectangleItem.G = 0;
            //rectangleItem.B = 0;
            //var jj = JsonSerializer.Serialize(rectangleItem, options);

            //Items items = new(10, 20);
            //// Children.Add は使わないで、AddChildを使う
            ////items.Children.Add(new TextBlockItem(10, 20, "TestAAA"));

            //items.AddChild(new TextBlockItem(10, 20, "TestAAA"));
            //items.AddChild(new RectangleItem(10, 80, 100, 50));
            //root = new(0, 0);
            //root.AddChild(items);
            //root.AddChild(new TextBlockItem(100, 20, "TestBBB"));
            //root.AddChild(root);



            //var json = JsonSerializer.Serialize(root, options);
            //Items? result = JsonSerializer.Deserialize<Items>(json, options);

        }



        private void PrepareDataItems()
        {
            RootItem.Background = Brushes.Black;

            //RootItem.Children.Add(new TextBlockItem(10, 10, "ゆっくりしていってね！！！") { Background = Brushes.Gold });
            //RootItem.Children.Add(new RectangleItem(150, 50, 100, 50) {Background = Brushes.Cyan });

            var group = new Items(60, 150);
            group.Children.Add(new RectangleItem(0, 0, 89, 90) { Background = Brushes.Maroon });
            group.Children.Add(new TextBlockItem(50, 90, "これはグループ内です！！！！") { Background = Brushes.YellowGreen });
            group.Background = Brushes.LightGray;

            RootItem.Children.Add(group);
        }

        private void PrepareData()
        {
            Items root = new(0, 0);
            ItemsList.Add(root);

            root.Children.Add(new TextBlockItem(10, 10, "ゆっくりしていってね！！！") { Background = Brushes.Gold });
            root.Children.Add(new RectangleItem(150, 50, 100, 50) { BackgroundR = 255, BackgroundG = 0, BackgroundB = 0 });

            var group = new Items(60, 250);
            group.Children.Add(new RectangleItem(70, 0, 89, 90) { Background = Brushes.Maroon });
            group.Children.Add(new TextBlockItem(50, 90, "これはグループ内です！！！！") { Background=Brushes.YellowGreen});
            group.Background = Brushes.LightGray;
            root.Children.Add(group);


        }


        //private void PrepareData()
        //{
        //    ItemsList.Add(new TextBlockItem(10, 10, "ゆっくりしていってね！！！") { Background = Brushes.Gold });
        //    ItemsList.Add(new RectangleItem(150, 50, 100, 50) { BackgroundR = 255, BackgroundG = 0, BackgroundB = 0 });

        //    var group = new Items(60, 150);
        //    group.Children.Add(new RectangleItem(70, 0, 89, 90) { Background = Brushes.Maroon });
        //    group.Children.Add(new TextBlockItem(50, 90, "これはグループ内です！！！！"));
        //    group.Background = Brushes.LightGray;
        //    ItemsList.Add(group);

        //}


        private void AddRectangle()
        {
            Rectangle rectangle = new() { Width = 100, Height = 100 };
            LinearGradientBrush liniar = new()
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            liniar.GradientStops.Add(new GradientStop(Colors.Yellow, 0.0));
            liniar.GradientStops.Add(new GradientStop(Colors.Brown, 0.5));
            liniar.GradientStops.Add(new GradientStop(Colors.Cyan, 1.0));
            rectangle.Fill = liniar;
            MyCanvas.Children.Add(rectangle);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //ItemsList[1].Background = Brushes.Black;
            //if (ItemsList[2] is Items ii)
            //{
            //    ii.Children[0].Background = Brushes.Cyan;
            //}
        }

        private void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is TextBlock tb && tb.DataContext is TextBlockItem item)
            {
                item.Width = e.NewSize.Width;
                item.Height = e.NewSize.Height;

                // 親サイズの再計算
                item.Parent?.UpdateBounds();
            }
        }

        public static void SaveToImage(FrameworkElement element, string filePath)
        {
            int width = (int)element.ActualWidth;
            int height = (int)element.ActualHeight;
            if (width <= 0 || height <= 0) { return; }

            /*⚠️ 注意点：DPIスケーリング
            Windowsの設定で「テキストの拡大（125 % など）」にしている場合、そのまま保存すると画像がボケたり、サイズがズレたりすることがあります。
            完璧な解像度を保つには、以下のようにシステムのDPIを取得して RenderTargetBitmap に渡すのがプロの技です。*/
            Visual visual = element;
            var source = PresentationSource.FromVisual(visual);
            double dpiX = 96.0 * source.CompositionTarget.TransformFromDevice.M11;
            double dpiY = 96.0 * source.CompositionTarget.TransformFromDevice.M22;

            RenderTargetBitmap rtb = new(width, height, dpiX, dpiY, PixelFormats.Pbgra32);

            rtb.Render(element);

            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using FileStream fs = File.OpenWrite(filePath);
            encoder.Save(fs);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg",
                Title = "全体を画像として保存",
                FileName = "MyDiagram.png"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    SaveToImage(MyDiagramCanvas, saveFileDialog.FileName);
                    MessageBox.Show("保存した");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存中にエラーが発生しました: {ex.Message}");
                    throw;
                }
            }

        }
    }
}