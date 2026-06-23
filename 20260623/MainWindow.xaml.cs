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

namespace _20260623
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string ImagePath = "D:\\ブログ用\\テスト用画像\\collection3.png";
        public MainWindow()
        {
            InitializeComponent();

            BitmapImage img = new();
            img.BeginInit();
            img.UriSource = new Uri(ImagePath);
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.EndInit();

            MyImage.Source = img;
        }
    }
}