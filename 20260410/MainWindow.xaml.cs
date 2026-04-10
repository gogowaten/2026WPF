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
                Fill = Brushes.Silver,
                IsOffset = true,
                IsCanDragMove = true,
            };
            return data;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MyData?.MyPoints?.Add(new Point(0, 10));
            MyData?.MyPoints?.Add(new Point(100, 30));
        }

        private void Button_Click_Clear(object sender, RoutedEventArgs e)
        {
            MyData.MyPoints?.Clear();
        }

        private void Button_Click_ChangePoint(object sender, RoutedEventArgs e)
        {
            MyData.MyPoints?[0] = new Point();
        }

        private void Button_Click_Add2(object sender, RoutedEventArgs e)
        {
            MyData.MyPoints?.Add(new Point(-40, -20));
        }
    }
}