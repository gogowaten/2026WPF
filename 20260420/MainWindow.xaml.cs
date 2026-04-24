using System.Collections.ObjectModel;
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

namespace _20260420
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public GeoLineData MyData { get; set; }
        public ObservableCollection<Point> MyPoints { get; set; } = [];
        public MainWindow()
        {
            InitializeComponent();
            MyData = CreateData();
            DataContext = this;


        }


        private GeoLineData CreateData()
        {
            var data = new GeoLineData()
            {
                MyPoints = [new Point(80, 0), new Point(80, 100), new Point(2, 2)],
                //MyPoints = [new Point(80, 0), new Point(80, 100)],
                Stroke = Brushes.Plum,
                StrokeThickness = 20,
                Background = Brushes.CadetBlue,
                Fill = Brushes.Silver,
                //IsOffset = true,
                IsCanDragMove = true,
            };
            return data;
        }

        private void Button_Click_Check(object sender, RoutedEventArgs e)
        {
            var elementData = MyElement.DataContext;
            var neko = MyData;
            MyPoints.Add(new Point());
            MyPoints[0] = new Point(1, 1);
        }

        private void Button_Click_Add(object sender, RoutedEventArgs e)
        {
            MyData.MyPoints?.Add(new Point(150, 50));
            MyData.MyPoints?.Add(new Point(-20, 60));
            MyElement.UpdateVertexHandle();
        }

        private void Button_Click_Clear(object sender, RoutedEventArgs e)
        {
            MyData.MyPoints?.Clear();
            MyElement.UpdateVertexHandle();
        }

        private void Button_Click_StrokeColor(object sender, RoutedEventArgs e)
        {
            MyData.Stroke = Brushes.SlateBlue;
        }

        private void Button_Click_ChangeResize(object sender, RoutedEventArgs e)
        {
            MyElement.ChangeResizeHandleVisible();
        }

        private void Button_Click_VertexHandle(object sender, RoutedEventArgs e)
        {
            //MyElement.ShowVertexAdorner();
            MyData.IsVisibleVertexHandles= true;
        }

        private void Button_Click_VertexHandleHide(object sender, RoutedEventArgs e)
        {
            //MyElement.HideVertexAdorner();
            MyData.IsVisibleVertexHandles = false;
        }

        private void Button_Click_UpdateVertexHandles(object sender, RoutedEventArgs e)
        {
            //MyElement.UpdateVertexHandles();
            
        }

        private void Button_Click_PerfectlyFit(object sender, RoutedEventArgs e)
        {
            MyElement.PerfectlyFit();
        }
    }
}