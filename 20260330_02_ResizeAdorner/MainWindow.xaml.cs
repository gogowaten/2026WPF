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

namespace _20260330_02_ResizeAdorner
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

      
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            AddResizeAdorner(TargetRect);
        }

        /// <summary>
        /// 指定された UI 要素に、サイズ変更アドーナがまだ存在しない場合に、追加します。
        /// </summary>
        /// <remarks>このメソッドは、指定された要素にサイズ変更アドーナが既に存在するかどうかを確認してから、新しいアドーナを追加します。
        /// 要素はビジュアルツリーの一部であり、関連付けられたアドーナレイヤーを持っている必要があります。
        /// </remarks>
        /// <param name="element">サイズ変更アドーナを追加する UI 要素。null は指定できません。</param>
        public void AddResizeAdorner(UIElement element)
        {
            if (AdornerLayer.GetAdornerLayer(element) is AdornerLayer layer)
            {
                var adorners = layer.GetAdorners(element);
                if (adorners is null || adorners.Length == 0)
                {
                    layer.Add(new ResizeAdorner(element));
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            RemoveResizeAdorner(TargetRect);
        }

        /// <summary>
        /// 指定された UI 要素から、すべてのアドナーを削除します。
        /// </summary>
        /// <remarks>このメソッドは、指定された要素に関連付けられたすべてのアドナーを検索して削除します。
        /// アドナーが見つからない場合は、false を返します。操作を成功させるには、要素がビジュアルツリーの一部である必要があります。
        /// </remarks>
        /// <param name="element">アドナーを削除する UI 要素。null は指定できません。</param>
        /// <returns>削除されたAdornerの個数を返す</returns>
        public int RemoveResizeAdorner(UIElement element)
        {
            int result = 0;
            if (AdornerLayer.GetAdornerLayer(element) is AdornerLayer layer)
            {
                if (layer.GetAdorners(element) is Adorner[] ados)
                {
                    foreach (Adorner item in ados)
                    {
                        layer.Remove(item);
                        result++;
                    }
                }
            }
            return result;
        }
    }
}