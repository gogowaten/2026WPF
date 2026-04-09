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

namespace _20260409
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //public Geometry MyPGAAA { get; set; }
        public GeoLineData MyPG { get; set; }
        //public PathGeometry MyPG { get; set; }
        

        public MainWindow()
        {
            InitializeComponent();
            MyPG = CreateData();
            //MyPGAAA = AAA();
            DataContext = this;
        }

        private PathGeometry AAA()
        {
            Path p = new()
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 20
            };
            PolyLineSegment seg = new([new Point(100, 0), new Point(100, 100)], true);
            PathFigure fig = new(new Point(), [seg], false);
            PathGeometry pg = new([fig]);
            return pg;
        }

        private GeoLineData CreateData()
        {
            var data = new GeoLineData()
            {
                Background = Brushes.Lavender,
                //Fill = Brushes.DeepSkyBlue;
                Stroke = Brushes.Orchid,
                StrokeThickness = 20,
                MyPoints = [(new Point(50, 0)), (new Point(50, 100)),],
                //MyPoints = [(new Point(50, 70)),
                //        (new Point(100, 150)),
                //        (new Point(50, 250)),
                //        (new Point(50, 200)),],
                MiterLimit = 10,
                IsCanDragMove = false,
                IsOffset = true
            };
            return data;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var data = MyPG;
            //MyShape.InvalidateVisual();
            //MyShape.InvalidateMeasure();
            //MyShape.InvalidateArrange();
            //MyShape.UpdateLayout();
        }

        private void Button_Click_Add(object sender, RoutedEventArgs e)
        {
            MyPG.MyPoints?.Add(new Point(0, 10));
        }
    }
}