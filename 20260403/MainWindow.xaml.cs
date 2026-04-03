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

namespace _20260403
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public GeoLineData MyData { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            MyData = TestGeoLineData();
            DataContext = this;
        }

        private GeoLineData TestGeoLineData()
        {
            GeoLineData data = new()
            {
                Background = Brushes.DeepSkyBlue,
                //Fill = Brushes.DeepSkyBlue;
                Stroke = Brushes.Gold,
                StrokeThickness = 20,
                Points = [(new Point(50, 70)),
                        (new Point(250, 150)),
                        (new Point(50, 250)),
                        (new Point(50, 200)),],
            };
            return data;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (MyData.Points.Count > 0)
            {
                MyData.Points[0] = new Point(10, 10);
            }
        }

        private void Button_Click_ChangeOffset(object sender, RoutedEventArgs e)
        {
            MyData.IsOffset = !MyData.IsOffset;
        }

        private void Button_Click_AddPoints(object sender, RoutedEventArgs e)
        {
            MyData.Points.Add(new Point(250, 150));
            MyData.Points.Add(new Point(50, 250));
            MyData.Points.Add(new Point(50, 200));
        }

        private void Button_Click_Check(object sender, RoutedEventArgs e)
        {
            var myData = MyData;
            var elementDC = MyElement.DataContext;
            //var elementMyData = MyElement.MyData;
        }

        private void Button_Click_PointsClear(object sender, RoutedEventArgs e)
        {
            MyData.Points.Clear();
        }

        private void Button_Click_ChangeCanDragMove(object sender, RoutedEventArgs e)
        {
            MyData.IsCanDragMove = !MyData.IsCanDragMove;
        }
    }
}