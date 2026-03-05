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

namespace _20260305
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public RectangleVM MyRectangleVM { get; set; } = new();
        public GroupVM MyGroupVM { get; set; } = new();
        public RectangleGroupVM MyRectangleGroupVM { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();
            //this.DataContext = this;
        }
    }
}