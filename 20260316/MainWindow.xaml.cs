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


/* DesiredSizeの変更を自動取得したい
 * 王道はSizeChangedイベントで取得するのが良いけど、TextBlockがGridなどの全体に広がるパネルに置かれているときは
 * パネルサイズより大きくなったときしかSizeChangedが発生しないのが欠点、しかしこれは
 * TextBlockをCanvasなどのパネルに置くなどで解決できる。
 * SizeChangedイベントの他ではLayoutUpdateイベントでもできる。これの欠点はLayoutUpdateイベントが頻繁に発生するところで
 * 対象の要素が増えると負荷が気になるかも？
 */
namespace _20260316
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public TextBlockData MyData { get; set; } = new();
        public MainWindow()
        {
            InitializeComponent();
            MyData.Text = "ゆっくりしていってね！！！";
            this.DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //MyData.FontSize++;
            Canvas.SetTop(MyTTT, Canvas.GetTop(MyTTT) + 10);
            var neko = MyTTT.DesiredSize;
        }
    }
}