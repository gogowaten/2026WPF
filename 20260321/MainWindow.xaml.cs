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

namespace _20260321
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public RootData MyDataVM { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(MyLine);// 指定要素＋子要素
            Rect cbounds = VisualTreeHelper.GetContentBounds(MyLine); // 指定要素のみ
            Rect ccbounds = VisualTreeHelper.GetDescendantBounds(MyCanvas);// 指定要素＋子要素
            Rect cccbounds = VisualTreeHelper.GetContentBounds(MyCanvas); // 指定要素のみ
            
        }
    }
}