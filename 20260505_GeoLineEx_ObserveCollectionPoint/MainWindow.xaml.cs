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

namespace _20260505_GeoLineEx_ObserveCollectionPoint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public GeoLineEX MyShape { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            MyShape = new GeoLineEX()
            {
                MyPoints = [new Point(), new Point(100, 100)],
                Stroke = Brushes.Red,
                StrokeThickness = 20,
                MyBackground = Brushes.Pink,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,

            };
            MyCanvas.Children.Add(MyShape);

        }

       
    }
}