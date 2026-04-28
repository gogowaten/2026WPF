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

namespace _20260428_GeoLineEx
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
            MyEx.MyPoints.Add(new Point(200, 200));
            MyEx.IsVertexHandle = !MyEx.IsVertexHandle;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var rect = MyEx.GetSurfaceBounds();
            MessageBox.Show($"{rect}");
        }
    }
}