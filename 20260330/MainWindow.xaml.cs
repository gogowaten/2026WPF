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

namespace _20260330
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if(AdornerLayer.GetAdornerLayer(TargetRect) is AdornerLayer layer)
            {
                layer.Add(new ResizeAdorner(TargetRect));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MyCanvas.UpdateSizeToContent();
            if (AdornerLayer.GetAdornerLayer(MyCanvas) is AdornerLayer layer)
            {
                layer.Add(new ResizeAdorner(MyCanvas));
            }
        }
    }
}