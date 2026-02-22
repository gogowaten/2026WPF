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

namespace _20260222_ResizePanel
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
            MyPanel.Resize();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ResizePanel.SetX(MyRect1, ResizePanel.GetX(MyRect1) + 10);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ResizePanel.SetX(MyRect1, ResizePanel.GetX(MyRect1) - 10);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            ResizePanel.SetX(MyRect2, ResizePanel.GetX(MyRect2) + 10);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            ResizePanel.SetX(MyRect2, ResizePanel.GetX(MyRect2) - 10);
        }
    }
}