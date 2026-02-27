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

namespace _20260227
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Datas RootData { get; set; } = new(0, 0);
        public MainWindow()
        {
            InitializeComponent();

            CreateData();
            this.DataContext = this;
        }

        
        private void CreateData()
        {
            RootData = new(0, 0);
            RootData.AddNodeData(new TextBlockData(0, 0, "AAA"));

            Datas group = new(100, 100);
            group.AddNodeData(new RectangleData(0, 0, 100, 50));
            group.AddNodeData(new RectangleData(50, 150, 100, 50));
            RootData.AddNodeData(group);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //MyTextYukkuri.Text = "ゆっくりしていってね！！！";
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //MyReCanvas.UpdateSize();

        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            //var neko = MyRootNode.DataContext;
        }
    }
}