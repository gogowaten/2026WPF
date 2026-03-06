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

namespace _20260306_DaraTemplateRectangle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public RectangleData MyRectangleData { get; set; }
        public RectangleVM MyRectangleVM { get; set; } = new();
        public MainWindow()
        {
            InitializeComponent();
            MyRectangleData = new(100, 50);
            this.DataContext = this;
        }
    }
}