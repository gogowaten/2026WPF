using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20260206_fairu_ni_hozon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new TextBoxViewModel();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
           SettingsManager.Save(MyTextBox);
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.Load(MyTextBox2);
        }

        private void ForegroundRed_Click(object sender, RoutedEventArgs e)
        {
            MyTextBox.Foreground = new SolidColorBrush(Color.FromRgb(143, 234, 0));
            //var data = (TextBoxViewModel)MyTextBox.DataContext;
            //data.Foreground = Colors.Yellow.ToString();
        }
    }
}