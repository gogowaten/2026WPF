using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


// TextBlockのサイズ取得に使う
namespace _20260224
{
    // 添付プロパティ
    // trueの場合にSizeChangedイベントを購読監視することになる
    // trueになった瞬間(xamlの方にtrueと書いてあるから起動時)に動く、そのときにOnObserveChangedが処理される、その処理の中にはSizeChangedイベントの購読登録があるので、購読開始になる、それによってサイズ変更があるたびにその中に書いてある処理のサイズ変更がされる
    public static class SizeObserver
    {
        //public static bool GetObserve(DependencyObject obj)
        //{
        //    return (bool)obj.GetValue(ObserveProperty);
        //}

        public static void SetObserve(DependencyObject obj, bool value)
        {
            obj.SetValue(ObserveProperty, value);
        }

        public static readonly DependencyProperty ObserveProperty =
            DependencyProperty.RegisterAttached("Observe", typeof(bool), typeof(SizeObserver), new FrameworkPropertyMetadata(OnObserveChanged));

        // サイズ変更時に更新された値をItemに書き戻す
        private static void OnObserveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement fe)
            {
                // 古い購読を解除(二重登録防止)
                fe.SizeChanged -= FrameworkElement_SizeChanged;

                // 新しい値がtrueなら購読開始
                if ((bool)e.NewValue)
                {
                    fe.SizeChanged += FrameworkElement_SizeChanged;
                }
            }
        }

        // 更新されたサイズをItemのプロパティに入れる
        private static void FrameworkElement_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            if (sender is FrameworkElement fe && fe.DataContext is TextBlockItem item)
            {
                item.Width = args.NewSize.Width;
                item.Height = args.NewSize.Height;
                //item.Parent?.UpdateBounds();
            }
        }
    }
}
