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

namespace _20260304
{
    
    public class CustomControl1 : Control
    {
        static CustomControl1()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomControl1), new FrameworkPropertyMetadata(typeof(CustomControl1)));
        }
    }

    public class CanvasEditor : ItemsControl
    {
        // 本来はDIやViewModel経由が望ましいが、簡略化のため一旦保持
        //public EditorService Service { get; } = new();

        static CanvasEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CanvasEditor), new FrameworkPropertyMetadata(typeof(CanvasEditor)));
        }
        public CanvasEditor()
        {

        }

        //protected override void OnMouseDown(MouseButtonEventArgs e)
        //{
        //    base.OnMouseDown(e);

        //    // 1. クリックされた要素を特定する
        //    // VisualTreeを遡って、Dataオブジェクトを持っているFrameworkElementを探す
        //    var hitResult = VisualTreeHelper.HitTest(this, e.GetPosition(this));
        //    var element = hitResult?.VisualHit as FrameworkElement;

        //    // CanvasElementControl または DataContextにData型を持つ要素を探す
        //    while (element != null && element.DataContext is not Data)
        //    {
        //        element = VisualTreeHelper.GetParent(element) as FrameworkElement;
        //    }

        //    if (element != null && element.DataContext is Data clickedData)
        //    {
        //        bool isCtrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        //        Service.Select(clickedData, isCtrl);

        //        e.Handled = true;

        //        this.Focus();
        //    }
        //    else { Service.ClearSelection(); }

        //    //if (element != null && element.DataContext is Data clickedData)
        //    //{
        //    //    // 2. EditorService に選択を依頼
        //    //    bool isCtrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        //    //    Service.Select(clickedData, isCtrl);

        //    //    // クリックされたことを他の要素に伝えない（バブリング停止）
        //    //    e.Handled = true;

        //    //    // フォーカスを当ててキーボード入力を受け取れるようにする
        //    //    this.Focus();
        //    //}
        //    //else
        //    //{
        //    //    // 背景をクリックした場合は選択解除
        //    //    Service.ClearSelection();
        //    //}

        //}
    }

    public class CanvasElementControl : ContentControl
    {
        static CanvasElementControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CanvasElementControl), new FrameworkPropertyMetadata(typeof(CanvasElementControl)));
        }
        public CanvasElementControl()
        {

        }
    }
}
