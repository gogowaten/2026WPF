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

namespace _20260306_DataTemplateBindingData
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public RectangleData MyRectangleData { get; set; } = new RectangleData();
        public EllipseData MyEllipseData { get; set; } = new EllipseData();
        public EllipseData My金丸Data { get; set; } = new EllipseData() { X = 150, Y = 50, Fill = Brushes.Gold };
        public EllipseData Myオリーブ丸Data { get; set; } = new EllipseData() { X = 100, Y = 150, Fill = Brushes.Olive };
        public EllipseData My緑丸Data { get; set; } = new EllipseData() { X = 100, Y = 100, Fill = Brushes.MediumAquamarine };

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;
        }
    }
}