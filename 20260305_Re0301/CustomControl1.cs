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

namespace _20260305_Re0301
{
    
   
    public class CanvasEditor : ItemsControl
    {

        public EditorService Service
        {
            get { return (EditorService)GetValue(ServiceProperty); }
            set { SetValue(ServiceProperty, value); }
        }
        public static readonly DependencyProperty ServiceProperty =
            DependencyProperty.Register(nameof(Service), typeof(EditorService), typeof(CanvasEditor));


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





}
