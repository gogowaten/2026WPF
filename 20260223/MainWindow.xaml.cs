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

namespace _20260223
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //public ObservableCollection<Item> ItemData { get; set; } = [];
        public Item MyItem { get; set; }
        public MainWindow()
        {
            InitializeComponent();

            //MyItem = new("Root", 20, 10);
            //MyItem.Children.Add(new("111", 100, 10));
            //MyItem.Children.Add(new("222", 100, 50));
            //DataContext = MyItem;

        }
    }
}