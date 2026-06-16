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


        public int MyX
        {
            get { return (int)GetValue(MyXProperty); }
            set { SetValue(MyXProperty, value); }
        }
        public static readonly DependencyProperty MyXProperty =
            DependencyProperty.Register(nameof(MyX), typeof(int), typeof(MainWindow), new PropertyMetadata(0));

        public int MyY
        {
            get { return (int)GetValue(MyYProperty); }
            set { SetValue(MyYProperty, value); }
        }
        public static readonly DependencyProperty MyYProperty =
            DependencyProperty.Register(nameof(MyY), typeof(int), typeof(MainWindow), new PropertyMetadata(0));


        public double MyScale
        {
            get { return (double)GetValue(MyScaleProperty); }
            set { SetValue(MyScaleProperty, value); }
        }
        public static readonly DependencyProperty MyScaleProperty =
            DependencyProperty.Register(nameof(MyScale), typeof(double), typeof(MainWindow), new PropertyMetadata(0.0));

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

            var pos = Mouse.GetPosition(MyRect);
            var bpos = Mouse.GetPosition(MyScroll);

            var rectmouse = MyRect.IsMouseOver;
        }

        private void MyRect_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(MyScroll);
            var neko = e.GetPosition(MyRect);

            var rectmouse = MyRect.IsMouseOver;
        }

        private void MyRect_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void UpdatePos()
        {
            
        }
    }
}