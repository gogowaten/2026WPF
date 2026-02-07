using CommunityToolkit.Mvvm.ComponentModel; // これが必要
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
using System.IO;
using CommunityToolkit.Mvvm.Input; // コマンド用


namespace _20260206_CommunityToolkit
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


    // 1. partialクラスにする（自動生成コードと合体するため）
    // 2. ObservableObjectを継承する
    public partial class TextBoxViewModel : ObservableObject
    {

        // [ObservableProperty] を付けると、大文字で始まる「Text」プロパティが自動生成される
        // [NotifyCanExecuteChangedFor] を使うと、
        // Textが変わった瞬間にコマンドの「押せる・押せない」を再判定してくれる
        // 実行したいメソッド名にCommandを付け足したものを指定する
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ComplexActionCommand))]
        public string _text = "";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ComplexActionCommand))]
        private double _fontSize = 12;

        [ObservableProperty]
        private string _fontWeight = "Normal";

        [ObservableProperty]
        private string _foreground = Colors.Black.ToString();

        [RelayCommand(CanExecute =nameof(CanExecuteComplexAction))]
        private void ComplexAction()
        {
            Foreground = Colors.Magenta.ToString();
        }
        
        // 複数条件での実行
        // 
        private bool CanExecuteComplexAction()
        {
            bool hasText = !string.IsNullOrWhiteSpace(Text);
            bool isLargeFont = FontSize > 10;
            return hasText && isLargeFont;
        }


        // プロパティが変更された後に呼ばれるメソッド（命名規則：Onプロパティ名Changed）
        partial void OnTextChanged(string value) => SaveSettings();
        partial void OnFontSizeChanged(double value) => SaveSettings();
        partial void OnFontWeightChanged(string value) => SaveSettings();
        partial void OnForegroundChanged(string value) => SaveSettings();

        // [RelayCommand] を付けると、裏で「ResetTextCommand」が自動生成される
        //[RelayCommand]
        // これのCanExeuteに判定メソッドを指定すると、それに合わせてボタンの有効無効が切り替わる
        // 1. コマンドに判定メソッドを指定する
        [RelayCommand(CanExecute = nameof(CanResetText))]
        private void ResetText()
        {
            Text = ""; // テキストを空にする
            FontSize = 12; // フォントサイズを初期化
                           // 保存処理はプロパティ側の変更通知で自動実行される
        }

        // 2. 判定ロジック（trueを返せばボタンが押せる）
        private bool CanResetText()
        {
            // 文字列が空または空白でない場合のみ、リセットボタンを有効にする
            return !string.IsNullOrWhiteSpace(Text);
        }

        // 引数を受け取るコマンドも作成可能
        [RelayCommand]
        private void ChangeFontSize(string size)
        {
            if (double.TryParse(size, out double newSize))
            {
                FontSize = newSize;
            }
        }

        // 非同期コマンド（AsyncRelayCommand）でファイルに保存
        [RelayCommand]
        private async Task SaveToFileAsync()
        {
            // ボタン連打防止（実行中は自動的にCanExecuteがfalseになる）
            await Task.Run(() => { SettingsManager.Save(this); });
            // 完了後の通知などはここ
        }

        
        
        

        private void SaveSettings()
        {
            SettingsManager.Save(this);
        }
    }


    public class TextBoxSettings
    {
        public string Text { get; set; } = string.Empty;
        public double FontSize { get; set; } = Application.Current.MainWindow.FontSize;
        public string FontFamily { get; set; } = Application.Current.MainWindow.FontFamily.ToString();
        public string Foreground { get; set; } = Colors.Black.ToString(); // Color名やHexコードで保存
        public string Background { get; set; } = Colors.Transparent.ToString(); // Color名やHexコードで保存
        public double MarginLeft { get; set; }
        public double MarginTop { get; set; }
        public double MarginRight { get; set; }
        public double MarginBottom { get; set; }
        public string FontWeight { get; set; } = "Normal";

    }

    public class SettingsManager
    {
        private static string filePath = "textbox_settings.json";

        public static TextBoxSettings? Load()
        {
            //string filePath = "textbox_settings.json";
            if (!File.Exists(filePath)) return null;

            try
            {
                string jsonString = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<TextBoxSettings>(jsonString);
            }
            catch
            {
                // ファイルが壊れている場合などのエラーハンドリング
                return null;
            }
        }


        // 保存処理
        public static void Save(TextBox textBox)
        {
            var settings = new TextBoxSettings
            {
                Text = textBox.Text,
                FontSize = textBox.FontSize,
                FontFamily = textBox.FontFamily.ToString(),
                Foreground = textBox.Foreground.ToString(),
                MarginLeft = textBox.Margin.Left,
                MarginTop = textBox.Margin.Top,
                MarginRight = textBox.Margin.Right,
                MarginBottom = textBox.Margin.Bottom,
                FontWeight = textBox.FontWeight.ToString(),
            };

            string jsonString = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, jsonString);
        }

        // 保存処理
        public static void Save(TextBoxViewModel vm)
        {
            var settings = new TextBoxSettings
            {
                Text = vm.Text,
                //FontSize = textBox.FontSize,
                //FontFamily = textBox.FontFamily.ToString(),
                Foreground = vm.Foreground.ToString(),
                //Background = vm.Background.ToString(),
                //MarginLeft = textBox.Margin.Left,
                //MarginTop = textBox.Margin.Top,
                //MarginRight = textBox.Margin.Right,
                //MarginBottom = textBox.Margin.Bottom,
                //FontWeight = textBox.FontWeight.ToString(),
            };

            string jsonString = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, jsonString);
        }


        // 読み込み処理
        public static void Load(System.Windows.Controls.TextBox textBox)
        {
            if (!File.Exists(filePath)) return;

            string jsonString = File.ReadAllText(filePath);
            var settings = JsonSerializer.Deserialize<TextBoxSettings>(jsonString);

            if (settings != null)
            {
                textBox.Text = settings.Text;
                textBox.FontSize = settings.FontSize;
                textBox.FontFamily = new FontFamily(settings.FontFamily);
                textBox.Foreground = (Brush)new BrushConverter().ConvertFromString(settings.Foreground);
                textBox.Background = (Brush)new BrushConverter().ConvertFromString(settings.Background);
                textBox.Margin = new Thickness(settings.MarginLeft, settings.MarginTop, settings.MarginRight, settings.MarginBottom);
                // 行末の！はnull免除演算子で、nullじゃない確信があるときにだけ使う
                textBox.FontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(settings.FontWeight)!;
            }
        }

        // 読み込み処理
        public static void Load(TextBoxViewModel vm)
        {
            if (!File.Exists(filePath)) return;

            string jsonString = File.ReadAllText(filePath);
            var settings = JsonSerializer.Deserialize<TextBoxSettings>(jsonString);

            if (settings != null)
            {
                //vm.Text = settings.Text;
                //vm.FontSize = settings.FontSize;
                //vm.FontFamily = new FontFamily(settings.FontFamily);
                //vm.Foreground = (Brush)new BrushConverter().ConvertFromString(settings.Foreground);
                //vm.Margin = new Thickness(settings.MarginLeft, settings.MarginTop, settings.MarginRight, settings.MarginBottom);
                //// 行末の！はnull免除演算子で、nullじゃない確信があるときにだけ使う
                //vm.FontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(settings.FontWeight)!;
            }
        }

    }


}

