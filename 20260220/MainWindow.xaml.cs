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

namespace _20260220
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            TextBlock tb = new() { Text = "yukkuri", FontSize = 16 };
            //tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Size desired = tb.DesiredSize;
            MyCanvas.Children.Add(tb);

            Initialized += (s, e) =>
            {
                var rend = tb.RenderSize;
                var wid = tb.Width;
            };

            Loaded += (s, e) =>
            {
                var width = tb.ActualWidth;
                var wid = tb.Width;
            };
        }

        private void RectSizeChange_Click(object sender, RoutedEventArgs e)
        {
            MyRectangle.Width = 120;
            MyRectangle.Height = 90;
        }

        private void TextBlockTextChange_Click(object sender, RoutedEventArgs e)
        {
            Canvas.SetTop(MyTextBlock, 120);
            MyTextBlock.Text = "ゆっくりしていってね！！！";
        }
    }
}