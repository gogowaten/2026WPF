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

namespace _20260306_DataTemplateGroupData
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public RectangleData MyRectangleData { get; set; } = new RectangleData() { X = 0, Y = 0 };
        public EllipseData MyEllipseData { get; set; } = new EllipseData();
        public EllipseData MyEllipseData金色 { get; set; } = new EllipseData() { X = 150, Y = 50, Fill = Brushes.Gold };
        public EllipseData MyEllipseDataオリーブ { get; set; } = new EllipseData() { X = 100, Y = 150, Fill = Brushes.Olive };
        public EllipseData MyEllipseData緑 { get; set; } = new EllipseData() { X = 100, Y = 100, Fill = Brushes.MediumAquamarine };
        public GroupData MyGroupData { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();

            MyGroupData.DataList.Add(new RectangleData() { X = 20, Y = 80, Width = 50, Height = 50, Fill = Brushes.Gray });
            MyGroupData.DataList.Add(new EllipseData() { X = 40, Y = 120, Width = 50, Height = 50, Fill = Brushes.Salmon });
            MyGroupData.DataList.Add(MyEllipseData緑);
            MyGroupData.DataList.Add(MyRectangleData);

            GroupData groupData = new() { X = 100, Y = 50 };
            groupData.DataList.Add(MyEllipseDataオリーブ);
            groupData.DataList.Add(MyEllipseData金色);
            groupData.DataList.Add(new RectangleData() { X = 0, Y = 0, Width = 30, Height = 30, Fill = Brushes.YellowGreen });
            MyGroupData.DataList.Add(groupData);


            this.DataContext = this;
        }
    }
}