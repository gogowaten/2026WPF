using System.Collections.ObjectModel;
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

namespace _20260505
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public GeoLineEX MyElement;
        public RootData MyData { get; set; }

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG   
            this.Left = 0;
            this.Top = 0;
#endif
            //MyElement = MyTest();
            //MyCanvas.Children.Add(MyElement);

            MyData = CreateRootData();
            this.DataContext = this;
        }

        private RootData CreateRootData()
        {
            RootData root = new();
            EllipseData ellipse = new() { X = 20, Y = 30, Fill = Brushes.Olive, Width = 30, Height = 30 };
            root.AddData(ellipse);

            //EllipseData ellipseBlue = new() { Fill = Brushes.Blue, Width = 30, Height = 30 };
            //GroupData group = new();
            //group.AddData(ellipseBlue);
            //group.AddData(new EllipseData() { X = 10, Y = 50, Fill = Brushes.Orange, Width = 30, Height = 30 });
            //root.AddData(group);

            root.AddData(CreateGeoLineGroupData());
            //root.AddData(CreateGeoLineData());
            return root;
        }


        private GeoLineData CreateGeoLineData()
        {
            GeoLineData geo = new()
            {
                X = 100,
                Y = 100,
                Points = [new Point(50, 0), new Point(100, 00), new Point(50, 100)],
                //Points = [new Point(50, 0), new Point(100, 00)],
                //Points = [new Point(-50, 0), new Point(100, 00)],
                //Points = [new Point(), new Point(100, 100)],
                Stroke = Brushes.Crimson,
                StrokeThickness = 30,
                //Background = Brushes.Pink,
                //IsCanDragMove = true,
                //IsVertexHandle = true,
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
                //IsCanDragMove = true,
                //IsVertexHandle = true,
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
                //IsCanDragMove = true,
                //IsVertexHandle = true,
            };
            group.AddData(geo);
            return group;
        }

        private GeoLineEX MyTest()
        {
            var geo = new GeoLineEX()
            {
                MyPoints = [new Point(), new Point(100, 100)],
                Stroke = Brushes.Red,
                StrokeThickness = 20,
            };
            Canvas.SetLeft(geo, 20);
            Canvas.SetTop(geo, 20);
            return geo;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //if(MyData.DataList[1] is GeoLineData data)
            //{
            //    data.Points.Add(new Point(200, 50));
            //}
            if (MyRootItems.MyData.MyClickedItem?.MyContent is GeoLineEX geo)
            {
                geo.MyPoints.Add(new Point(200, 50));
            }
            //MyElement.MyPoints.Add(new Point(50, 200));
            //MyElement.InvalidateMeasure();
            //MyElement.InvalidateVisual();
        }

        private void background_Click(object sender, RoutedEventArgs e)
        {

        }

        private void check_Click(object sender, RoutedEventArgs e)
        {
            var clicked = MyData.ClickedItemData;
            var rootcliked = MyRootItems.MyData.MyClickedItem.MyData;
            var clickeditem = MyRootItems.MyData.MyClickedItem;
            if (MyData.ClickedItemData is GeoLineData data)
            {
                var ps = data.Points;
                var bo = data.Bounds;
                if (Math.Abs(bo.X + bo.Y) < 0.01) { return; }

                var op = new ObservableCollection<Point>();
                foreach (Point item in ps)
                {
                    op.Add(new Point(item.X - bo.X, item.Y - bo.Y));
                }
                data.Points = op;

            }
        }

        private void vertexhandle_Click(object sender, RoutedEventArgs e)
        {
            if (MyData.ClickedItemData is GeoLineData data)
            {
                data.IsVertexHandle = !data.IsVertexHandle;
            }
        }

        private void RemoveEndPoint_Click(object sender, RoutedEventArgs e)
        {
            if (MyData.ClickedItemData is GeoLineData data)
            {
                data.Points.RemoveAt(data.Points.Count - 1);
            }
        }

        private void InsertStartPoint_Click(object sender, RoutedEventArgs e)
        {
            if (MyData.ClickedItemData is GeoLineData data)
            {
                data.Points.Insert(0, new Point(100, 100));
            }
        }
    }
}