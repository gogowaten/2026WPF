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

namespace _20260331
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public GeoShapeData MyData { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            MyData = new();
            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //MyData.StrokeThickness = 30;
            //MyData.Stroke = Brushes.MediumAquamarine;

            MyData.Points.Add(new Point(50, 150));
            MyData.Points.Add(new Point(150, 100));
            MyData.Points.Add(new Point(250, 250));

            ////先頭座標変更
            //MyData.Points[0] = new Point(-10, 50);


        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Pointsクリア
            MyData.Points.Clear();
            //MyData.Points = new();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MyData.IsOffset = !MyData.IsOffset;
        }
    }
}