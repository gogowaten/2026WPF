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
            //RootData.UpdateBounds();
            this.DataContext = this;
        }


        //private void CreateData()
        //{
        //    RootData = new(0, 0);
        //    RootData.AddNodeData(new TextBlockData(0, 0, "AAA"));

        //    Datas group = new(100, 100);
        //    group.AddNodeData(new RectangleData(0, 0, 100, 50));
        //    group.AddNodeData(new RectangleData(50, 150, 100, 50));
        //    //group.UpdateBounds(); // 追加
        //    RootData.AddNodeData(group);
        //}

        private void CreateData()
        {
            // 1. まず RootData を作る（この時点で CollectionChanged の監視が始まる）
            RootData = new Datas(0, 0);

            // 2. group を作る
            Datas group = new Datas(100, 100);

            // 3. group に要素を入れる 
            // (このタイミングで group.Width/Height が自動更新されるように OnNodePropertyChanged が動くはず)
            group.AddNodeData(new RectangleData(0, 0, 100, 50));
            group.AddNodeData(new RectangleData(50, 150, 100, 50));

            // 4. 最後に RootData に追加する
            RootData.AddNodeData(new TextBlockData(0, 0, "AAA"));
            RootData.AddNodeData(group);
            RootData.UpdateBounds();
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