
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
            //GroupData group = new();
            //group.AddData(ellipseBlue);
            //group.AddData(new EllipseData() { X = 10, Y = 50, Fill = Brushes.Orange, Width = 30, Height = 30 });
            //root.AddData(group);
            //root.AddData(CreateGeoLineGroupData());
            root.AddData(CreateGeoLineData());
            return root;
        }

        private GeoLineData CreateGeoLineData()
        {
            GeoLineData geo = new()
            {
                X = 100,
                Y = 100,
                //Points = [new Point(50, 0), new Point(100, 00), new Point(50, 100)],
                Points = [new Point(50, 0), new Point(100, 00)],
                //Points = [new Point(-50, 0), new Point(100, 00)],
                //Points = [new Point(), new Point(100, 00)],
                Stroke = Brushes.Crimson,
                StrokeThickness = 50,
                Background = Brushes.Pink,
                IsCanDragMove = true,
                IsVertexHandle = true,
            };
            return geo;
        }

        private GroupData CreateGeoLineGroupData()
        {
            GeoLineData geo = new()
            {
                X = 0,
                Y = 0,
                Points = [new Point(), new Point(50, 00)],
                Stroke = Brushes.Crimson,
                StrokeThickness = 20,
                Background = Brushes.Pink,
                IsCanDragMove = true,
                IsVertexHandle = true,
            };
            GroupData group = new() { X = 100, Y = 100 };
            group.AddData(geo);

            geo = new()
            {
                X = 150,
                Y = 50,
                Points = [new Point(), new Point(50, 00)],
                Stroke = Brushes.Green,
                StrokeThickness = 20,
                Background = Brushes.YellowGreen,
                IsCanDragMove = true,
                IsVertexHandle = true,
            };
            group.AddData(geo);
            return group;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(MyData.ClickedItemData is GeoLineData data)
            {
                //data.IsVertexHandle = !data.IsVertexHandle;
                data.Points.Add(new Point(100, 100));
                //data.StrokeStartLineCap = PenLineCap.Round;
            }
            //var bounds = MyTest.GetRenderBoundsWithPen();
            //MyTest.IsVertexHandle = !MyTest.IsVertexHandle;


            //MyLine.VertexTopLeftZeroFix();

            //Pen pp = new Pen()
            //{
            //    Thickness = MyLine.StrokeThickness,
            //};
            //Rect re = MyLine.RenderedGeometry.GetRenderBounds(pp);
            //PointCollection pc = MyLine.Points;
            //for (int i = 0; i < pc.Count; i++)
            //{
            //    var poi = pc[i];
            //    MyLine.Points[i] = new Point(poi.X - re.X, poi.Y - re.Y);

            //}


        }
    }
}