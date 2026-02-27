using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace _20260227
{

    public class NodeContainer : ListBox
    {   
        static NodeContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NodeContainer), new FrameworkPropertyMetadata(typeof(NodeContainer)));
        }

        //public ObservableCollection<Data> Datas { get; set; } = [];

        //public Datas TargetNodes
        //{
        //    get { return (Datas)GetValue(TargetNodesProperty); }
        //    set { SetValue(TargetNodesProperty, value); }
        //}
        //public static readonly DependencyProperty TargetNodesProperty =
        //    DependencyProperty.Register(nameof(TargetNodes), typeof(Datas), typeof(NodeContainer), new PropertyMetadata(null));

        public NodeContainer()
        {
            //DataContext = Nodes;
            //ItemsSource = Datas;
            //DataContext = this;

            //SetBinding(ItemsSourceProperty, new Binding() { Source = this, Path = new PropertyPath(TargetNodesProperty) });
        }

    }

 

}
