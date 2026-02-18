using System.Collections.ObjectModel;
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
/*動作イメージ

RootNodes → ItemsControl → CollapsiblePanel で階層表示
ヘッダークリックで開閉
データ構造を変えると UI も自動更新（ObservableCollection）


💡 この作り方をさらに発展させれば、TreeView の完全カスタム版 や アコーディオンメニュー を MVVM パターンで構築できます。

もし希望があれば、この階層型 CollapsiblePanel を 再帰的 DataTemplate だけでシンプルに書くバージョン にして、コードビハインドをほぼゼロにする方法も作れますが、それも作りますか？
そうすれば XAML だけで階層UIが完結します。
*/

namespace _20260218_HeaderItemControl2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Node> RootNodes { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            RootNodes =
            [
                new() {
                    Name = "フルーツ",
                    Children =
                    {
                        new Node { Name = "りんご" },
                        new Node { Name = "バナナ" },
                        new Node
                        {
                            Name = "柑橘類",
                            Children =
                            {
                                new Node { Name = "みかん" },
                                new Node { Name = "オレンジ" }
                            }
                        }
                    }
                },
                new Node
                {
                    Name = "野菜",
                    Children =
                    {
                        new Node { Name = "にんじん" },
                        new Node { Name = "じゃがいも" }
                    }
                }
            ];

            DataContext = this;

        }
    }
}