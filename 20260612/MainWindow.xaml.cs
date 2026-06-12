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

namespace _20260612
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GeneralTransform neko = MyScroll.TransformToAncestor(this);
            //var inu = MyScroll.TransformToDescendant(this); // 子孫じゃない
            var tako = MyScroll.TransformToVisual(this);
            //GeneralTransform neko2 = MyScroll.TransformToAncestor(MyRect); // 祖先じゃない
            var inu2 = MyScroll.TransformToDescendant(MyRect);
            var tako2 = MyScroll.TransformToVisual(MyRect);

            var inuPoint = inu2.Transform(new Point());

            var inu3 = MyRect.TransformToVisual(MyScroll);
            var tako3 = MyRect.TransformToAncestor(MyScroll);

        }

        private void MyRect_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(MyScroll);
            var neko = e.GetPosition(MyRect);
        }
    }
}