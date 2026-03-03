using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20260301
{

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

        #region 旧式OnMouseDown


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
        //        // 2. EditorService に選択を依頼
        //        bool isCtrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        //        Service.Select(clickedData, isCtrl);

        //        // クリックされたことを他の要素に伝えない（バブリング停止）
        //        e.Handled = true;

        //        // フォーカスを当ててキーボード入力を受け取れるようにする
        //        this.Focus();
        //    }
        //    else
        //    {
        //        // 背景をクリックした場合は選択解除
        //        Service.ClearSelection();
        //    }

        //}
        #endregion 旧式

        //protected override void OnMouseDown(MouseButtonEventArgs e)
        //{
        //    base.OnMouseDown(e);

        //    // OriginalSource（クリックされた実体）から親へ辿り、IsSelectableがTrueのものを探す
        //    DependencyObject? d = e.OriginalSource as DependencyObject;
        //    FrameworkElement? selectableElement = null;

        //    while (d != null && d != this)
        //    {
               
        //        if (d is FrameworkElement fe && EditorBehavior.GetIsSelectable(fe))
        //        {   
        //            selectableElement = fe;
        //            break;
        //        }
        //        d = VisualTreeHelper.GetParent(d);
        //    }

        //    if (selectableElement != null && selectableElement.DataContext is Data clickedData)
        //    {
        //        bool isCtrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        //        Service.Select(clickedData, isCtrl);
        //        e.Handled = true;
        //    }
        //    else { Service.ClearSelection(); }
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


    public class GroupItemsControl : ItemsControl
    {
        static GroupItemsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GroupItemsControl), new FrameworkPropertyMetadata(typeof(GroupItemsControl)));
        }
        public GroupItemsControl()
        {
            var ics = this.ItemContainerStyle;
            //var icsre = ics.Resources;
            //var setters = ics.Setters;
        }
    }

    public class Node : ItemsControl
    {
        static Node()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Node), new FrameworkPropertyMetadata(typeof(Node)));
        }
        public Node()
        {

        }
    }

    public class NodeReCanvas : Control
    {
        //public ObservableCollection<Data> MyDatas { get; set; } = [];
        public RootData MyRootData { get; set; } = new();
        public ItemsControl MyItems { get; set; } = null!;




        static NodeReCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NodeReCanvas), new FrameworkPropertyMetadata(typeof(NodeReCanvas)));
        }
        public NodeReCanvas()
        {
            this.DataContext = this;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("PART_ItemsControl") is ItemsControl ic)
            {
                MyItems = ic;
            }
            else { throw new ApplicationException("not found template"); }
        }
    }

}
