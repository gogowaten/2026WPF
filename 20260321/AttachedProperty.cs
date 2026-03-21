using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace _20260321
{

    // TextBlockなどWidthとHeightがNaNの要素からサイズを取得するときに使う添付プロパティ
    // 取得する値は、たぶんActualHeight、ActualWidth
    /*                   local:SizeObserver.Observe="True"
                         local:SizeObserver.ObservedWidth="{Binding Width, Mode=OneWayToSource}"
                         local:SizeObserver.ObservedHeight="{Binding Height, Mode=OneWayToSource}"/>
    みたいに使う、BindingModeはOneWayToSourceで取得用になる
     */
    public static class SizeObserver
    {
        // サイズ変更監視のオンオフ切り替え
        public static readonly DependencyProperty ObserveProperty =
            DependencyProperty.RegisterAttached("Observe", typeof(bool), typeof(SizeObserver), new FrameworkPropertyMetadata(OnObserveChanged));
        public static bool GetObserve(DependencyObject obj) => (bool)obj.GetValue(ObserveProperty);
        public static void SetObserve(DependencyObject obj, bool value) => obj.SetValue(ObserveProperty, value);

        // サイズ変更イベントの購読と処理の付け外し
        private static void OnObserveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement fe)
            {
                if ((bool)e.NewValue)
                {
                    fe.SizeChanged += Fe_SizeChanged;
                    //SetObservedWidth(fe, fe.ActualWidth);// これだと初回読み込み時のサイズが常に0になってしまう

                    //初回のみLoadedイベントをフックして、レンダリング後のサイズを取得する
                    fe.Loaded += Fe_Loaded;
                }
                else
                {
                    fe.SizeChanged -= Fe_SizeChanged;
                }
            }
        }

        private static void Fe_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe)
            {
                // 読み込みが完了したので、この時点のActualWidthを反映
                SetObservedWidth(fe, fe.ActualWidth);
                SetObservedHeight(fe, fe.ActualHeight);
                // 一度取得すれば用済みなので、イベントを外してメモリリークを防止
                fe.Loaded -= Fe_Loaded;
            }
        }


        // サイズ変更時の処理、新たなサイズをObservedWidthとHeightそれぞれに記録
        private static void Fe_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width != 0 && e.NewSize.Height != 0)
            {
                if (sender is FrameworkElement fe)
                {
                    SetObservedWidth(fe, e.NewSize.Width);
                    SetObservedHeight(fe, e.NewSize.Height);
                    //SetObservedHeight(fe, fe.DesiredSize.Height); // これで問題ない？

                }
            }
        }

        // Width記録用
        public static readonly DependencyProperty ObservedWidthProperty =
            DependencyProperty.RegisterAttached("ObservedWidth", typeof(double), typeof(SizeObserver), new PropertyMetadata(0.0));
        public static double GetObservedWidth(DependencyObject obj) => (double)obj.GetValue(ObservedWidthProperty);
        public static void SetObservedWidth(DependencyObject obj, double value) => obj.SetValue(ObservedWidthProperty, value);

        // Height記録用
        public static readonly DependencyProperty ObservedHeightProperty =
            DependencyProperty.RegisterAttached("ObservedHeight", typeof(double), typeof(SizeObserver), new PropertyMetadata(0.0));
        public static double GetObservedHeight(DependencyObject obj) => (double)obj.GetValue(ObservedHeightProperty);
        public static void SetObservedHeight(DependencyObject obj, double value) => obj.SetValue(ObservedHeightProperty, value);

        //// --- 実行したいコマンド用のプロパティ ---
        //public static readonly DependencyProperty WidthChangedCommandProperty =
        //    DependencyProperty.RegisterAttached("WidthChangedCommand", typeof(ICommand), typeof(SizeObserver), new PropertyMetadata(null));
        //public static ICommand GetWidthChangedCommand(DependencyObject obj) => (ICommand)obj.GetValue(WidthChangedCommandProperty);
        //public static void SetWidthChangedCommand(DependencyObject obj, ICommand value) => obj.SetValue(WidthChangedCommandProperty, value);

    }

    internal class AttachedProperty
    {
    }
}