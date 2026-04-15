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

namespace _20260413
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

        private void Button_Click_Add(object sender, RoutedEventArgs e)
        {
            //MyCanvasThumb.AddResizeAdorner2();
            MyCanvasThumb.AddResizeAdorner();
            ResizeAdorner.AddResizeAdorner(MyCanvasThumb2);
        }
        private void Button_Click_Remove(object sender, RoutedEventArgs e)
        {
            MyCanvasThumb.RemoveResizeAdorner();
            _ = ResizeAdorner.RemoveResizeAdorner(MyCanvasThumb2);
        }

    }
}