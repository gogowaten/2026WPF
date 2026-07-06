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

namespace _20260705_Test_Pbgra32
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

        private void Test(FrameworkElement element)
        {
            var width = element.ActualWidth;
            var height = element.ActualHeight;
            int pw = (int)Math.Ceiling(width);
            int ph = (int)Math.Ceiling(height);
            Rect drawRect = new(0, 0, pw, ph);

            VisualBrush brush = new(element);
            brush.Stretch = Stretch.None;
            DrawingVisual dv = new();
            using (var context = dv.RenderOpen())
            {
                context.DrawRectangle(brush, null,drawRect);
            }

            RenderTargetBitmap bmp = new(pw, ph, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(dv);

            RenderTargetBitmap visualBmp = new(pw, ph, 96, 96, PixelFormats.Pbgra32);
            visualBmp.Render(element);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Test(MyElement);
            Test(MyButton);
        }
    }
}