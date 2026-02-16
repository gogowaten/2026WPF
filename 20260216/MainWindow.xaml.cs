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

namespace _20260216
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            NodeContainer child = new()
            {
                Width = 100,
                Height = 100,
                Background = Brushes.LightBlue,

            };
            DragBehavior.SetIsEnabled(child, true);
            RootNode.AddChild(child, new Point(50, 50));

            child = new()
            {
                Width = 100,
                Height = 100,
                Background = Brushes.LightGreen
            };
            DragBehavior.SetIsEnabled(child, true);
            RootNode.AddChild(child, new Point(150, 150));

            Rectangle r = new() { Width = 100, Height = 100, Fill = Brushes.Orange };
            DragBehavior.SetIsEnabled(r, true);
            RootNode.AddChild(r, new Point(200, 40));

        }
    }
}