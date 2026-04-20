using Microsoft.VisualBasic;
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

namespace _20260419
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
            MyData = CreateGeoData();
            DataContext = MyData;
        }

        private GeoLineData CreateGeoData()
        {
            GeoLineData data = new()
            {
                MyPoints = [new Point(40, 20), new Point(60, 140)],
                Stroke = Brushes.Green,
                StrokeThickness = 20,
                IsOffset = false,
                Background = Brushes.Gold,
                Fill = Brushes.DeepSkyBlue,
                IsCanDragMove = true,

            };
            return data;
        }

        private void Button_Click_AddPoint(object sender, RoutedEventArgs e)
        {
            MyData.MyPoints?.Add(new Point(90, 50));
            //MyElement.MyPoints.Add(new Point(70, 30));
        }

        private void Button_Click_Vertex(object sender, RoutedEventArgs e)
        {
            
            if(AdornerLayer.GetAdornerLayer(MyElement.MyGeoLine) is AdornerLayer layer)
            {
                VertexAdorner adorner = new(MyElement.MyGeoLine);
                layer.Add(adorner);
            }
            
            //if(AdornerLayer.GetAdornerLayer(MyElement) is AdornerLayer layer)
            //{
            //    VertexAdorner adorner = new(MyElement);
            //    layer.Add(adorner);
            //}
        }

        private void Button_Click_IsOffset(object sender, RoutedEventArgs e)
        {
            MyData.IsOffset = !MyData.IsOffset;
        }

        private void Button_Click_Check(object sender, RoutedEventArgs e)
        {
            var po = MyData.MyPoints;
        }
    }
}