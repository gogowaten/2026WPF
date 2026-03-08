using System;
using System.Collections.Generic;
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

namespace _20260308_CustomContainer
{
    // 独自のコンテナを作る（MySelectorItem）
    //まずは、個々のアイテムを表示するための「器」を作ります。これに IsSelected プロパティを持たせます。
    public class MySelectorItem : ContentControl
    {
        // 選択状態を保持する依存関係プロパティ
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(MySelectorItem), new PropertyMetadata(false));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        // 静的コンストラクタで、デフォルトのスタイル（Generic.xaml）を適用
        static MySelectorItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MySelectorItem), new FrameworkPropertyMetadata(typeof(MySelectorItem)));
        }
    }
}
