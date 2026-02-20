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
using System.Windows.Controls.Primitives;


namespace _20260219
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

        private void Thumb_DragDelta2(object sender, DragDeltaEventArgs e)
        {
            if (sender is Thumb t)
            {
                Canvas.SetLeft(t, Canvas.GetLeft(t) + e.HorizontalChange);
                Canvas.SetTop(t, Canvas.GetTop(t) + e.VerticalChange);
            }
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            // パネルが自動リサイズモードでない場合のみ、サイズを手動更新する
            if (MyPanel != null && !MyPanel.IsAutoSize)
            {
                // 現在の ActualWidth/Height をベースにドラッグ量を加算
                // 最初は Width/Height が NaN なので ActualWidth を使うのがコツです
                double newWidth = (double.IsNaN(MyPanel.Width) ? MyPanel.ActualWidth : MyPanel.Width) + e.HorizontalChange;
                double newHeight = (double.IsNaN(MyPanel.Height) ? MyPanel.ActualHeight : MyPanel.Height) + e.VerticalChange;

                // 最小サイズの制限（小さくなりすぎないように）
                if (newWidth > 30) MyPanel.Width = newWidth;
                if (newHeight > 30) MyPanel.Height = newHeight;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(sender is FrameworkElement ui)
            {
                ui.Width += 10;
                ui.Height += 10;
            }
        }
    }
}