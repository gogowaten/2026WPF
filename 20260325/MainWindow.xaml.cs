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

namespace _20260325
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

            MyData = new GeoShapeData()
            {
                Name = "ベジェ曲線",
                Stroke = Brushes.MediumAquamarine,
                StrokeThickness = 20.0,
                Points =
                [
                    new Point(50, 70),
                    new Point(250, 150),
                    new Point(50, 250),
                    new Point(50, 200),
                    new Point(50, 150),
                    new Point(150, 100),
                    new Point(250, 250),
                ],
                StrokeEndLineCap = PenLineCap.Round,
                Background = Brushes.Gray
            };

            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dc = MyThumb.DataContext;
            var data = MyThumb.MyData;

            // 以下は描画更新される
            //MyData.Stroke = Brushes.Red;
            //MyData.StrokeThickness = 30;

            MyData.Points[0] = new Point();

            //// Pointの追加では描画更新されない
            //MyData.Points.Add(new Point(10, 10));
            //MyData.Points.Add(new Point(10, 100));
            //MyData.Points.Add(new Point(10, 200));

            
            //MyGeo.Points.Add(new Point(10, 10));
            //MyGeo.Points.Add(new Point(10, 100));
            //MyGeo.Points.Add(new Point(10, 200));

            
        }
    }
}