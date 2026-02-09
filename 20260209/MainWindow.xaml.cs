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

namespace _20260209
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MyCanvas.MouseDown += (s, e) =>
            {
                if (e.Source is Canvas canvas)
                {
                    foreach (var child in canvas.Children.OfType<DraggableRectangle>())
                    {
                        child.IsSelected = false;
                    }
                }
            };
        }
    }
}