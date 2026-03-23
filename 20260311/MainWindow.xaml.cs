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

namespace _20260311
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public RootData MyDataVM { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dc = MyAAA.DataContext;
            //MyTTText.Text = "acb";
            foreach (var item in MyDataVM.DataList)
            {
                if(item is TextBlockData txt)
                {
                    //txt.Text = "ゆっくりしていってね！！！";
                    txt.FontSize++;
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            if (MyDataVM.RootData is RootData root)
            {
                if(root.CurrentItem is Data data)
                {

                    data.Z++;
                }
            }
        }

        
    }
}