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

namespace _20260327_01_OffsetGeoShape
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //MyShape.StrokeThickness = 30;
            //MyShape.Stroke = Brushes.MediumAquamarine;

            MyShape.Points.Add(new Point(50, 150));
            MyShape.Points.Add(new Point(150, 100));
            MyShape.Points.Add(new Point(250, 250));

            //先頭座標変更
            MyShape.Points[0] = new Point(-10, 50);


        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Pointsクリア
            MyShape.Points.Clear();

        }
    }
}