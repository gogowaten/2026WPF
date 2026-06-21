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

namespace _20260621
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string MyPicturePath = "D:\\ブログ用\\テスト用画像\\collection3.png";
        //public BitmapImage MyBitmap {  get; set; }
        public MainWindow()
        {
            InitializeComponent();

            MyBitmap = new();
            MyBitmap.BeginInit();
            MyBitmap.UriSource = new Uri(MyPicturePath);
            MyBitmap.CacheOption = BitmapCacheOption.OnLoad;
            MyBitmap.EndInit();

            MyImage.Source = MyBitmap;

            var bb = (BitmapSource)MyImage.Source;
        }



        public BitmapImage MyBitmap
        {
            get { return (BitmapImage)GetValue(MyBitmapProperty); }
            set { SetValue(MyBitmapProperty, value); }
        }
        public static readonly DependencyProperty MyBitmapProperty =
            DependencyProperty.Register(nameof(MyBitmap), typeof(BitmapImage), typeof(MainWindow), new PropertyMetadata(null));

        public double MyScale
        {
            get { return (double)GetValue(MyScaleProperty); }
            set { SetValue(MyScaleProperty, value); }
        }
        public static readonly DependencyProperty MyScaleProperty =
            DependencyProperty.Register(nameof(MyScale), typeof(double), typeof(MainWindow), new PropertyMetadata(40.0));

        private void MyScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            MyDraw.InvalidateVisual();
        }
    }
}