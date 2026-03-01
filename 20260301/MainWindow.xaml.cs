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

namespace _20260301
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public RootData MyRootData { get; set; } = null!;
        public RectangleData RectangleData1 { get; set; } = new(20, 20, 100, 50);
        public RectangleData RectangleData2 { get; set; } = new(20, 100, 100, 50);
        public RectangleData RectangleData3 { get; set; } = new(100, 20, 100, 50);
        public GroupData GroupDataA { get; set; } = new();
        public GroupData GroupDataB { get; set; } = new();
        public GroupData MyRootGroupData { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();

            //this.DataContext = this;
            //InitRootData();

            //MyRootNodeThumb.MyRootData = MyRootData;
        }

        private void InitRootData()
        {
            GroupDataA.Children.Add(RectangleData1);
            GroupDataA.Children.Add(RectangleData2);

            MyRootGroupData.Children.Add(RectangleData3);
            MyRootGroupData.Children.Add(GroupDataA);
        }
    }
}