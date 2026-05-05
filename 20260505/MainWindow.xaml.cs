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
        public MainWindow()
        {
            InitializeComponent();
            MyElement = MyTest();
            MyCanvas.Children.Add(MyElement);
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
            MyElement.MyPoints.Add(new Point(50, 200));
            //MyElement.InvalidateMeasure();
            //MyElement.InvalidateVisual();
        }
    }
}