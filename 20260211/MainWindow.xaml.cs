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
        private PointCollection _points = [new Point(), new Point(100, 50)];
        private MainViewModel MyMainViewModel = new();
        
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = MyMainViewModel;
            //this.DataContext = new MainViewModel();

            MyPolyline.Points = MyMainViewModel.MyPoints;
        }
    }
}