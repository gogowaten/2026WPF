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

namespace _20260309_DataTemplate_CustomControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public GroupData MyRootData { get; set; } = new();
        public MainWindow()
        {
            InitializeComponent();
            MyInit();
            DataContext = this;
        }

        private void MyInit()
        {
            RectangleData rRed = new() { Name = "RedRect", X = 0, Y = 0, Width = 60, Height = 60, Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)) };
            RectangleData rBlue = new() { Name = "BlueRect", X = 20, Y = 20, Width = 60, Height = 60, Fill = new SolidColorBrush(Color.FromArgb(50, 0, 0, 255)) };
            EllipseData maruRed = new() { Name = "RedEllipse", X = 0, Y = 0, Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)) };
            EllipseData maruBlue = new() { Name = "BlueEllipse", X = 20, Y = 20, Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromArgb(50, 0, 0, 255)) };
            EllipseData maruGreen = new() { Name = "GreenEllipse", X = 40, Y = 140, Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromArgb(50, 0, 255, 0)) };

            GroupData groupRect = new() { Name = "GropuA", X = 0, Y = 0 };
            groupRect.DataList.Add(rRed);
            groupRect.DataList.Add(rBlue);

            GroupData groupEllipse = new() { Name = "GropuB", X = 100, Y = 0 };
            groupEllipse.DataList.Add(maruRed);
            groupEllipse.DataList.Add(maruBlue);

            MyRootData.DataList.Add(groupRect);
            MyRootData.DataList.Add(groupEllipse);
            MyRootData.DataList.Add(maruGreen);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var neko = MyAAA.DataContext;
            var itemssource = MyAAA.ItemsSource;
        }
    }
}