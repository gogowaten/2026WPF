using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20260224
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            JsonSerializerOptions options = new() { WriteIndented = true };
            RectangleItem rectangleItem = new(10, 20, 40, 50);
            var jj = JsonSerializer.Serialize(rectangleItem, options);


            GroupItem items = new(10, 20);
            // Children.Add は使わないで、AddChildを使う
            //items.Children.Add(new TextBlockItem(10, 20, "TestAAA"));

            items.AddChild(new TextBlockItem(10, 20, "TestAAA"));
            items.AddChild(new RectangleItem(10, 80, 100, 50));
            GroupItem root = new(0, 0);
            root.AddChild(items);
            root.AddChild(new TextBlockItem(100, 20, "TestBBB"));
            root.AddChild(root);

            
            var json = JsonSerializer.Serialize(root, options);
            GroupItem? result = JsonSerializer.Deserialize<GroupItem>(json, options);

        }
    }
}