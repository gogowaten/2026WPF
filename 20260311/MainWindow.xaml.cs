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

namespace _20260311
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public RootData MyDataVM { get; set; } = new();
        public GeoLineData2 MyData { get; set; } = null!;

        public MainWindow()
        {
            InitializeComponent();
            MyData = GeoLineData2();
            this.DataContext = this;
        }

        private GeoLineData2 GeoLineData2()
        {
            GeoLineData2 data = new GeoLineData2()
            {
                Background = Brushes.Lavender,
                //Fill = Brushes.DeepSkyBlue;
                Stroke = Brushes.Orchid,
                StrokeThickness = 20,
                Points = [(new Point(50, 70)),
                        (new Point(100, 150)),
                        (new Point(50, 250)),
                        (new Point(50, 200)),],
                MiterLimit = 10,
                Width=200,Height=200,
                //IsOffset = true,
                //IsCanDragMove = true,
            };
            return data;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dc = MyAAA.DataContext;
            //MyTTText.Text = "acb";
            foreach (var item in MyDataVM.DataList)
            {
                if(item is TextBlockData txt)
                {
                    //txt.Text = "ゆっくりしていってね！！！";
                    txt.FontSize++;
                }
                if(item is GeoShapeData geo)
                {
                    geo.Points.Add(new Point(0, 10));
                    geo.Points.Add(new Point(10, 100));
                    geo.Points.Add(new Point(0, 200));
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            if (MyDataVM.RootData is RootData root)
            {
                if(root.CurrentItem is Data data)
                {

                    data.Z++;
                }
            }
        }

        private void Check_Click(object sender, RoutedEventArgs e)
        {
            var data = MyDataVM;
            //var edata = MyTestElement.MyGeoData;
        }

        private void Button_Click_geoLineChangeOffset(object sender, RoutedEventArgs e)
        {
            if(MyDataVM.CurrentItem is GeoLineData2 data)
            {
                data.IsOffset = !data.IsOffset;
            }
        }

        private void Button_Click_geoLineChangeMove(object sender, RoutedEventArgs e)
        {
            if (MyDataVM.CurrentItem is GeoLineData2 data)
            {
                data.IsCanDragMove = !data.IsCanDragMove;
            }
        }
    }
}