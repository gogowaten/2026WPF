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

namespace _20260327_02_OffsetGeoShape
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public GeoShapeData MyData { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            MyData = new();
            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //MyShape.StrokeThickness = 30;
            //MyShape.Stroke = Brushes.MediumAquamarine;

            //MyShape.Points.Add(new Point(50, 150));
            //MyShape.Points.Add(new Point(150, 100));
            //MyShape.Points.Add(new Point(250, 250));

            ////先頭座標変更
            //MyShape.Points[0] = new Point(-10, 50);


        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Pointsクリア
            MyData.Points.Clear();

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            
            if(MyData is GeoShapeData data)
            {
                data.IsOffset = !data.IsOffset;


                //if (data.IsOffset)
                //{
                //    data.IsOffset= false;
                //    data.X -= data.OriginBounds.X;
                //    data.Y -= data.OriginBounds.Y;
                //}
                //else
                //{
                //    data.IsOffset = true;
                //    data.X += data.OriginBounds.X;
                //    data.Y += data.OriginBounds.Y;
                //}
            }
        }
    }
}