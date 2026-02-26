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

namespace _20260226_DataTemplate_CustomControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Item> ItemsList { get; set; } = [];
        public MainWindow()
        {
            InitializeComponent();
            PrepareData();
            this.DataContext = this;
        }

        private void PrepareData()
        {

            ItemsList.Add(new TextBlockItem(10, 10, "ゆっくりしていってね！！！"));
            ItemsList.Add(new RectangleItem(150, 50, 100, 50) { R = 255, G = 0, B = 0 });

            var group = new Items(60, 150);
            group.Children.Add(new RectangleItem(0, 0, 89, 90) { R = 0, G = 0, B = 255 });
            group.Children.Add(new TextBlockItem(10, 90, "これはグループ内です"));
            ItemsList.Add(group);

        }
    }
}