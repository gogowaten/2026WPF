using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

// サイズ変更時にActualWidthとActualHeightを自動取得したいときに使う添付プロパティ
namespace _20260316_TextBlockSizeを添付プロパティで
{
    /// <summary>
    /// WPF 要素のサイズ変更を監視および追跡するための添付プロパティを提供します。
    /// プロパティを対象要素に添付することで、幅と高さの更新を監視できます。
    /// </summary>
    /// <remarks>添付プロパティを使用して、任意の WPF 要素のサイズ監視を有効にします。「Observe」
    /// プロパティを<see langword = "true" /> に設定すると、要素の幅と高さが、サイズ変更のたびに「ObserveWidth」プロパティと「ObserveHeight」プロパティで自動的に更新されます。
    /// これは、カスタムイベントハンドラーを使用せずに、XAML でサイズ変更に反応したり、バインドしたりする必要がある場合に役立ちます。</remarks>
    public static class SizeObserver
    {
        // true で監視のオンオフ切り替え
        public static readonly DependencyProperty ObserveProperty =
            DependencyProperty.RegisterAttached("Observe", typeof(bool), typeof(SizeObserver), new FrameworkPropertyMetadata(OnObserveChanged));
        public static bool GetObserve(DependencyObject obj) => (bool)obj.GetValue(ObserveProperty);
        public static void SetObserve(DependencyObject obj, bool value) => obj.SetValue(ObserveProperty, value);


        // ActualWidth用
        public static readonly DependencyProperty ObserveWidthProperty =
            DependencyProperty.RegisterAttached("ObserveWidth", typeof(double), typeof(SizeObserver), new PropertyMetadata(0.0));
        //public static double GetObserveWidth(DependencyObject obj) => (double)obj.GetValue(ObserveWidthProperty);
        public static void SetObserveWidth(DependencyObject obj, double value) => obj.SetValue(ObserveWidthProperty, value);

        // ActualHeight用
        public static readonly DependencyProperty ObserveHeightProperty =
            DependencyProperty.RegisterAttached("ObserveHeight", typeof(double), typeof(SizeObserver), new PropertyMetadata(0.0));
        //public static double GetObserveHeight(DependencyObject obj) => (double)obj.GetValue(ObserveHeightProperty);
        public static void SetObserveHeight(DependencyObject obj, double value) => obj.SetValue(ObserveHeightProperty, value);



        // 監視オンでSizeChangedイベント購読、サイズ変更時にそれぞれのプロパティを書き換える
        // 要素のActualWidthとActualHeightをsetする
        private static void OnObserveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement fe)
            {
                if ((bool)e.NewValue)
                {
                    fe.SizeChanged += Fe_SizeChanged;
                }
                else
                {
                    fe.SizeChanged -= Fe_SizeChanged;
                }
            }
        }

        private static void Fe_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is FrameworkElement fe)
            {
                SetObserveWidth(fe, fe.ActualWidth);
                SetObserveHeight(fe, fe.ActualHeight);
            }
        }
    }
}