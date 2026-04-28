
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

namespace _20260428
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public RootData MyData { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            MyData = CreateData();

            DataContext = this;
        }

        private RootData CreateData()
        {
            RootData root = new();
            EllipseData ellipse = new() { X = 20, Y = 30, Fill = Brushes.Olive, Width = 30, Height = 30 };
            root.AddData(ellipse);

            EllipseData ellipseBlue = new() { Fill = Brushes.Blue, Width = 30, Height = 30 };
            GroupData group = new();
            group.AddData(ellipseBlue);
            group.AddData(new EllipseData() { X = 10, Y = 50, Fill = Brushes.Orange, Width = 30, Height = 30 });
            root.AddData(group);
            root.AddData(CreateGeoLineData());
            return root;
        }

        private GroupData CreateGeoLineData()
        {

            GeoLineData geo = new()
            {
                X = 0,
                Y = 0,
                Points = [new Point(), new Point(100, 100), new Point(-20, 50)],
                Stroke = Brushes.Crimson,
                StrokeThickness = 20,
                Background = Brushes.Pink,
                InternalX = 0,
                InternalY = 0,
                IsCanDragMove = true,
                IsVertexHandle = true,                
            };
            GroupData group = new() { X = 100, Y = 100 };
            group.AddData(geo);

            geo = new()
            {
                X = 100,
                Y = 50,
                Points = [new Point(), new Point(100, 100)],
                Stroke = Brushes.Green,
                StrokeThickness = 20,
                Background = Brushes.YellowGreen,
                InternalX = 0,
                InternalY = 0,
                IsCanDragMove = true,
                IsVertexHandle = true,                
            };
            group.AddData(geo);
            return group;
        }


    }
}