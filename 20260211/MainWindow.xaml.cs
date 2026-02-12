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

// Google GeminiWPFで折れ線描画アプリ作成

// https://gemini.google.com/app/6fd6e42819f46efa?hl=ja

namespace _20260211
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new MainViewModel();

            double thickness = 4;
            Point p1 = new();
            Point p2 = new(100, 0);
            Vector vec = p2 - p1;
            Vector direction = p2 - p1;
            direction.Normalize();
            double arrowLength = thickness * 5; // 太さに応じたサイズ
            double arrowWidth = thickness * 4;

            Vector normal = new Vector(-direction.Y, direction.X);
            Point basePoint = p2 - (direction * arrowLength);
            Point left = basePoint + (normal * arrowWidth / 2);
            Point right = basePoint - (normal * arrowWidth / 2);

        }
    }
}