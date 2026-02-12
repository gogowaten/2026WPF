using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace _20260212_Behaviors
{

    // マウスが入ったら色が変わるビヘイビア、Border要素専用
    public class MouseOverBehavior : Behavior<Border>
    {
        // アタッチされた時の処理
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseEnter += OnMouseEnter;
            AssociatedObject.MouseLeave += OnMouseLeave;
        }

        // デタッチ(切り離し)された時の処理(メモリリーク防止)
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseLeave -= OnMouseLeave;
            AssociatedObject.MouseEnter -= OnMouseEnter;
        }


        private void OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            AssociatedObject.Background = Brushes.LightBlue;
        }

        private void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            AssociatedObject.Background = Brushes.Transparent;
        }
    }
}
