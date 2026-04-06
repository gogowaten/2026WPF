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

namespace _20260406
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
            GeoLineData data = new()
            {
                Points = [new Point(20, 30), new Point(120, 10), new Point(10, 100)],
                Stroke = Brushes.HotPink,
                StrokeThickness = 20,
                Background = new SolidColorBrush(Color.FromArgb(100, 200, 200, 100)),// Brushes.Gainsboro,
                IsCanDragMove = true,
                IsOffset = true,
                MiterLimit = 10,
                Name = "テスト用"
            };
            return data;
        }

        private void Button_Click_Check(object sender, RoutedEventArgs e)
        {
            var neko = MyElement.DataContext;
        }

        private void Button_Click_ChangeDragMove(object sender, RoutedEventArgs e)
        {
            //MyElement.ChangeGeoLineDragMove();
            MyData.IsCanDragMove = !MyData.IsCanDragMove;
        }
    }
}