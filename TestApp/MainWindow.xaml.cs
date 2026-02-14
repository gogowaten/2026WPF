using System.Diagnostics;
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

namespace TestApp
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

        private void MyNumericUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<int> e)
        {
            // e.OldValue と e.NewValue を使って、変更前後の値にアクセスできます
            //Debug.WriteLine($"値が {e.OldValue} から {e.NewValue} に変わりました！");
        }
    }
}