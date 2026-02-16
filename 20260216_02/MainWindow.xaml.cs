using System.Text;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20260216_02
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private MainViewModel VM
        {
            get
            {
                return (MainViewModel)DataContext;
            }
        }

        private void AddNodeAt(double x, double y)
        {
            VM.AddNodeCommand.Execute(new Point(x, y));
        }
    }
}