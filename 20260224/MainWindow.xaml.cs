using System.Collections.ObjectModel;
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
        public ObservableCollection<Item> ItemsList { get; set; } = [];
        //public Items ItemsList { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            //AddRectangle();
            //ItemsList = new(0, 0);
            PrepareData();
            this.DataContext = this;


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

        //private void PrepareData()
        //{

        //    ItemsList.Children.Add(new TextBlockItem(10, 10, "ゆっくりしていってね！！！"));
        //    ItemsList.Children.Add(new RectangleItem(150, 50, 100, 50) { R = 255, G = 0, B = 0 });

        //    var group = new Items(60, 150);
        //    group.Children.Add(new RectangleItem(0, 0, 89, 90) { R = 0, G = 0, B = 255 });
        //    group.Children.Add(new TextBlockItem(10, 90, "これはグループ内です"));
        //    ItemsList.Children.Add(group);

        //}

        private void PrepareData()
        {

            ItemsList.Add(new TextBlockItem(10, 10, "ゆっくりしていってね！！！"));
            ItemsList.Add(new RectangleItem(150, 50, 100, 50) { R = 255, G = 0, B = 0 });

            var group = new Items(60, 150);
            group.Children.Add(new RectangleItem(0, 0, 89, 90) { R = 0, G = 0, B = 255 });
            group.Children.Add(new TextBlockItem(10, 90, "これはグループ内です"));
            ItemsList.Add(group);

        }


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
    }
}