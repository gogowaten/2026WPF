using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20260224_Json
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //// --- データの準備 ---
            //var list = new ObservableCollection<Item>
            //{
            //    new TextBlockItem(10, 20, "こんにちはWPF"),
            //    new RectangleItem(50, 50, Brushes.Blue.ToString(), 100, 200),
            //    new Items(0, 0) // 子要素を持つグループ
            //};

            //// --- シリアライズ (保存) ---
            //var options = new JsonSerializerOptions
            //{
            //    WriteIndented = true // 見やすく整形する
            //};
            //string json = JsonSerializer.Serialize(list, options);

            //// 確認用にコンソール表示
            //Console.WriteLine(json);

           // ---デシリアライズ(復元)-- -
           // Itemはabstractですが、JsonDerivedType属性のおかげで適切な派生クラスで復元されます
           //var deserializedList = JsonSerializer.Deserialize<ObservableCollection<Item>>(json, options);


            Items items = new(50, 120);
            items.Children.Add(new TextBlockItem(10, 10, "ゆっくりしていってね！！！"));
            items.Children.Add(new RectangleItem(100, 100, Brushes.Blue.ToString(), 100, 200));

            var options = new JsonSerializerOptions();
            var json2 = JsonSerializer.Serialize(items,options);
            var result = JsonSerializer.Deserialize<Items>(json2, options);

            //// 1. データの作成（入れ子構造）
            //Items rootItems = new(50, 120);
            //rootItems.Children.Add(new TextBlockItem(10, 10, "ゆっくりしていってね！！！"));
            //rootItems.Children.Add(new RectangleItem(100, 100, Brushes.Blue.ToString(), 100, 200));

            //// 2. シリアライズ
            //var options = new JsonSerializerOptions
            //{
            //    WriteIndented = true,
            //    PropertyNameCaseInsensitive = true
            //};
            //string json = JsonSerializer.Serialize(rootItems, options);

            //// 3. デシリアライズ
            //// Items型として直接復元できます
            //Items deserializedItems = JsonSerializer.Deserialize<Items>(json, options);

            //// 確認
            //Console.WriteLine($"子要素の数: {deserializedItems.Children.Count}");
        }
    }
}