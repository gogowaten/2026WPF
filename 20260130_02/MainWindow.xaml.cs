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

namespace _20260130_02
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
            Rectangle rectangle = new Rectangle
            {
                Width = 50,
                Height = 50,
                Fill = Brushes.Salmon,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
            };
            AutoSizingPanel.SetLeft(rectangle, 200 + myAutoPanel.Children.Count * 30);
            myAutoPanel.Children.Add(rectangle);
        }
    }
}