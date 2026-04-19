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
                MyPoints = [new Point(), new Point(20, 100)],
                Stroke = Brushes.Green,
                StrokeThickness = 20,
                IsOffset = true,
                Background = Brushes.Gold,
                Fill = Brushes.DeepSkyBlue,
                IsCanDragMove = true,

            };
            return data;
        }

        private void Button_Click_AddPoint(object sender, RoutedEventArgs e)
        {
            MyElement.MyPoints.Add(new Point(50, 10));
        }
    }
}