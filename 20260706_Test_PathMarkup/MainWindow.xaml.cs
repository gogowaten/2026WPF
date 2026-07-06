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

namespace _20260706_Test_PathMarkup
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
            var neko = VisualTreeHelper.GetContentBounds(MyCanvas); // empty
            var tako = VisualTreeHelper.GetDescendantBounds(MyCanvas);
            
            var inu = VisualTreeHelper.GetContentBounds(MyPath);
            var uma = VisualTreeHelper.GetDescendantBounds(MyPath);

            var ika = MyPath.RenderedGeometry.GetRenderBounds(new Pen(Brushes.Red, 1));
        }
    }
}