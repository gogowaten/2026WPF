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

namespace _20260324
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
            MyData = new GeoLineData
            {
                Points = [new Point(50, 70), new Point(250, 150), new Point(50, 250), new Point(50, 200),]
            };
            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //MyLine.Points.Add(new Point(0, 10));
            //MyLine.Points.Add(new Point(0, 100));
            //MyLine.Points.Add(new Point(0, 200));
            MyData.Points[0] = new Point(0, 0);
            var geo = MyGeoLineThumb;
            var neko = MyGeoLineThumb.DataContext;
            var inu = MyGeoLineThumb.MyData;
        }
    }
}