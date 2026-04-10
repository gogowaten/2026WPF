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

namespace _20260410
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
            MyData = CreateData();
            DataContext = this;
        }

        private GeoLineData CreateData()
        {
            var data = new GeoLineData()
            {
                MyPoints = [new Point(80, 0), new Point(80, 100)],
                Stroke = Brushes.LimeGreen,
                StrokeThickness = 20,
                Background = Brushes.CadetBlue,
                Fill = Brushes.Gray,
                IsOffset = true,
                IsCanDragMove = true,
            };
            return data;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}