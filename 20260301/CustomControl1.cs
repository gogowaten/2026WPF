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
        static CanvasEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CanvasEditor), new FrameworkPropertyMetadata(typeof(CanvasEditor)));
        }
        public CanvasEditor()
        {

        }
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
            if(GetTemplateChild("PART_ItemsControl") is ItemsControl ic)
            {
                MyItems = ic;
            }
            else { throw new ApplicationException("not found template"); }
        }
    }

}
